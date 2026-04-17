using HarmonyLib;
using UnityEngine;
using UnityEngine.Windows;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    internal static class VehicleControllerPatch
    {
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

        //[HarmonyPatch("Update")]
        //[HarmonyPostfix]
        //private static void GetVehicleInputPatch(VehicleController __instance)
        //{
        //    if (!__instance.IsOwner) return;
        //    if (!FreeCamClass.isInFreeCam) return;
        //    FreeCamClass.TickDistanceWarning();
        //    if (FreeCamClass.lockFreeCam) return;

        //    if (!FreeCamClass.IsInVehicle()) return;

        //    Vector2 move = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        //    float sprint = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>();

        //    if (move.sqrMagnitude < 0.01f) return;

        //    float speed = FreeCamClass.GetSpeed();
        //    if (sprint > 0.5f) speed *= FreeCamSpeedInfo.SprintMultiplier;

        //    Transform compass = StartOfRound.Instance.freeCinematicCameraTurnCompass;
        //    compass.position += (compass.right * move.x + compass.forward * move.y) * speed * Time.deltaTime;

        //    StartOfRound.Instance.freeCinematicCamera.transform.position = Vector3.Lerp(StartOfRound.Instance.freeCinematicCamera.transform.position, compass.position, 3f * Time.deltaTime);
        //    StartOfRound.Instance.freeCinematicCamera.transform.rotation = Quaternion.Slerp(StartOfRound.Instance.freeCinematicCamera.transform.rotation, compass.rotation, 3f * Time.deltaTime);
        //}

        [HarmonyPatch("DoTurboBoost")]
        [HarmonyPrefix]
        private static bool BlockTurboBoost(VehicleController __instance)
        {
            if (!__instance.localPlayerInControl) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;
            return false;
        }
    }
}
