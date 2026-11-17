using Nautilus.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // The pylons, the links between them (saved as id pairs) and the rail meshes stretched along each link
    public static class MonorailNetwork
    {
        public const float MaxSpan = 50f;
        public const float MinSpan = 4f;
        public const float MaxTurn = 60f;    // the drum hides mitres up to about 60 degrees
        public const float MaxSlope = 25f;
        public const float MinArmAngle = 40f; // a linked pylon's arm must stay this far off the rail, or the rail cuts the column

        private static readonly Dictionary<string, MonorailPylon> Pylons = new Dictionary<string, MonorailPylon>();
        private static readonly Dictionary<string, GameObject> Spans = new Dictionary<string, GameObject>();

        private static List<string> Links => Plugin.ModData.Links;

        private static string Key(string a, string b) => string.CompareOrdinal(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";

        private static IEnumerable<string> NeighbourIds(string id)
        {
            foreach (var link in Links)
            {
                var bar = link.IndexOf('|');
                if (bar < 0)
                    continue;
                var a = link.Substring(0, bar);
                var b = link.Substring(bar + 1);
                if (a == id)
                    yield return b;
                else if (b == id)
                    yield return a;
            }
        }

        public static int PylonCount => Pylons.Count;

        public static int LinkCount(MonorailPylon p) => NeighbourIds(p.Id).Count();

        public static bool IsLinked(MonorailPylon a, MonorailPylon b) => Links.Contains(Key(a.Id, b.Id));

        // The loaded pylon linked to p other than notThis (null at the end of the line)
        public static MonorailPylon Other(MonorailPylon p, MonorailPylon notThis)
        {
            foreach (var id in NeighbourIds(p.Id))
                if ((notThis == null || id != notThis.Id) && Pylons.TryGetValue(id, out var other) && other != null)
                    return other;
            return null;
        }

        private static float _lastRegister;

        public static void Register(MonorailPylon p)
        {
            _lastRegister = Time.time;
            Pylons[p.Id] = p;
            RebuildAround(p);
        }

        // The pylon unloads: its rails go with it, the links stay saved
        public static void Unregister(MonorailPylon p)
        {
            if (p.Id == null || !Pylons.TryGetValue(p.Id, out var known) || known != p)
                return;
            Pylons.Remove(p.Id);
            foreach (var id in NeighbourIds(p.Id))
                DestroySpan(Key(p.Id, id));
        }

        // The pylon is being deconstructed: forget its links and fix its neighbours' mitres
        public static void Unlink(MonorailPylon p)
        {
            var neighbours = NeighbourIds(p.Id).ToList();
            foreach (var id in neighbours)
            {
                Links.Remove(Key(p.Id, id));
                DestroySpan(Key(p.Id, id));
            }
            foreach (var id in neighbours)
                if (Pylons.TryGetValue(id, out var n) && n != null)
                    RebuildAround(n);
        }

        // Drops links to pylons that are gone (saves from builds that kept the links of a deconstructed pylon). Pylons
        // are Global entities: once the load is over and no pylon has joined for 30 s, every one standing is in.
        public static void PruneMissing()
        {
            if (WaitScreen.IsWaiting || Time.timeSinceLevelLoad < 30f || Time.time - _lastRegister < 30f)
                return;
            var stale = Links.Where(link =>
            {
                var bar = link.IndexOf('|');
                return bar < 0 || !Pylons.ContainsKey(link.Substring(0, bar)) || !Pylons.ContainsKey(link.Substring(bar + 1));
            }).ToList();
            if (stale.Count == 0)
                return;
            var touched = new HashSet<MonorailPylon>();
            foreach (var link in stale)
            {
                Links.Remove(link);
                DestroySpan(link);
                foreach (var id in link.Split('|'))
                    if (Pylons.TryGetValue(id, out var p) && p != null)
                        touched.Add(p);
            }
            foreach (var p in touched)
                RebuildAround(p);
            Plugin.Logger.LogInfo($"Monorail: dropped {stale.Count} rail link(s) to pylons that no longer exist");
        }

        // Links p to the nearest pylon that can take a rail to it. Returns a message for the player.
        public static string TryAutoLink(MonorailPylon p)
        {
            PruneMissing();
            if (LinkCount(p) >= 2)
                return "This pylon already carries two rails";

            string best = null;
            var bestDist = float.MaxValue;
            foreach (var other in Pylons.Values)
            {
                if (other == null || other == p || !other.IsBuilt || IsLinked(p, other) || LinkCount(other) >= 2)
                    continue;
                var d = Vector3.Distance(p.transform.position, other.transform.position);
                if (d < bestDist && d <= MaxSpan + 5f)
                {
                    bestDist = d;
                    best = other.Id;
                }
            }
            if (best == null)
                return $"No free pylon within {MaxSpan:0} m";

            var target = Pylons[best];
            // A lone pylon turns its arm to stand beside the rail: try both pylons before refusing
            if (LinkCount(p) == 0)
                AlignArm(p, target.RailPoint.position);
            if (LinkCount(target) == 0)
                AlignArm(target, p.RailPoint.position);

            var problem = CheckLink(p, target);
            if (problem != null)
                return problem;

            Links.Add(Key(p.Id, target.Id));
            RebuildAround(p);
            RebuildAround(target);
            return null;
        }

        private static string CheckLink(MonorailPylon a, MonorailPylon b)
        {
            var d = b.RailPoint.position - a.RailPoint.position;
            var flat = new Vector3(d.x, 0f, d.z).magnitude;
            if (d.magnitude > MaxSpan)
                return $"Too far: {d.magnitude:0} m, the longest rail is {MaxSpan:0} m";
            if (d.magnitude < MinSpan)
                return $"Too close: pylons need {MinSpan:0} m between them";
            if (Mathf.Atan2(Mathf.Abs(d.y), flat) * Mathf.Rad2Deg > MaxSlope)
                return $"Too steep: the rail can climb {MaxSlope:0} degrees at most";
            if (!TurnOk(a, b) || !TurnOk(b, a))
                return $"Too sharp a turn: {MaxTurn:0} degrees at most at a pylon";
            if (!ArmOk(a, b) || !ArmOk(b, a))
                return "The rail would hit a pylon's column: turn the pylon so its arm points sideways";
            return null;
        }

        // Why p's rail point can't move to railPoint (telescoping), or null: its rails must stay within the span and
        // slope limits and the turn at p within MaxTurn
        public static string CheckHeight(MonorailPylon p, Vector3 railPoint)
        {
            var neighbours = NeighbourIds(p.Id).Select(id => Pylons.TryGetValue(id, out var n) ? n : null).Where(n => n != null).ToList();
            foreach (var n in neighbours)
            {
                var d = n.RailPoint.position - railPoint;
                var flat = new Vector3(d.x, 0f, d.z).magnitude;
                if (d.magnitude > MaxSpan)
                    return $"The rail to the next pylon would be longer than {MaxSpan:0} m";
                if (Mathf.Atan2(Mathf.Abs(d.y), flat) * Mathf.Rad2Deg > MaxSlope)
                    return $"Too steep: the rail to the next pylon would climb more than {MaxSlope:0} degrees";
            }
            if (neighbours.Count == 2
                && Vector3.Angle(railPoint - neighbours[0].RailPoint.position, neighbours[1].RailPoint.position - railPoint) > MaxTurn)
                return $"Too sharp a turn at this pylon: {MaxTurn:0} degrees at most";
            return null;
        }

        // The turn at p between its current rail and a new one towards next
        private static bool TurnOk(MonorailPylon p, MonorailPylon next)
        {
            var prev = Other(p, next);
            if (prev == null)
                return true;
            var dirIn = p.RailPoint.position - prev.RailPoint.position;
            var dirOut = next.RailPoint.position - p.RailPoint.position;
            return Vector3.Angle(dirIn, dirOut) <= MaxTurn;
        }

        // The arm (column to rail) must stay well off the rail direction, or the rail runs through the column
        private static bool ArmOk(MonorailPylon p, MonorailPylon other)
        {
            var arm = TrackPos.Flat(p.RailPoint.position - p.transform.position);
            var rail = TrackPos.Flat(other.RailPoint.position - p.RailPoint.position);
            var angle = Vector3.Angle(arm, rail);
            return angle >= MinArmAngle && angle <= 180f - MinArmAngle;
        }

        // Turns p about its column so its arm is perpendicular to the rail towards target, keeping the side
        // closer to where the arm points now
        private static void AlignArm(MonorailPylon p, Vector3 target)
        {
            var arm = p.RailPoint.position - p.transform.position;
            arm.y = 0f;
            var armLen = arm.magnitude;
            var toTarget = target - p.transform.position;
            toTarget.y = 0f;
            var dist = toTarget.magnitude;
            if (armLen < 0.01f || dist <= armLen)
                return;

            // RailPoint = foot + arm, rail along (target - RailPoint): perpendicular when cos(arm, toTarget) = armLen / dist
            var phi = Mathf.Acos(armLen / dist) * Mathf.Rad2Deg;
            var baseDir = toTarget / dist;
            var c1 = Quaternion.AngleAxis(phi, Vector3.up) * baseDir;
            var c2 = Quaternion.AngleAxis(-phi, Vector3.up) * baseDir;
            var wanted = Vector3.Angle(c1, arm) <= Vector3.Angle(c2, arm) ? c1 : c2;
            var turn = Vector3.SignedAngle(arm, wanted, Vector3.up);
            p.transform.rotation = Quaternion.AngleAxis(turn, Vector3.up) * p.transform.rotation;
        }

        // ------------------------------------------------------------------ queries

        // The closest rail point to pos, within maxDist
        public static TrackPos NearestPoint(Vector3 pos, float maxDist)
        {
            TrackPos best = null;
            var bestDist = maxDist;
            foreach (var (a, b) in LoadedLinks())
            {
                var pa = a.RailPoint.position;
                var ab = b.RailPoint.position - pa;
                var t = Mathf.Clamp(Vector3.Dot(pos - pa, ab) / ab.sqrMagnitude, 0f, 1f);
                var dist = Vector3.Distance(pos, pa + ab * t);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new TrackPos(a, b, t * ab.magnitude);
                }
            }
            return best;
        }

        // The rail point closest to a ray (the player's aim): at most maxRay along it and maxDist off it
        public static TrackPos NearestToRay(Ray ray, float maxRay, float maxDist)
        {
            TrackPos best = null;
            var bestDist = maxDist;
            foreach (var (a, b) in LoadedLinks())
            {
                var pa = a.RailPoint.position;
                var ab = b.RailPoint.position - pa;
                var len = ab.magnitude;
                var u = ab / len;
                // closest points between the ray and the rail line, then clamped to the rail
                var w = pa - ray.origin;
                var bDot = Vector3.Dot(ray.direction, u);
                var denom = 1f - bDot * bDot;
                var s = denom < 1e-4f ? 0f : (bDot * Vector3.Dot(ray.direction, w) - Vector3.Dot(u, w)) / denom;
                s = Mathf.Clamp(s, 0f, len);
                var onRail = pa + u * s;
                var t = Mathf.Clamp(Vector3.Dot(onRail - ray.origin, ray.direction), 0f, maxRay);
                var dist = Vector3.Distance(onRail, ray.origin + ray.direction * t);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new TrackPos(a, b, s);
                }
            }
            return best;
        }

        private static IEnumerable<(MonorailPylon, MonorailPylon)> LoadedLinks()
        {
            foreach (var link in Links)
            {
                var bar = link.IndexOf('|');
                if (bar < 0)
                    continue;
                if (Pylons.TryGetValue(link.Substring(0, bar), out var a) && a != null
                    && Pylons.TryGetValue(link.Substring(bar + 1), out var b) && b != null)
                    yield return (a, b);
            }
        }

        // ------------------------------------------------------------------ rail meshes

        public static void RebuildAround(MonorailPylon p)
        {
            foreach (var id in NeighbourIds(p.Id).ToList())
            {
                if (!Pylons.TryGetValue(id, out var n) || n == null)
                    continue;
                BuildSpan(p, n);
                // the neighbour's far rail shares its mitre with this one
                var far = Other(n, p);
                if (far != null)
                    BuildSpan(n, far);
            }
        }

        private static void DestroySpan(string key)
        {
            if (Spans.TryGetValue(key, out var go))
            {
                if (go != null)
                {
                    Object.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                    Object.Destroy(go);
                }
                Spans.Remove(key);
            }
        }

        private static void BuildSpan(MonorailPylon a, MonorailPylon b)
        {
            var key = Key(a.Id, b.Id);
            DestroySpan(key);
            if (!RailTemplate.Ready)
                return;

            var pa = a.RailPoint.position;
            var pb = b.RailPoint.position;
            var along = (pb - pa).normalized;
            var frame = Quaternion.LookRotation(along, Vector3.up);
            var side = frame * Vector3.right;
            var up = frame * Vector3.up;

            // mitre planes: the bisector at a joint, square at the end of the line
            var prev = Other(a, b);
            var next = Other(b, a);
            var nA = prev == null ? along : ((pa - prev.RailPoint.position).normalized + along).normalized;
            var nB = next == null ? along : (along + (next.RailPoint.position - pb).normalized).normalized;

            var go = new GameObject($"MonorailRail {key}");
            go.transform.position = pa;
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<MeshFilter>().sharedMesh = RailTemplate.Stretch(pb - pa, along, side, up, nA, nB);
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = RailTemplate.Materials;
            var sky = go.AddComponent<SkyApplier>();
            sky.renderers = new Renderer[] { renderer };
            sky.anchorSky = Skies.Auto;
            Spans[key] = go;
        }

        // A hidden copy of the rail pieces from the bundle, measured once: the 1 m beam template (stretched along each
        // span) and the beam-end sleeve (placed at both ends). Older bundles only have the beam.
        private static class RailTemplate
        {
            private struct Vert
            {
                public float A, S, H;        // along, side, height in the template
                public Vector3 N;            // normal as (along, side, height)
                public Vector2 UV;
                public int Twin;             // the near-end vertex a far vertex stretches from (-1 when none)
            }

            private class Piece
            {
                public Vert[] Verts;
                public int[][] Triangles;    // per submesh
                public int[] Slot;           // submesh -> index in Materials
            }

            private static readonly string[] AssetNames =
                { "Assets/Monorail/Models/Monorail_Rail.fbx", "Monorail_Rail.fbx", "Monorail_Rail.prefab" };

            private static Piece _beam, _sleeve;
            private static Vector3 _alongT, _sideT;
            public static Material[] Materials;
            private static bool _tried;

            public static bool Ready
            {
                get
                {
                    if (!_tried)
                        Load();
                    return _beam != null;
                }
            }

            private static void Load()
            {
                _tried = true;
                var asset = AssetNames.Select(n => Plugin.AssetBundle.LoadAsset<GameObject>(n)).FirstOrDefault(a => a != null);
                if (asset == null)
                {
                    Plugin.Logger.LogError("Monorail_Rail is missing from the bundle: rails can't be drawn");
                    return;
                }
                var template = Object.Instantiate(asset);
                template.SetActive(false);
                Object.DontDestroyOnLoad(template);
                template.transform.position = Vector3.zero;
                MaterialUtils.ApplySNShaders(template);

                var filters = template.GetComponentsInChildren<MeshFilter>(true);
                var beam = filters.FirstOrDefault(f => f.name == "MR_RailBeam") ?? filters.FirstOrDefault();
                if (beam == null || !beam.sharedMesh.isReadable)
                {
                    Plugin.Logger.LogError("Monorail_Rail beam is missing or not readable (Read/Write in the import settings)");
                    return;
                }

                // the template runs 1 m along one horizontal axis from the rail line; find which and its sign
                var m = beam.transform.localToWorldMatrix;
                var pos = beam.sharedMesh.vertices.Select(v => m.MultiplyPoint3x4(v)).ToArray();
                var minX = pos.Min(p => p.x);
                var maxX = pos.Max(p => p.x);
                var minZ = pos.Min(p => p.z);
                var maxZ = pos.Max(p => p.z);
                _alongT = maxX - minX >= maxZ - minZ
                    ? (maxX > -minX ? Vector3.right : Vector3.left)
                    : (maxZ > -minZ ? Vector3.forward : Vector3.back);
                _sideT = Vector3.Cross(Vector3.up, _alongT);

                var materials = new List<Material>();
                _beam = MakePiece(beam, materials, true);
                var sleeve = filters.FirstOrDefault(f => f.name == "MR_RailSleeve");
                if (sleeve != null && sleeve.sharedMesh.isReadable)
                    _sleeve = MakePiece(sleeve, materials, false);
                Materials = materials.ToArray();
            }

            private static Piece MakePiece(MeshFilter mf, List<Material> materials, bool stretched)
            {
                var mesh = mf.sharedMesh;
                var m = mf.transform.localToWorldMatrix;
                var pos = mesh.vertices;
                var nrm = mesh.normals;
                var uv = mesh.uv;
                var verts = new Vert[pos.Length];
                for (var i = 0; i < pos.Length; i++)
                {
                    var p = m.MultiplyPoint3x4(pos[i]);
                    var n = m.MultiplyVector(nrm[i]).normalized;
                    var local = new Vector3(Vector3.Dot(n, _alongT), Vector3.Dot(n, _sideT), n.y);
                    verts[i] = new Vert
                    {
                        A = Vector3.Dot(p, _alongT),
                        S = Vector3.Dot(p, _sideT),
                        H = p.y,
                        N = stretched ? SectionNormal(local) : local,
                        UV = uv[i],
                        Twin = -1,
                    };
                }

                if (stretched)
                {
                    // twins: same section point, normal and V at the other end (the along part of the normal keeps the
                    // two end caps apart)
                    var near = new Dictionary<string, int>();
                    for (var i = 0; i < verts.Length; i++)
                        if (verts[i].A < 0.5f)
                            near[TwinKey(verts[i])] = i;
                    for (var i = 0; i < verts.Length; i++)
                        if (verts[i].A >= 0.5f && near.TryGetValue(TwinKey(verts[i]), out var twin))
                            verts[i].Twin = twin;
                }

                var shared = mf.GetComponent<MeshRenderer>().sharedMaterials;
                var piece = new Piece { Verts = verts, Triangles = new int[mesh.subMeshCount][], Slot = new int[mesh.subMeshCount] };
                for (var s = 0; s < mesh.subMeshCount; s++)
                {
                    piece.Triangles[s] = mesh.GetTriangles(s);
                    var mat = shared[Mathf.Min(s, shared.Length - 1)];
                    var slot = materials.IndexOf(mat);
                    if (slot < 0)
                    {
                        materials.Add(mat);
                        slot = materials.Count - 1;
                    }
                    piece.Slot[s] = slot;
                }
                return piece;
            }

            // A long face's normal lies in the section: drop any lean along the rail (smooth-shaded templates lean
            // towards the end caps, which would tilt the shading over the whole span). End caps keep theirs.
            private static Vector3 SectionNormal(Vector3 n) =>
                Mathf.Abs(n.x) >= 0.7f ? n : new Vector3(0f, n.y, n.z).normalized;

            // rounded to ints, so -0 and 0 match; the along part only tells the two end caps apart
            private static string TwinKey(Vert v) =>
                $"{R(v.S, 1000)}|{R(v.H, 1000)}|{(v.N.x >= 0.7f ? 1 : v.N.x <= -0.7f ? -1 : 0)}|{R(v.N.y, 100)}|{R(v.N.z, 100)}|{R(v.UV.y, 10000)}";

            private static int R(float x, float scale) => Mathf.RoundToInt(x * scale);

            // One span from the origin to span: the beam stretched, each end cut on its mitre plane (normals nA, nB),
            // and a sleeve at each end (the far one turned to face back along the rail)
            public static Mesh Stretch(Vector3 span, Vector3 along, Vector3 side, Vector3 up, Vector3 nA, Vector3 nB)
            {
                var length = span.magnitude;
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var triangles = Materials.Select(_ => new List<int>()).ToArray();

                void AddTriangles(Piece piece, int first)
                {
                    for (var s = 0; s < piece.Triangles.Length; s++)
                        triangles[piece.Slot[s]].AddRange(piece.Triangles[s].Select(i => i + first));
                }

                AddTriangles(_beam, vertices.Count);
                foreach (var v in _beam.Verts)
                {
                    var far = v.A >= 0.5f;
                    var section = side * v.S + up * v.H;
                    // slide the end vertex along the rail onto the mitre plane through the rail point
                    var n = far ? nB : nA;
                    var dn = Vector3.Dot(along, n);
                    var shift = Mathf.Abs(dn) < 0.2f ? 0f : -Vector3.Dot(section, n) / dn;
                    vertices.Add((far ? span : Vector3.zero) + section + along * shift);
                    normals.Add((along * v.N.x + side * v.N.y + up * v.N.z).normalized);
                    var uv = v.UV;
                    if (far && v.Twin >= 0)
                    {
                        var u0 = _beam.Verts[v.Twin].UV.x;
                        uv.x = u0 + (uv.x - u0) * length;
                    }
                    uvs.Add(uv);
                }

                if (_sleeve != null)
                    foreach (var sign in new[] { 1f, -1f })
                    {
                        AddTriangles(_sleeve, vertices.Count);
                        var origin = sign > 0f ? Vector3.zero : span;
                        foreach (var v in _sleeve.Verts)
                        {
                            vertices.Add(origin + along * (sign * v.A) + side * (sign * v.S) + up * v.H);
                            normals.Add(along * (sign * v.N.x) + side * (sign * v.N.y) + up * v.N.z);
                            uvs.Add(v.UV);
                        }
                    }

                var mesh = new Mesh { name = "MonorailRail" };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.subMeshCount = triangles.Length;
                for (var s = 0; s < triangles.Length; s++)
                    mesh.SetTriangles(triangles[s], s);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
