using AD3D_IndustrySolution.Runtime;
using HarmonyLib;

namespace AD3D_IndustrySolution.Patches
{
    // Machine relays are consumers, not grid nodes. In vanilla a relay only links on when something feeds it, so a
    // machine could only reach a base by pulling nearby transmitters into itself, rerouting the grid through it.
    [HarmonyPatch(typeof(PowerRelay))]
    internal class MachinePowerPatcher
    {
        private static bool IsMachine(PowerRelay relay) => relay != null && relay.TryGetComponent<DrillController>(out _);

        // A machine links on to the nearest relay even with nothing feeding it, and draws from that chain's end
        [HarmonyPatch(nameof(PowerRelay.CanMakeConnection))]
        [HarmonyPostfix]
        private static void CanMakeConnectionPatch(PowerRelay __instance, ref bool __result)
        {
            if (!__result && IsMachine(__instance) && __instance.gameObject.activeInHierarchy &&
                !__instance.dontConnectToRelays && !__instance.IsUnderConstruction())
                __result = true;
        }

        // Only generators (relays with their own source) may plug into a machine; transmitters, bases and other
        // machines keep their links elsewhere. Existing links into a machine drop and reconnect on the next check.
        [HarmonyPatch("IsValidRelayForConnection")]
        [HarmonyPostfix]
        private static void IsValidRelayForConnectionPatch(PowerRelay __instance, PowerRelay potentialRelay, ref bool __result)
        {
            if (__result && __instance.internalPowerSource == null && IsMachine(potentialRelay))
                __result = false;
        }
    }
}
