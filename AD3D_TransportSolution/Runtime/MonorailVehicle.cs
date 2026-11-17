using System.Collections.Generic;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // Something that hangs on the rail: its root is the hang point on the rail line and it stays level, turning only
    // about the vertical. Pods pull trains of wagons coupled behind them.
    public abstract class MonorailVehicle : MonoBehaviour
    {
        public static readonly List<MonorailVehicle> All = new List<MonorailVehicle>();

        public const float SnapDistance = 4f;

        private Constructable _constructable;
        private float _nextSnap;

        public TrackPos Track;
        public int Facing = 1;              // +1: the vehicle's front points from Track.From to Track.To
        public MonorailVehicle Leader, Follower;

        // Measured from the model's empties, in root space
        public struct Shape
        {
            public Vector3 Forward;
            public float FrontLen, RearLen;   // hang point to the front / rear coupling face
        }

        public Vector3 LocalForward = Vector3.forward;
        public float FrontLen, RearLen;
        public RunningGear Gear;
        public float Length => FrontLen + RearLen;

        public bool IsBuilt => _constructable == null || _constructable.constructed;
        public bool OnTrack => Track != null;

        // false while something sticks out that can't travel (a Prawn carrier's deck slid out): the train won't move
        public virtual bool Stowed => true;
        public virtual string StowMessage => null;
        private static float _nextStowMessage;

        protected abstract Shape Measure();

        protected virtual void Awake()
        {
            _constructable = GetComponent<Constructable>();
            var shape = Measure();
            LocalForward = shape.Forward;
            FrontLen = shape.FrontLen;
            RearLen = shape.RearLen;
            Gear = new RunningGear(this);
        }

        protected virtual void OnEnable() => All.Add(this);

        protected virtual void OnDisable()
        {
            All.Remove(this);
            Uncouple();
            if (Follower != null)
                Follower.Uncouple();
        }

        protected virtual void Update()
        {
            if (!IsBuilt)
            {
                Track = null;
                Uncouple();
                if (Follower != null)
                    Follower.Uncouple();
                return;
            }
            if (Track != null && !Track.IsValid)
                Track = null;
            // a telescoping pylon carries the rail, and whatever hangs on it, up or down
            if (Track != null && (Track.From.IsMoving || Track.To.IsMoving))
                Place();
            if (Track == null && Leader == null && Time.time >= _nextSnap)
            {
                _nextSnap = Time.time + 0.5f;
                TrySnap();
            }
        }

        protected virtual void LateUpdate() => Gear?.Update();

        private void TrySnap()
        {
            var t = MonorailNetwork.NearestPoint(transform.position, SnapDistance);
            if (t == null)
                return;
            Track = t;
            Facing = Vector3.Dot(transform.TransformDirection(LocalForward), t.Dir) >= 0f ? 1 : -1;
            Place();
        }

        public void Place()
        {
            transform.position = Track.Position;
            var dir = Track.SmoothDir() * Facing;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(LocalForward, Vector3.up));
        }

        // Where a vehicle coupled behind this one would hang
        public TrackPos BehindPos(MonorailVehicle follower)
        {
            var p = Track.Clone();
            return p.Move(-Facing * (RearLen + follower.FrontLen)) ? p : null;
        }

        public void CoupleBehind(MonorailVehicle leader)
        {
            Leader = leader;
            leader.Follower = this;
            Facing = leader.Facing;
            Track = leader.BehindPos(this);
            Place();
            // the wagons already coupled behind this one line up too
            for (var f = Follower; f != null; f = f.Follower)
            {
                f.Facing = f.Leader.Facing;
                var p = f.Leader.BehindPos(f);
                if (p != null)
                {
                    f.Track = p;
                    f.Place();
                }
            }
        }

        public virtual void Uncouple()
        {
            if (Leader != null && Leader.Follower == this)
                Leader.Follower = null;
            Leader = null;
        }

        public bool InTrainOf(MonorailVehicle head)
        {
            for (var v = head; v != null; v = v.Follower)
                if (v == this)
                    return true;
            return false;
        }

        // Moves this vehicle and everything coupled behind it by delta metres along its facing. Nothing moves if a
        // follower would run off the end of the track or the train would push into another vehicle. Returns false
        // when the train stopped (blocked, or the head reached the end of the track).
        protected bool TryMoveTrain(float delta)
        {
            for (var v = this; v != null; v = v.Follower)
                if (!v.Stowed)
                {
                    if (Time.time >= _nextStowMessage)
                    {
                        _nextStowMessage = Time.time + 3f;
                        ErrorMessage.AddMessage(v.StowMessage ?? "Monorail: the train can't move");
                    }
                    return false;
                }

            var plan = new List<(MonorailVehicle v, TrackPos pos)>();
            var head = Track.Clone();
            var free = head.Move(delta * Facing);
            plan.Add((this, head));
            var leaderPos = head;
            for (var f = Follower; f != null; f = f.Follower)
            {
                var p = leaderPos.Clone();
                if (!p.Move(-f.Leader.Facing * (f.Leader.RearLen + f.FrontLen)))
                    return false;
                plan.Add((f, p));
                leaderPos = p;
            }

            foreach (var other in All)
            {
                if (other.Track == null || other.InTrainOf(this))
                    continue;
                foreach (var (v, p) in plan)
                {
                    var min = 0.9f * 0.5f * (v.Length + other.Length);
                    var then = Vector3.Distance(p.Position, other.transform.position);
                    if (then < min && then < Vector3.Distance(v.transform.position, other.transform.position))
                        return false;
                }
            }

            foreach (var (v, p) in plan)
            {
                v.Track = p;
                v.Place();
            }
            return free;
        }

        // The position of a named empty in root's space
        protected static Vector3? LocalPoint(GameObject root, string name)
        {
            var t = AD3D_Common.Utils.GameObjectFinder.FindByName(root, name);
            return t == null ? (Vector3?)null : root.transform.InverseTransformPoint(t.transform.position);
        }

        protected static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized;
        }
    }
}
