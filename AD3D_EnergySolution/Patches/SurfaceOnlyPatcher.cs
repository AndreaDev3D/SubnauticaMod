using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AD3D_EnergySolution.Patches
{
    // Generators that burn in air (Biomass Burner) can't be placed under water: the ghost turns red below the surface
    [HarmonyPatch(typeof(Builder))]
    internal class SurfaceOnlyPatcher
    {
        public static readonly HashSet<TechType> TechTypes = new HashSet<TechType>();

        // Lowest base height allowed, so a burner still fits on a beach right at the waterline
        public const float MinHeight = -0.5f;

        private static float _lastMessage;

        [HarmonyPatch("UpdateAllowed")]
        [HarmonyPostfix]
        private static void UpdateAllowedPatch(TechType ___constructableTechType, Vector3 ___placePosition, ref bool __result)
        {
            if (!__result || !TechTypes.Contains(___constructableTechType) || ___placePosition.y >= MinHeight)
                return;

            __result = false;
            if (Time.time - _lastMessage > 3f)
            {
                _lastMessage = Time.time;
                ErrorMessage.AddMessage($"{Language.main.Get(___constructableTechType.AsString())} needs air: build it above the water");
            }
        }
    }
}
