using AD3D_TransportSolution.Runtime;
using HarmonyLib;

namespace AD3D_TransportSolution.Patches
{
    // A Prawn riding on a Prawn carrier keeps its parked crouch. The game sets the 'sit' pose every frame in
    // Exosuit.Update, and only while the Prawn touches the ground and is out of the water; a carried Prawn is frozen
    // (never grounded) and the train dips under water, so it would stand up through the carrier's roof.
    [HarmonyPatch(typeof(Exosuit))]
    internal class ExosuitPatcher
    {
        [HarmonyPatch(nameof(Exosuit.Update))]
        [HarmonyPostfix]
        public static void Update_Postfix(Exosuit __instance)
        {
            if (!MonorailPrawnCarrier.Riding.Contains(__instance) || __instance.mainAnimator == null)
                return;
            var animator = __instance.mainAnimator;
            animator.SetBool("sit", true);
            animator.SetBool("onGround", true);
            animator.SetFloat("move_speed_x", 0f);
            animator.SetFloat("move_speed_y", 0f);
            animator.SetFloat("move_speed_z", 0f);
            animator.SetFloat("thrustIntensity", 0f);
        }
    }
}
