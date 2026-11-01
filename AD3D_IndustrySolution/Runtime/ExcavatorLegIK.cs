using AD3D_Common.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace AD3D_IndustrySolution.Runtime
{
    // Plants each foot on the terrain: the leg swings about its hip pin until the sole reaches the ground, the foot
    // tilts to the slope about its ankle, and the hydraulic ram re-aims at the leg's lug with its rod sliding to fit.
    // Needs the rigged model (EX_Leg_<k>, EX_Ankle_<k>, EX_RamTarget_<k>, EX_Ram_<k>, EX_RamRod_<k>); does nothing
    // on a prefab without them. All maths is in world space, so the FBX axis conversion doesn't matter.
    public class ExcavatorLegIK : MonoBehaviour
    {
        // Tuning: set in Plugin.cs
        public float MaxDrop = 0.8f;      // how far a foot can reach down below the machine's floor plane
        public float MaxRise = 0.6f;      // how far a foot can lift above it
        public float MaxFootTilt = 12f;   // degrees, the ankle's designed pitch range
        public float SearchHeight = 3f;   // ground raycast starts this far above the sole
        public float SolveDuration = 10f; // keep re-solving while terrain streams in

        private class Leg
        {
            public string Name;
            public Transform Hip, Ankle, RamTarget, Ram, RamRod;
            public Quaternion HipRest, AnkleRest, RamRest;
            public Vector3 RamRodRest;
        }

        private readonly List<Leg> _legs = new List<Leg>();
        private float _solveUntil;

        private void Start()
        {
            foreach (var k in new[] { "FL", "FR", "RL", "RR" })
            {
                var leg = new Leg
                {
                    Name = k,
                    Hip = gameObject.FindByName($"EX_Leg_{k}")?.transform,
                    Ankle = gameObject.FindByName($"EX_Ankle_{k}")?.transform,
                    RamTarget = gameObject.FindByName($"EX_RamTarget_{k}")?.transform,
                    Ram = gameObject.FindByName($"EX_Ram_{k}")?.transform,
                    RamRod = gameObject.FindByName($"EX_RamRod_{k}")?.transform,
                };
                if (leg.Hip == null || leg.Ankle == null)
                    continue;

                leg.HipRest = leg.Hip.localRotation;
                leg.AnkleRest = leg.Ankle.localRotation;
                if (leg.Ram != null) leg.RamRest = leg.Ram.localRotation;
                if (leg.RamRod != null) leg.RamRodRest = leg.RamRod.localPosition;
                _legs.Add(leg);
            }

            if (_legs.Count == 0)
                return;

            _solveUntil = Time.time + SolveDuration;
            InvokeRepeating(nameof(Solve), 0.2f, 1f);
        }

        private void Solve()
        {
            foreach (var leg in _legs)
                SolveLeg(leg);

            if (Time.time > _solveUntil)
                CancelInvoke(nameof(Solve));
        }

        private void SolveLeg(Leg leg)
        {
            var up = transform.up;

            // Start from the modelled pose every time
            leg.Hip.localRotation = leg.HipRest;
            leg.Ankle.localRotation = leg.AnkleRest;
            if (leg.Ram != null) leg.Ram.localRotation = leg.RamRest;
            if (leg.RamRod != null) leg.RamRod.localPosition = leg.RamRodRest;

            var ankleRestWorld = leg.Ankle.rotation;
            var ramTargetRest = leg.RamTarget != null ? leg.RamTarget.position : Vector3.zero;

            var groundNormal = up;
            var planted = false;

            for (var i = 0; i < 3; i++)
            {
                var hip = leg.Hip.position;
                var ankle = leg.Ankle.position;

                // Sole centre: straight below the ankle, on the machine's floor plane (model floor = root pivot)
                var sole = ankle - up * Vector3.Dot(ankle - transform.position, up);
                if (!FindGround(sole, up, out var ground, out groundNormal))
                    break;
                planted = true;

                var dh = Mathf.Clamp(Vector3.Dot(ground - sole, up), -MaxDrop, MaxRise);
                if (Mathf.Abs(dh) < 0.005f)
                    break;

                // Swing about the hip pin in the leg's vertical plane so the ankle moves by dh
                var r = ankle - hip;
                var b = Vector3.Dot(r, up);
                var horizontal = Vector3.ProjectOnPlane(r, up);
                var length = r.magnitude;
                var targetB = Mathf.Clamp(b + dh, -length * 0.98f, length * 0.98f);
                var target = horizontal.normalized * Mathf.Sqrt(length * length - targetB * targetB) + up * targetB;

                leg.Hip.rotation = Quaternion.FromToRotation(r, target) * leg.Hip.rotation;
            }

            // Foot: back to its modelled world orientation, then tilted to the slope (clamped)
            var tilt = planted ? Vector3.RotateTowards(up, groundNormal, MaxFootTilt * Mathf.Deg2Rad, 0f) : up;
            leg.Ankle.rotation = Quaternion.FromToRotation(up, tilt) * ankleRestWorld;

            // Ram: barrel pivots on its base pin to face the lug, rod slides out/in by the length change
            if (leg.Ram != null && leg.RamTarget != null)
            {
                var basePin = leg.Ram.position;
                var restDir = ramTargetRest - basePin;
                var newDir = leg.RamTarget.position - basePin;
                leg.Ram.rotation = Quaternion.FromToRotation(restDir, newDir) * leg.Ram.rotation;

                if (leg.RamRod != null)
                    leg.RamRod.position = basePin + newDir.normalized * (newDir.magnitude - restDir.magnitude);
            }
        }

        private bool FindGround(Vector3 sole, Vector3 up, out Vector3 point, out Vector3 normal)
        {
            point = sole; normal = up;
            var origin = sole + up * SearchHeight;
            var hits = Physics.RaycastAll(origin, -up, SearchHeight + MaxDrop + 0.5f, ~0, QueryTriggerInteraction.Ignore);

            var nearest = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.distance >= nearest || hit.collider.transform.IsChildOf(transform))
                    continue;
                // Skip fish, dropped items and anything else that moves
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                    continue;

                nearest = hit.distance;
                point = hit.point;
                normal = hit.normal;
            }
            return nearest < float.MaxValue;
        }
    }
}
