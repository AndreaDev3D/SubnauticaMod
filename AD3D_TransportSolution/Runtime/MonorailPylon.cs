using AD3D_Common.Utils;
using System.Linq;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // A monorail pylon: joins the network once built, and a freshly built one links itself to the nearest free pylon.
    // It telescopes: the head (arm, drum, RailPoint) rides on two nested masts, in 5 levels from the built height up
    // to 3x the rail height, and its rails and whatever hangs on them follow.
    public class MonorailPylon : MonoBehaviour, IHandTarget, IConstructable
    {
        public const int Levels = 5;
        public const float LiftSpeed = 1.5f;   // m/s

        private Constructable _constructable;
        private bool _registered;
        private float _firstSeen = -1f;  // first frame out of the loading screen, built
        private bool _seenUnbuilt;   // built in this session (not loaded built), so it may link itself

        private Transform _head, _mast1, _mast2;
        private Vector3 _headRest, _mast1Rest, _mast2Rest;
        private float _railHeight;               // rail line above the foot, collapsed
        private float _lift, _liftTarget;        // metres the head is raised
        private int _movedFrame = -10;
        private BoxCollider _columnBox, _armBox;
        private Vector3 _columnCenter, _columnSize, _armCenter;
        private Light _callLight;

        public Transform RailPoint { get; private set; }
        public string Id { get; private set; }
        public bool IsBuilt => _constructable == null || _constructable.constructed;
        public bool IsRegistered => _registered;
        public int Level { get; private set; }
        public bool CanTelescope => _head != null;
        public float RailHeight => _railHeight + LiftFor(Level);
        // still true the frame after the last step, so whatever follows it settles on the final height
        public bool IsMoving => Time.frameCount - _movedFrame <= 1;

        private float LiftFor(int level) => level * 0.5f * _railHeight;

        private void Awake()
        {
            _constructable = GetComponent<Constructable>();
            RailPoint = gameObject.FindByName("RailPoint")?.transform;
            if (RailPoint == null)
            {
                Plugin.Logger.LogWarning("Monorail_Pylon has no RailPoint: the rail runs from the pylon's foot");
                RailPoint = transform;
            }
            _railHeight = Vector3.Dot(RailPoint.position - transform.position, transform.up);

            // telescoping parts (older bundles have none: the pylon then keeps its height)
            _head = gameObject.FindByName("MR_PylonHead")?.transform;
            _mast1 = gameObject.FindByName("MR_PylonMast1")?.transform;
            _mast2 = gameObject.FindByName("MR_PylonMast2")?.transform;
            if (_head != null)
                _headRest = _head.localPosition;
            if (_mast1 != null)
                _mast1Rest = _mast1.localPosition;
            if (_mast2 != null)
                _mast2Rest = _mast2.localPosition;
            // the prefab's collider: the tall box is the column (grows), the other the arm (rides up)
            var boxes = gameObject.FindByName("Collider")?.GetComponents<BoxCollider>().OrderByDescending(b => b.size.y).ToArray();
            if (boxes != null && boxes.Length > 0)
            {
                _columnBox = boxes[0];
                _columnCenter = _columnBox.center;
                _columnSize = _columnBox.size;
                if (boxes.Length > 1)
                {
                    _armBox = boxes[1];
                    _armCenter = _armBox.center;
                }
            }

            // the call box button (older bundles: the same spot in root space, where Blender (x, y, z) is (-x, z, -y)
            // after the -90 X import) and the height panel's Up and Down buttons
            var call = AddButton("CallButton", new Vector3(-0.375f, 1.42f, 0f));
            call.AddComponent<MonorailCallButton>().Pylon = this;
            _callLight = call.AddComponent<Light>();
            _callLight.type = LightType.Point;
            _callLight.color = new Color(0.3f, 0.9f, 1f);
            _callLight.range = 1.5f;
            _callLight.intensity = 1.5f;
            _callLight.shadows = LightShadows.None;
            _callLight.enabled = false;
            if (CanTelescope)
                foreach (var (name, step) in new[] { ("HeightUp", 1), ("HeightDown", -1) })
                {
                    var button = AddButton(name, null);
                    if (button == null)
                        continue;
                    var b = button.AddComponent<MonorailHeightButton>();
                    b.Pylon = this;
                    b.Step = step;
                }
        }

        // A 0.2 m click area on the named empty (or at fallback in root space; none without either)
        private GameObject AddButton(string empty, Vector3? fallback)
        {
            var at = gameObject.FindByName(empty)?.transform;
            if (at == null && fallback == null)
                return null;
            var button = new GameObject(empty + "Trigger");
            button.transform.SetParent(at != null ? at : transform, false);
            if (at == null)
                button.transform.localPosition = fallback.Value;
            // 0.2 m in the world, whatever scale the FBX import left on the empty
            button.AddComponent<BoxCollider>().size = Vector3.one * 0.2f / Mathf.Max(1e-4f, button.transform.lossyScale.x);
            return button;
        }

        private void Update()
        {
            if (!_registered)
            {
                if (!IsBuilt)
                {
                    // not while loading, when a saved pylon can read as unbuilt for a moment
                    if (!WaitScreen.IsWaiting)
                        _seenUnbuilt = true;
                    return;
                }
                // a loaded pylon waits for its saved id (its rails are saved against it); a new one, or one whose id
                // never came (10 s after the load), gets a fresh one
                if (WaitScreen.IsWaiting)
                    return;
                if (_firstSeen < 0f)
                    _firstSeen = Time.time;
                Id = SaveIds.Of(this, _seenUnbuilt || Time.time - _firstSeen > 10f);
                if (string.IsNullOrEmpty(Id))
                    return;

                // a saved height applies at once, before the rails are laid
                if (CanTelescope && Plugin.ModData.Heights.TryGetValue(Id, out var level))
                {
                    Level = Mathf.Clamp(level, 0, Levels - 1);
                    _lift = _liftTarget = LiftFor(Level);
                    ApplyLift();
                }
                _registered = true;
                MonorailNetwork.Register(this);
                if (_seenUnbuilt && MonorailNetwork.LinkCount(this) == 0)
                    ErrorMessage.AddMessage(MonorailNetwork.TryAutoLink(this) ?? "Monorail: rail laid to the nearest pylon");
            }

            if (_lift != _liftTarget)
            {
                _lift = Mathf.MoveTowards(_lift, _liftTarget, LiftSpeed * Time.deltaTime);
                _movedFrame = Time.frameCount;
                ApplyLift();
                MonorailNetwork.RebuildAround(this);
            }

            // the call lamp blinks while a pod is on its way here
            var called = MonorailPod.IsCalledTo(this);
            _callLight.enabled = called && Mathf.Repeat(Time.time, 0.8f) < 0.4f;
        }

        // Head and top mast up by the lift, middle mast by half; the column collider grows, the arm's rides up
        private void ApplyLift()
        {
            var up = transform.up;
            Slide(_head, _headRest, _lift);
            Slide(_mast2, _mast2Rest, _lift);
            Slide(_mast1, _mast1Rest, _lift * 0.5f);
            if (_columnBox != null)
            {
                var d = _columnBox.transform.InverseTransformVector(up * _lift);
                _columnBox.center = _columnCenter + d * 0.5f;
                _columnBox.size = _columnSize + new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z));
            }
            if (_armBox != null)
                _armBox.center = _armCenter + _armBox.transform.InverseTransformVector(up * _lift);
        }

        private void Slide(Transform t, Vector3 rest, float by)
        {
            if (t != null)
                t.localPosition = rest + t.parent.InverseTransformVector(transform.up * by);
        }

        // Telescopes one level up (+1) or down (-1). Returns a message for the player.
        public string TryStep(int step)
        {
            if (!CanTelescope)
                return "This pylon was built from an older model and can't telescope";
            var level = Level + step;
            if (level < 0 || level >= Levels)
                return step > 0 ? "The pylon is at its highest" : "The pylon is at its lowest";
            var problem = MonorailNetwork.CheckHeight(this, RailPoint.position + transform.up * (LiftFor(level) - _lift));
            if (problem != null)
                return problem;
            Level = level;
            _liftTarget = LiftFor(level);
            if (level == 0)
                Plugin.ModData.Heights.Remove(Id);
            else
                Plugin.ModData.Heights[Id] = level;
            return $"Pylon height {level + 1}/{Levels}: rail {RailHeight:0.0} m above the foot";
        }

        // Deconstruction started: take the rails down and forget the links. The game disables every component on the
        // root at the same moment (Constructable.SetState), so Update never sees it; this is called just before.
        public void OnConstructedChanged(bool constructed)
        {
            if (constructed || !_registered)
                return;
            _registered = false;
            _seenUnbuilt = true;
            Plugin.ModData.Heights.Remove(Id);
            MonorailNetwork.Unlink(this);
            MonorailNetwork.Unregister(this);
            // finished again later, it starts from the lowest setting, as its saved height is gone
            Level = 0;
            _lift = _liftTarget = 0f;
            ApplyLift();
            if (_callLight != null)
                _callLight.enabled = false;
        }

        public bool IsDeconstructionObstacle() => false;

        // Refused while a pod or wagon hangs on one of its rails: taking the rail away would leave it in mid-air
        public bool CanDeconstruct(out string reason)
        {
            reason = null;
            foreach (var v in MonorailVehicle.All)
                if (v != null && v.Track != null && (v.Track.From == this || v.Track.To == this))
                {
                    reason = $"A {(v is MonorailPod ? "pod" : "wagon")} is on this pylon's rails: move it off or deconstruct it first";
                    return false;
                }
            return true;
        }

        private void OnDestroy()
        {
            if (_registered)
                MonorailNetwork.Unregister(this);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!_registered)
                return;
            MonorailNetwork.PruneMissing();
            var links = MonorailNetwork.LinkCount(this);
            HandReticle.main.SetText(HandReticle.TextType.Hand, $"Monorail pylon: {links}/2 rails", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, links < 2 ? "Click to lay a rail to the nearest free pylon" : string.Empty, false);
            HandReticle.main.SetIcon(links < 2 ? HandReticle.IconType.Hand : HandReticle.IconType.Info);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!_registered || MonorailNetwork.LinkCount(this) >= 2)
                return;
            ErrorMessage.AddMessage(MonorailNetwork.TryAutoLink(this) ?? "Monorail: rail laid to the nearest pylon");
        }
    }
}
