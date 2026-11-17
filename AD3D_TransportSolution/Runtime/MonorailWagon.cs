using System.Linq;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // A cargo wagon: couples itself behind a pod (or the last wagon of a train) that backs up to it, and comes off
    // when clicked. The cargo hold is the StorageContainer on its 'Storage' child.
    public class MonorailWagon : MonorailVehicle, IHandTarget
    {
        public const float CoupleDistance = 0.6f;
        public const float ReleaseDistance = 1.5f;   // after uncoupling, the train must pull this far away first

        private MonorailVehicle _uncoupledFrom;

        // the cargo door leaves swing out about their hinges while the hold is open
        private const float LeafAngle = 100f;
        private const float DoorSeconds = 0.6f;
        private StorageContainer _storage;
        private Transform[] _leaves;
        private Quaternion[] _leafRest;
        private Vector3[] _leafAxis;
        private float[] _leafAngle;
        private float _door;

        public static Shape MeasureShape(GameObject root) => MeasureShape(root, "WagonCouplerRear");

        protected static Shape MeasureShape(GameObject root, string rearCoupler)
        {
            var front = LocalPoint(root, "CouplerFront");
            var forward = front != null ? Flat(front.Value) : Vector3.forward;
            var rear = LocalPoint(root, rearCoupler);
            return new Shape
            {
                Forward = forward,
                FrontLen = front != null ? Vector3.Dot(front.Value, forward) : 1.7f,
                RearLen = rear != null ? -Vector3.Dot(rear.Value, forward) : 1.7f,
            };
        }

        protected override Shape Measure() => MeasureShape(gameObject);

        protected override void Awake()
        {
            base.Awake();
            _storage = GetComponentInChildren<StorageContainer>(true);
            var lateral = Vector3.Cross(Vector3.up, LocalForward);
            _leaves = GetComponentsInChildren<Transform>(true).Where(t => t.name.Contains("CargoLeaf")).ToArray();
            _leafRest = _leaves.Select(t => t.localRotation).ToArray();
            _leafAxis = _leaves.Select(t => t.parent.InverseTransformDirection(transform.up)).ToArray();
            _leafAngle = _leaves.Select(t =>
            {
                // from the hinge to the leaf's middle, in root space; it must swing away from the body
                var mf = t.GetComponent<MeshFilter>();
                var middle = mf != null && mf.sharedMesh != null ? t.TransformPoint(mf.sharedMesh.bounds.center) : t.position;
                var arm = transform.InverseTransformVector(middle - t.position);
                arm.y = 0f;
                var outward = Vector3.Dot(transform.InverseTransformPoint(t.position), lateral) >= 0f ? lateral : -lateral;
                return Vector3.Dot(Quaternion.AngleAxis(LeafAngle, Vector3.up) * arm, outward) > 0f ? LeafAngle : -LeafAngle;
            }).ToArray();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            var want = _storage != null && _storage.GetOpen() ? 1f : 0f;
            if (Mathf.Approximately(_door, want))
                return;
            _door = Mathf.MoveTowards(_door, want, Time.deltaTime / DoorSeconds);
            var t = Mathf.SmoothStep(0f, 1f, _door);
            for (var i = 0; i < _leaves.Length; i++)
                _leaves[i].localRotation = Quaternion.AngleAxis(_leafAngle[i] * t, _leafAxis[i]) * _leafRest[i];
        }

        protected override void Update()
        {
            base.Update();
            if (IsBuilt && Track != null && Leader == null)
                TryCouple();
        }

        private void TryCouple()
        {
            foreach (var v in All)
            {
                if (v == this || v.Track == null || v.Follower != null || !v.IsBuilt || v.InTrainOf(this))
                    continue;
                var p = v.BehindPos(this);
                if (p == null)
                    continue;
                var d = Vector3.Distance(p.Position, transform.position);
                if (v == _uncoupledFrom)
                {
                    if (d > ReleaseDistance)
                        _uncoupledFrom = null;
                    continue;
                }
                if (d < CoupleDistance)
                {
                    CoupleBehind(v);
                    if (Player.main != null && Player.main.IsPiloting())
                        ErrorMessage.AddMessage($"Monorail: {Label} coupled");
                    return;
                }
            }
        }

        public override void Uncouple()
        {
            if (Leader != null)
                _uncoupledFrom = Leader;
            base.Uncouple();
        }

        protected virtual string Label => "wagon";

        // the hover's second line
        protected virtual string Status => Leader != null ? string.Empty : Track != null ? "Back a pod into it to couple" : "Not on a rail";

        public void OnHandHover(GUIHand hand)
        {
            if (!IsBuilt)
                return;
            HandReticle.main.SetText(HandReticle.TextType.Hand, Leader != null ? $"Uncouple {Label}" : $"Monorail {Label}", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, Status, false);
            HandReticle.main.SetIcon(Leader != null ? HandReticle.IconType.Hand : HandReticle.IconType.Info);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (Leader == null)
                return;
            Uncouple();
            ErrorMessage.AddMessage($"Monorail: {Label} uncoupled");
        }
    }
}
