using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // A point on the monorail: D metres along the rail from From's RailPoint towards To's
    public class TrackPos
    {
        public MonorailPylon From, To;
        public float D;

        public TrackPos(MonorailPylon from, MonorailPylon to, float d)
        {
            From = from;
            To = to;
            D = d;
        }

        public Vector3 A => From.RailPoint.position;
        public Vector3 B => To.RailPoint.position;
        public float Length => Vector3.Distance(A, B);
        public Vector3 Dir => (B - A).normalized;
        public Vector3 Position => A + Dir * D;

        // Both pylons still there and still linked
        public bool IsValid => From != null && To != null && MonorailNetwork.IsLinked(From, To);

        public TrackPos Clone() => new TrackPos(From, To, D);

        // Moves along the rail across pylons. Returns false (and stops at the end) when the track ends.
        public bool Move(float delta)
        {
            D += delta;
            for (var guard = 0; guard < 64; guard++)
            {
                if (D > Length)
                {
                    var next = MonorailNetwork.Other(To, From);
                    if (next == null)
                    {
                        D = Length;
                        return false;
                    }
                    D -= Length;
                    From = To;
                    To = next;
                }
                else if (D < 0f)
                {
                    var prev = MonorailNetwork.Other(From, To);
                    if (prev == null)
                    {
                        D = 0f;
                        return false;
                    }
                    To = From;
                    From = prev;
                    D += Length;
                }
                else
                    return true;
            }
            return true;
        }

        // Metres along the track to pylon p's rail point, travelling with sense (+1 towards To, -1 towards From);
        // -1 when the line ends first
        public float DistanceTo(MonorailPylon p, int sense)
        {
            var behind = sense > 0 ? From : To;
            var ahead = sense > 0 ? To : From;
            var d = sense > 0 ? Length - D : D;
            for (var guard = MonorailNetwork.PylonCount + 1; guard > 0; guard--)
            {
                if (ahead == p)
                    return d;
                var next = MonorailNetwork.Other(ahead, behind);
                if (next == null)
                    return -1f;
                d += Vector3.Distance(ahead.RailPoint.position, next.RailPoint.position);
                behind = ahead;
                ahead = next;
            }
            return -1f;
        }

        // Level travel direction (From -> To), turning smoothly through the last metres before and after a pylon
        public Vector3 SmoothDir()
        {
            var dir = Flat(Dir);
            var len = Length;
            var blend = Mathf.Min(1.5f, len / 2f);
            if (D < blend)
            {
                var prev = MonorailNetwork.Other(From, To);
                if (prev != null)
                    dir = Vector3.Slerp(Flat(A - prev.RailPoint.position), dir, 0.5f + 0.5f * D / blend);
            }
            else if (D > len - blend)
            {
                var next = MonorailNetwork.Other(To, From);
                if (next != null)
                    dir = Vector3.Slerp(dir, Flat(next.RailPoint.position - B), 0.5f * (D - (len - blend)) / blend);
            }
            return dir.normalized;
        }

        public static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.normalized;
        }
    }
}
