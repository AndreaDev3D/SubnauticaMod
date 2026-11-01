using AD3D_Common.Utils;
using UnityEngine;

namespace AD3D_IndustrySolution.Runtime
{
    // Procedural drill animation, driven by ExcavatorController: telescope extension, bit spin,
    // shake and sediment particles where the bit meets the ground.
    public class ExcavatorAnimator : MonoBehaviour
    {
        // Set by ExcavatorController
        public bool IsDrilling;
        public float TargetExtension; // 0 = retracted, 1 = both stages fully down
        public float PowerRatio = 1f; // the bit spins slower on weak power

        // Tuning: set in Plugin.cs
        public float SlideStroke = 0.80f;
        public float Slide2Stroke = 1.10f;
        public float ExtendSpeed = 0.15f;  // extension units per second
        public float RetractSpeed = 0.6f;
        public float MaxSpinSpeed = 240f;  // degrees per second
        public float SpinAcceleration = 120f;
        public float ShakeAmplitude = 0.008f;
        public float GroundContactDistance = 0.3f;

        private Transform _slide, _slide2, _spin;
        private Vector3 _slideHome, _slide2Home;
        private Renderer _bit;

        private float _extension;
        private float _spinSpeed;

        private ParticleSystem[] _drillFX;
        private Transform _drillFXRoot;
        private bool _isEmitting;

        private void Start()
        {
            _slide = gameObject.FindByName("EX_Drill_Slide")?.transform;
            _slide2 = gameObject.FindByName("EX_Drill_Slide2")?.transform;
            _spin = gameObject.FindByName("EX_Drill_Spin")?.transform;
            _bit = gameObject.FindByName("EX_Bit_low")?.GetComponent<Renderer>();

            if (_slide != null) _slideHome = _slide.localPosition;
            if (_slide2 != null) _slide2Home = _slide2.localPosition;

            SetupDrillFX();
        }

        private void Update()
        {
            var target = IsDrilling ? TargetExtension : 0f;
            var speed = target > _extension ? ExtendSpeed : RetractSpeed;
            _extension = Mathf.MoveTowards(_extension, target, speed * Time.deltaTime);

            var spinTarget = IsDrilling ? MaxSpinSpeed * Mathf.Lerp(0.25f, 1f, PowerRatio) : 0f;
            _spinSpeed = Mathf.MoveTowards(_spinSpeed, spinTarget, SpinAcceleration * Time.deltaTime);

            var inGround = IsDrilling && UpdateGroundContact();

            // The drill axis is the model's local Z; the stages telescope down along it
            if (_slide != null)
                _slide.localPosition = _slideHome - Vector3.forward * (_extension * SlideStroke);
            if (_slide2 != null)
            {
                var shake = inGround ? (Vector3)(Random.insideUnitCircle * ShakeAmplitude) : Vector3.zero;
                _slide2.localPosition = _slide2Home - Vector3.forward * (_extension * Slide2Stroke) + shake;
            }
            if (_spin != null && _spinSpeed > 0f)
                _spin.Rotate(0f, 0f, _spinSpeed * Time.deltaTime, Space.Self);

            SetEmitting(inGround);
        }

        // Moves the FX to where the bit meets the ground; true when the tip is at or below it
        private bool UpdateGroundContact()
        {
            if (_bit == null || _drillFXRoot == null)
                return false;

            var bounds = _bit.bounds;
            var tip = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            var origin = tip + Vector3.up * 3f;
            var hits = Physics.RaycastAll(origin, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore);

            var groundY = float.NegativeInfinity;
            var nearest = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.distance >= nearest)
                    continue;
                nearest = hit.distance;
                groundY = hit.point.y;
            }

            if (float.IsNegativeInfinity(groundY))
            {
                _drillFXRoot.position = tip;
                return _extension > 0.5f;
            }

            _drillFXRoot.position = new Vector3(tip.x, groundY, tip.z);
            return tip.y <= groundY + GroundContactDistance;
        }

        private void SetEmitting(bool emit)
        {
            if (_drillFX == null || emit == _isEmitting)
                return;

            _isEmitting = emit;
            foreach (var fx in _drillFX)
            {
                if (emit) fx.Play();
                else fx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void SetupDrillFX()
        {
            // A particle system authored in Unity and named "DrillFX" wins over the procedural one
            var authored = gameObject.FindByName("DrillFX");
            if (authored != null)
            {
                _drillFXRoot = authored.transform;
                _drillFX = authored.GetComponentsInChildren<ParticleSystem>(true);
                return;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Plugin.Logger.LogWarning("Excavator: 'Sprites/Default' shader not found, drill particles disabled.");
                return;
            }

            var material = new Material(shader) { mainTexture = CreateSoftCircle(32) };

            _drillFXRoot = new GameObject("DrillFX").transform;
            _drillFXRoot.SetParent(transform, false);

            _drillFX = new[]
            {
                // Sediment cloud billowing up around the bit
                CreateParticles("Sediment", material, new Color(0.72f, 0.66f, 0.52f, 0.35f),
                    lifetime: new Vector2(2.5f, 4f), speed: new Vector2(0.3f, 1.2f), size: new Vector2(0.5f, 1.3f),
                    rate: 18f, gravity: -0.03f, coneAngle: 70f, growth: 2.5f),
                // Rock chips thrown out and falling back
                CreateParticles("Debris", material, new Color(0.25f, 0.22f, 0.18f, 0.9f),
                    lifetime: new Vector2(0.8f, 1.4f), speed: new Vector2(1.5f, 3f), size: new Vector2(0.04f, 0.1f),
                    rate: 14f, gravity: 0.6f, coneAngle: 50f, growth: 1f),
            };
        }

        private ParticleSystem CreateParticles(string name, Material material, Color color, Vector2 lifetime, Vector2 speed,
            Vector2 size, float rate, float gravity, float coneAngle, float growth)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_drillFXRoot, false);
            go.transform.localRotation = Quaternion.LookRotation(Vector3.up); // cone opens upwards

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.maxParticles = 150;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = color;
            main.gravityModifier = gravity;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = 0.35f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, growth));

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }

        private static Texture2D CreateSoftCircle(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.ARGB32, false) { wrapMode = TextureWrapMode.Clamp };
            var center = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.SmoothStep(1f, 0f, d)));
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
