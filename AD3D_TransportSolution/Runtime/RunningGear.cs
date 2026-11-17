using System.Linq;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // The moving parts above a vehicle's cabin: the bogie pitches to the beam about its pin (the cabin stays level),
    // the up-stop rollers spin with the distance run, and the drawbars swing to meet the coupled car's drawbar.
    // Every part is optional, so older bundles without them still work.
    public class RunningGear
    {
        private const float RollerRadius = 0.08f;

        private readonly MonorailVehicle _vehicle;
        private readonly Transform _bogie;
        private readonly Quaternion _bogieRest;
        private readonly Transform[] _rollers;
        private readonly Quaternion[] _rollerRest;
        private float _pitch, _spin;
        private Vector3 _lastPos;

        public readonly Bar Front, Rear;

        public class Bar
        {
            public Transform T;
            public Quaternion Rest;
            public Vector3 RestDir;   // the bar's pointing direction in its parent's space, at rest
        }

        public RunningGear(MonorailVehicle vehicle)
        {
            _vehicle = vehicle;
            var all = vehicle.GetComponentsInChildren<Transform>(true);
            _bogie = all.FirstOrDefault(t => t.name.EndsWith("Bogie"));
            if (_bogie == null)
                return;
            _bogieRest = _bogie.localRotation;
            _rollers = all.Where(t => t.name.Contains("Roller")).ToArray();
            _rollerRest = _rollers.Select(t => t.localRotation).ToArray();
            var forward = vehicle.transform.TransformDirection(vehicle.LocalForward);
            Front = MakeBar(all.FirstOrDefault(t => t.name.Contains("Drawbar") && t.name.EndsWith("Front")), forward);
            Rear = MakeBar(all.FirstOrDefault(t => t.name.Contains("Drawbar") && t.name.EndsWith("Rear")), -forward);
            _lastPos = vehicle.transform.position;
        }

        private static Bar MakeBar(Transform t, Vector3 worldDir) =>
            t == null ? null : new Bar { T = t, Rest = t.localRotation, RestDir = t.parent.InverseTransformDirection(worldDir) };

        public void Update()
        {
            if (_bogie == null)
                return;
            var root = _vehicle.transform;
            var lateral = root.TransformDirection(Vector3.Cross(Vector3.up, _vehicle.LocalForward)).normalized;

            // pitch: the rail's slope seen along the vehicle's forward, eased so it doesn't snap at a pylon
            var target = 0f;
            if (_vehicle.Track != null)
            {
                var flat = root.TransformDirection(_vehicle.LocalForward);
                var dir = _vehicle.Track.Dir * _vehicle.Facing;
                if (Vector3.Dot(TrackPos.Flat(dir), flat) < 0f)
                    dir = -dir;
                target = Vector3.SignedAngle(flat, dir, lateral);
            }
            _pitch = Mathf.Lerp(_pitch, target, 1f - Mathf.Exp(-8f * Time.deltaTime));
            _bogie.localRotation = Quaternion.AngleAxis(_pitch, _bogie.parent.InverseTransformDirection(lateral)) * _bogieRest;

            // spin: forward travel turns the rollers so their bottoms run along the flange
            var moved = root.position - _lastPos;
            _lastPos = root.position;
            if (moved.sqrMagnitude < 25f)  // not a teleport
                _spin += Vector3.Dot(moved, root.TransformDirection(_vehicle.LocalForward)) / RollerRadius * Mathf.Rad2Deg;
            _spin %= 360f;
            for (var i = 0; i < _rollers.Length; i++)
                _rollers[i].localRotation = Quaternion.AngleAxis(_spin, _rollers[i].parent.InverseTransformDirection(lateral)) * _rollerRest[i];

            Aim(Front, _vehicle.Leader?.Gear?.Rear);
            Aim(Rear, _vehicle.Follower?.Gear?.Front);
        }

        // Turns bar from its rest direction to point at the partner's pivot (they meet head to head)
        private static void Aim(Bar bar, Bar partner)
        {
            if (bar == null)
                return;
            bar.T.localRotation = bar.Rest;
            if (partner == null)
                return;
            var want = partner.T.position - bar.T.position;
            if (want.sqrMagnitude < 0.01f)
                return;
            var rest = bar.T.parent.TransformDirection(bar.RestDir);
            bar.T.rotation = Quaternion.FromToRotation(rest, want) * bar.T.rotation;
        }
    }
}
