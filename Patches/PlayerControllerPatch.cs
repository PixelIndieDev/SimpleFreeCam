using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerControllerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void OnAwake(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || !__instance.IsLocalPlayer)
            {
                return;
            }

            FreeCamClass.SetPlayerController(__instance);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void ApplyFreecamMovement(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;
            if (!FreeCamClass.isInFreeCam) return;
            if (FreeCamClass.lockFreeCam) return;

            if (FreeCamClass.isInVehicle) return;

            Vector2 move = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            float sprint = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>();

            if (move.sqrMagnitude < 0.01f) return;

            float speed = FreeCamClass.GetSpeed();
            if (sprint > 0.5f) speed *= FreeCamSpeedInfo.SprintMultiplier;

            Transform compass = StartOfRound.Instance.freeCinematicCameraTurnCompass;
            compass.position += (compass.right * move.x + compass.forward * move.y) * speed * Time.deltaTime;

            StartOfRound.Instance.freeCinematicCamera.transform.position = Vector3.Lerp(StartOfRound.Instance.freeCinematicCamera.transform.position, compass.position, 3f * Time.deltaTime);
            StartOfRound.Instance.freeCinematicCamera.transform.rotation = Quaternion.Slerp(StartOfRound.Instance.freeCinematicCamera.transform.rotation, compass.rotation, 3f * Time.deltaTime);
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPostfix]
        private static void OnPlayerDeath(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner)
            {
                return;
            }

            if (__instance.isFreeCamera)
            {
                FreeCamClass.DisableFreecam();
            }
        }

        [HarmonyPatch("ScrollMouse_performed")]
        [HarmonyPrefix]
        private static bool InterceptScroll(PlayerControllerB __instance, InputAction.CallbackContext context)
        {
            if (!__instance.IsOwner) return true;
            if (!FreeCamClass.isInFreeCam) return true;
            if (FreeCamClass.lockFreeCam) return true;

            FreeCamClass.AdjustSpeed(context.ReadValue<float>());
            return false;
        }
    }
}
