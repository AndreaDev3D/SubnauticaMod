using System.Linq;
using UnityEngine;

namespace AD3D_IndustrySolution.Runtime
{
    // Blinking amber light over the lubricant point while a machine is out of lubricant. A 'LubricantLight' object in
    // the prefab sets where it shines; otherwise it sits just above the anchor object (collider, then renderer bounds).
    // Kept per mod rather than in AD3D_Common: every mod ships its own AD3D_Common.dll and only one of them loads.
    public class LubricantWarning : MonoBehaviour
    {
        public string AnchorName = "Lubricant";
        public float HeightAboveAnchor = 0.3f;
        public bool Active;

        public Color Color = new Color(1f, 0.55f, 0.05f);
        public float Range = 4f;
        public float MaxIntensity = 2.5f;
        public float BlinkPerSecond = 1f;

        private Light _light;

        private void Start()
        {
            var lightObject = new GameObject("LubricantWarningLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.position = FindLightPosition();

            _light = lightObject.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = Color;
            _light.range = Range;
            _light.shadows = LightShadows.None;
            _light.enabled = false;
        }

        private void Update()
        {
            if (_light == null)
                return;

            _light.enabled = Active;
            if (Active)
                _light.intensity = MaxIntensity * (0.5f + 0.5f * Mathf.Sin(Time.time * BlinkPerSecond * 2f * Mathf.PI));
        }

        private Vector3 FindLightPosition()
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            var marker = transforms.FirstOrDefault(t => t.name == "LubricantLight");
            if (marker != null)
                return marker.position;

            var anchor = transforms.FirstOrDefault(t => t.name == AnchorName);
            if (anchor == null)
                return transform.position + Vector3.up;

            var colliders = anchor.GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).Select(c => c.bounds);
            var renderers = anchor.GetComponentsInChildren<Renderer>().Select(r => r.bounds);
            var bounds = colliders.Concat(renderers).ToList();
            if (bounds.Count == 0)
                return anchor.position + Vector3.up * HeightAboveAnchor;

            var all = bounds[0];
            foreach (var b in bounds.Skip(1))
                all.Encapsulate(b);
            return new Vector3(all.center.x, all.max.y + HeightAboveAnchor, all.center.z);
        }
    }
}
