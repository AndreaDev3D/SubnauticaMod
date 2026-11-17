using AD3D_TransportSolution.Runtime;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AD3D_TransportSolution.Patches
{
    // Nautilus has no placement hook, so the builder is patched: pods and wagons are placed by aiming at a rail, the
    // ghost hangs on the rail facing away from the player
    [HarmonyPatch(typeof(Builder))]
    internal class BuilderPatcher
    {
        // TechType -> the vehicle's forward in root space
        public static readonly Dictionary<TechType, Vector3> OnRail = new Dictionary<TechType, Vector3>();
        public const float AimRange = 25f;
        public const float AimTolerance = 5f;

        private static float _lastMessage;

        private static void Message(string text)
        {
            if (Time.time - _lastMessage < 3f)
                return;
            _lastMessage = Time.time;
            ErrorMessage.AddMessage(text);
        }

        [HarmonyPatch("CheckAsSubModule")]
        [HarmonyPostfix]
        private static void CheckAsSubModulePatch(TechType ___constructableTechType, ref Vector3 ___placePosition,
            ref Quaternion ___placeRotation, ref GameObject ___placementTarget, ref bool __result)
        {
            if (!OnRail.TryGetValue(___constructableTechType, out var localForward))
                return;

            var aim = MainCamera.camera.transform;
            var track = MonorailNetwork.NearestToRay(new Ray(aim.position, aim.forward), AimRange, AimTolerance);
            if (track == null)
            {
                __result = false;
                Message("Aim at a monorail rail");
                return;
            }

            var dir = track.SmoothDir();
            if (Vector3.Dot(aim.forward, dir) < 0f)
                dir = -dir;
            ___placePosition = track.Position;
            ___placeRotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(localForward, Vector3.up));
            ___placementTarget = null;
            __result = true;
        }
    }
}
