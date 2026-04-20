using HarmonyLib;
using UnityEngine;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    internal static class VehicleControllerPatch
    {
        [HarmonyPatch("GetVehicleInput")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BlockVehicleInput(VehicleController __instance)
        {
            if (!__instance.localPlayerInControl) return true;
            if (!FreeCamClass.isInFreeCam) return true;

            if (!FreeCamClass.lockFreeCam)
            {
                __instance.moveInputVector = Vector2.zero;
                __instance.drivePedalPressed = false;
                __instance.brakePedalPressed = false;
                return false;
            }

            return true;
        }

        [HarmonyPatch("DoTurboBoost")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BlockTurboBoost(VehicleController __instance)
        {
            if (!__instance.localPlayerInControl) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;
            return false;
        }
    }
}
