using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using System.Collections;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // For MaterialUtils.ApplySNShaders(prefab, 4f, 1f, 1f, new GlassModifier()).
    // Nautilus makes a material transparent when its name contains "transparent", but it only turns specular on when
    // the material has a specular map, so a plain glass material ends up as a faint film with no reflection. This gives
    // such a material the game's own glass instead (Nautilus' copy of the Seamoth canopy; the Seatruck's in Below Zero).
    // Kept in this mod rather than in AD3D_Common: every mod ships its own AD3D_Common.dll and only one of them loads.
    public class GlassModifier : MaterialModifier
    {
        private const float MaxWait = 10f;
        private static bool _gaveUp;

        public static bool Ready => MaterialUtils.GlassMaterial != null;

        // Nautilus loads the glass at the main menu. A prefab built before that keeps its plain transparent material,
        // so a prefab factory with glass yields this first. It gives up after MaxWait, once, and never blocks again.
        public static IEnumerator WaitUntilReady()
        {
            var waited = 0f;
            while (!Ready && !_gaveUp)
            {
                waited += Time.unscaledDeltaTime;
                if (waited > MaxWait)
                    _gaveUp = true;
                yield return null;
            }
        }

        public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
        {
            if (materialType != MaterialUtils.MaterialType.Transparent || !Ready)
                return;

            var glass = MaterialUtils.GlassMaterial;
            var name = material.name;
            material.shader = glass.shader;
            material.CopyPropertiesFromMaterial(glass);
            material.shaderKeywords = glass.shaderKeywords;
            material.renderQueue = glass.renderQueue;
            material.name = name;
        }
    }
}
