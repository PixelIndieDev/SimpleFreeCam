using HarmonyLib;
using System.Reflection;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal static class HUDManagerPatch
    {
        private static MethodInfo MethodDisableScanElementsCached;

        [HarmonyPatch("PingScan_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool DisableScanning(HUDManager __instance)
        {
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;
            return false;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void DisableScanHUD(HUDManager __instance)
        {
            if(!FreeCamClass.isInFreeCam) return;

            if (MethodDisableScanElementsCached == null)
            {
                MethodDisableScanElementsCached = typeof(HUDManager).GetMethod("DisableAllScanElements", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            MethodDisableScanElementsCached?.Invoke(__instance, null);
        }
    }
}
