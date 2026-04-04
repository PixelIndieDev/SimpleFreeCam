using HarmonyLib;
using UnityEngine;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    internal static class VehicleControllerPatch
    {
        [HarmonyPatch("TakeControlOfVehicle")]
        [HarmonyPostfix]
        private static void OnTakeControlOfVehicle(VehicleController __instance)
        {
            if (!__instance.IsOwner || !__instance.IsLocalPlayer)
            {
                return;
            }

            FreeCamClass.SetInVehicle(true);
        }

        [HarmonyPatch("LoseControlOfVehicle")]
        [HarmonyPostfix]
        private static void OnLoseControlOfVehicle(VehicleController __instance)
        {
            if (!__instance.IsOwner || !__instance.IsLocalPlayer)
            {
                return;
            }

            FreeCamClass.SetInVehicle(false);
        }

        [HarmonyPatch("GetVehicleInput")]
        [HarmonyPrefix]
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
    }
}
