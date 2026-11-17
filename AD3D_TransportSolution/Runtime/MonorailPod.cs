using AD3D_Common.Utils;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // The driven cabin: click to board, forward/back to drive, the exit key to step out beside it
    public class MonorailPod : MonorailVehicle, IHandTarget
    {
        public const float MaxSpeed = 10f;
        public const float Acceleration = 4f;
        public const float Braking = 6f;

        private Transform _seat;
        private Transform _eye;
        private Transform _exitPoint;
        private Light[] _lights;
        private float _speed;
        private bool _driving;

        // called to a pylon: drives itself there along the track (sense +1 is towards Track.To)
        private bool _calling;
        private MonorailPylon _callTarget;
        private int _callSense;

        // the plug-sliding hatch (out 3 cm, then back along the side) and the fold-down step under it, open while
        // someone boards or steps out
        private const float DoorSeconds = 0.8f;
        private Transform _hatch, _step;
        private Vector3 _hatchRest, _hatchOut, _hatchBack;
        private Quaternion _stepRest;
        private Vector3 _stepAxis;
        private float _stepAngle;
        private float _door, _doorUntil;

        // The nose is between the headlights, the tow coupler at the back
        public static Shape MeasureShape(GameObject root)
        {
            var l = LocalPoint(root, "HeadlightL");
            var r = LocalPoint(root, "HeadlightR");
            var nose = l != null && r != null ? (l.Value + r.Value) / 2f : new Vector3(0f, 0f, 2f);
            var forward = Flat(nose);
            var rear = LocalPoint(root, "CouplerRear");
            return new Shape
            {
                Forward = forward,
                FrontLen = Vector3.Dot(nose, forward) + 0.1f,
                RearLen = rear != null ? -Vector3.Dot(rear.Value, forward) : 1.9f,
            };
        }

        protected override Shape Measure() => MeasureShape(gameObject);

        protected override void Awake()
        {
            base.Awake();

            // the player sits facing the nose
            var seatPoint = gameObject.FindByName("SeatPoint");
            _seat = new GameObject("MonorailSeat").transform;
            _seat.SetParent(transform, false);
            _seat.localPosition = seatPoint != null ? transform.InverseTransformPoint(seatPoint.transform.position) : new Vector3(0f, -2.2f, 0f);
            _seat.localRotation = Quaternion.LookRotation(LocalForward, Vector3.up);
            _eye = gameObject.FindByName("EyePoint")?.transform;
            _exitPoint = gameObject.FindByName("ExitPoint")?.transform;

            var cabinLight = MakeLight("CabinLight", gameObject.FindByName("EyePoint")?.transform, LightType.Point, 4f, new Color(1f, 0.92f, 0.8f));
            if (cabinLight != null)
                cabinLight.transform.localPosition += Vector3.up * 0.6f;
            _lights = new[]
            {
                MakeLight("HeadlightSpotL", gameObject.FindByName("HeadlightL")?.transform, LightType.Spot, 40f, new Color(1f, 0.97f, 0.9f)),
                MakeLight("HeadlightSpotR", gameObject.FindByName("HeadlightR")?.transform, LightType.Spot, 40f, new Color(1f, 0.97f, 0.9f)),
                cabinLight,
            };
            SetLights(false);

            var lateral = Vector3.Cross(Vector3.up, LocalForward);
            _hatch = gameObject.FindByName("MR_PodHatch")?.transform;
            if (_hatch != null)
            {
                var outward = Outward(_hatch, lateral);
                _hatchRest = _hatch.localPosition;
                _hatchOut = _hatch.parent.InverseTransformVector(transform.TransformDirection(outward) * 0.03f);
                _hatchBack = _hatch.parent.InverseTransformVector(transform.TransformDirection(-LocalForward) * 0.8f);
            }
            _step = gameObject.FindByName("MR_PodStep")?.transform;
            if (_step != null)
            {
                // hinged along the pod: turn it the way that swings its hanging edge outwards
                var outward = Outward(_step, lateral);
                _stepRest = _step.localRotation;
                _stepAngle = Vector3.Dot(Quaternion.AngleAxis(90f, LocalForward) * Vector3.down, outward) > 0f ? 90f : -90f;
                _stepAxis = _step.parent.InverseTransformDirection(transform.TransformDirection(LocalForward));
            }
        }

        // The side of the pod (in root space) that t is on
        private Vector3 Outward(Transform t, Vector3 lateral) =>
            Vector3.Dot(transform.InverseTransformPoint(t.position), lateral) >= 0f ? lateral : -lateral;

        private void OpenDoor(float seconds) => _doorUntil = Time.time + DoorSeconds + seconds;

        protected override void LateUpdate()
        {
            base.LateUpdate();
            var want = Time.time < _doorUntil ? 1f : 0f;
            if (Mathf.Approximately(_door, want))
                return;
            _door = Mathf.MoveTowards(_door, want, Time.deltaTime / DoorSeconds);
            if (_hatch != null)
                _hatch.localPosition = _hatchRest + _hatchOut * Mathf.Clamp01(_door / 0.2f)
                                       + _hatchBack * Mathf.SmoothStep(0f, 1f, (_door - 0.2f) / 0.8f);
            if (_step != null)
                _step.localRotation = Quaternion.AngleAxis(_stepAngle * Mathf.SmoothStep(0f, 1f, _door), _stepAxis) * _stepRest;
        }

        private Light MakeLight(string name, Transform at, LightType type, float range, Color color)
        {
            if (at == null)
                return null;
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = at.position;
            go.transform.rotation = transform.rotation * Quaternion.LookRotation(LocalForward + Vector3.down * 0.15f, Vector3.up);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.range = range;
            light.color = color;
            light.intensity = type == LightType.Spot ? 1.6f : 0.9f;
            light.spotAngle = 55f;
            light.shadows = LightShadows.None;
            return light;
        }

        private void SetLights(bool on)
        {
            foreach (var light in _lights)
                if (light != null)
                    light.enabled = on;
        }

        // Still locked in (not dead, not let out by something else)
        private bool PlayerAboard => _driving && Player.main != null && Player.main.GetMode() == Player.Mode.LockedPiloting;

        protected override void Update()
        {
            base.Update();
            if (!_driving)
            {
                if (_calling)
                    UpdateCall();
                return;
            }
            if (!PlayerAboard)
            {
                StopDriving();
                return;
            }
            if (!IsBuilt || GameInput.GetButtonDown(GameInput.Button.Exit))
            {
                StepOut();
                return;
            }

            var player = Player.main;
            if (player.transform.parent != _seat)
                player.transform.SetParent(_seat, true);
            HandReticle.main.SetText(HandReticle.TextType.Use, "Exit monorail pod", false, GameInput.Button.Exit);
            KeepEyeAtEyePoint(player);
            if (Track == null)
                return;

            var input = GameInput.GetMoveDirection().z;
            var target = input * MaxSpeed;
            var speedingUp = Mathf.Abs(target) > Mathf.Abs(_speed) && (Mathf.Approximately(_speed, 0f) || Mathf.Sign(target) == Mathf.Sign(_speed));
            _speed = Mathf.MoveTowards(_speed, target, (speedingUp ? Acceleration : Braking) * Time.deltaTime);
            if (Mathf.Abs(_speed) > 0.001f && !TryMoveTrain(_speed * Time.deltaTime))
                _speed = 0f;
        }

        // ------------------------------------------------------------------ call buttons

        // The free pod (built, on a rail, nobody aboard) closest to pylon along the track, either way round
        private static MonorailPod FindFree(MonorailPylon pylon, out float distance, out int sense)
        {
            MonorailPod best = null;
            distance = float.MaxValue;
            sense = 0;
            foreach (var v in All)
            {
                if (!(v is MonorailPod pod) || !pod.IsBuilt || pod.Track == null || pod._driving)
                    continue;
                for (var s = 1; s >= -1; s -= 2)
                {
                    var d = pod.Track.DistanceTo(pylon, s);
                    if (d >= 0f && d < distance)
                    {
                        best = pod;
                        distance = d;
                        sense = s;
                    }
                }
            }
            return best;
        }

        public static bool IsCalledTo(MonorailPylon pylon)
        {
            foreach (var v in All)
                if (v is MonorailPod pod && pod._calling && pod._callTarget == pylon)
                    return true;
            return false;
        }

        // Metres to the nearest free pod, or -1 when there is none on this line
        public static float FreePodDistance(MonorailPylon pylon) => FindFree(pylon, out var d, out _) != null ? d : -1f;

        // Sends the nearest free pod to pylon. Returns a message for the player.
        public static string Call(MonorailPylon pylon)
        {
            var pod = FindFree(pylon, out var distance, out var sense);
            if (pod == null)
                return "No free monorail pod on this line";
            if (distance < 0.5f)
                return "The monorail pod is already here";
            if (!pod._calling || pod._callSense != sense)
                pod._speed = 0f;
            pod._calling = true;
            pod._callTarget = pylon;
            pod._callSense = sense;
            pod.SetLights(true);
            return $"Monorail pod on its way: {distance:0} m";
        }

        // Accelerates, then brakes so the hang point stops on the pylon's rail point
        private void UpdateCall()
        {
            if (_callTarget == null || !_callTarget.IsRegistered || Track == null || !IsBuilt)
            {
                EndCall("Monorail pod stopped: the pylon it was called to is gone");
                return;
            }
            var remaining = Track.DistanceTo(_callTarget, _callSense);
            if (remaining < 0f)
            {
                EndCall("Monorail pod stopped: the line to the pylon is broken");
                return;
            }
            if (remaining < 0.02f)
            {
                EndCall("Monorail pod has arrived");
                OpenDoor(2.5f);
                return;
            }

            var cap = Mathf.Min(MaxSpeed, Mathf.Sqrt(2f * Braking * remaining));
            _speed = Mathf.MoveTowards(_speed, cap, (cap > _speed ? Acceleration : Braking) * Time.deltaTime);
            var step = Mathf.Min(Mathf.Max(_speed, 0.2f) * Time.deltaTime, remaining);
            if (!TryMoveTrain(step * _callSense * Facing) && Track.DistanceTo(_callTarget, _callSense) > 0.05f)
                EndCall("Monorail pod stopped: something blocks the track");
        }

        private void EndCall(string message)
        {
            _calling = false;
            _callTarget = null;
            _speed = 0f;
            SetLights(false);
            if (message != null)
                ErrorMessage.AddMessage(message);
        }

        // The camera sits at some height above the player's root: lower the seat anchor so the camera ends up at
        // EyePoint (the player's root follows the anchor)
        private void KeepEyeAtEyePoint(Player player)
        {
            if (_eye == null || MainCamera.camera == null)
                return;
            var cameraHeight = MainCamera.camera.transform.position.y - player.transform.position.y;
            var wanted = _eye.position - Vector3.up * cameraHeight;
            if ((_seat.position - wanted).sqrMagnitude > 0.0004f)
                _seat.position = wanted;
        }

        private void StopDriving()
        {
            _driving = false;
            _speed = 0f;
            SetLights(false);
        }

        private void StepOut()
        {
            StopDriving();
            OpenDoor(1.5f);
            Player.main.ExitLockedMode(false, false);

            var exit = _exitPoint != null ? _exitPoint.position : transform.position - transform.right * 2f;
            // under water the player swims out beside the door; on land they drop to the ground below it
            var to = exit + Vector3.up * 1f;
            if (exit.y > 0f && Physics.Raycast(exit + Vector3.up * 1.5f, Vector3.down, out var hit, 12f, ~0, QueryTriggerInteraction.Ignore))
                to = hit.point + Vector3.up * 1f;
            Player.main.SetPosition(to);
        }

        private void OnDestroy()
        {
            if (PlayerAboard)
                StepOut();
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsBuilt)
                return;
            HandReticle.main.SetText(HandReticle.TextType.Hand, Track != null ? "Board monorail pod" : "Monorail pod: not on a rail", false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, Follower != null ? $"{TrainLength()} wagon(s) coupled" : string.Empty, false);
            HandReticle.main.SetIcon(Track != null ? HandReticle.IconType.Hand : HandReticle.IconType.HandDeny);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsBuilt || Track == null || _driving)
                return;
            if (_calling)
                EndCall(null);
            _driving = true;
            _speed = 0f;
            OpenDoor(1.5f);
            Player.main.EnterLockedMode(_seat, true);
            SetLights(true);
            ErrorMessage.AddMessage("Monorail: forward/back to drive, reverse into a wagon to couple it, exit key to step out");
        }

        private int TrainLength()
        {
            var n = 0;
            for (var f = Follower; f != null; f = f.Follower)
                n++;
            return n;
        }
    }
}
