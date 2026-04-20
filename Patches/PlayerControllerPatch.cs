using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerControllerPatch
    {
        private static int originalSensitivity = -1;
        private static Vector3 compassPositionBeforeUpdate;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void SaveCompassPosition(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && __instance == StartOfRound.Instance.localPlayerController && FreeCamClass.isInFreeCam)
            {
                //capture pre move location
                compassPositionBeforeUpdate = StartOfRound.Instance.freeCinematicCameraTurnCompass.position;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void ApplyFreecamMovement(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || __instance != StartOfRound.Instance.localPlayerController) return;
            if (!FreeCamClass.isInFreeCam) return;
            if (__instance.inTerminalMenu || __instance.isTypingChat)
            {
                FreeCamClass.DisableFreecam();
                return;
            }
            FreeCamClass.TickDistanceWarning();
            if (FreeCamClass.lockFreeCam) return;

            Transform compass = StartOfRound.Instance.freeCinematicCameraTurnCompass;
            //reset position back to pre move capture
            compass.position = compassPositionBeforeUpdate;
            //now we have a baseline, we can now add our own movement without it being multiplied
            Vector2 move = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            float sprint = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>();

            if (move.sqrMagnitude < 0.01f) return;

            float speed = FreeCamClass.GetSpeed();
            if (sprint > 0.5f) speed *= FreeCamSpeedInfo.SprintMultiplier;
            compass.position += (compass.right * move.x + compass.forward * move.y) * speed * Time.deltaTime;

            StartOfRound.Instance.freeCinematicCamera.transform.position = Vector3.Lerp(StartOfRound.Instance.freeCinematicCamera.transform.position, compass.position, 3f * Time.deltaTime);
            StartOfRound.Instance.freeCinematicCamera.transform.rotation = Quaternion.Slerp(StartOfRound.Instance.freeCinematicCamera.transform.rotation, compass.rotation, 3f * Time.deltaTime);
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
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
        [HarmonyPriority(Priority.Last)]
        private static bool InterceptScroll(PlayerControllerB __instance, InputAction.CallbackContext context)
        {
            if (!__instance.IsOwner || __instance != StartOfRound.Instance.localPlayerController) return true;
            if (!context.performed) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            float scrollDelta = Mathf.Clamp(context.ReadValue<float>(), -1f, 1f);
            if (scrollDelta == 0f) return false;
            if (FreeCamClass.shouldChangeFOV)
            {
                FreeCamClass.AdjustFOV(scrollDelta > 0 ? -FreeCamFOVInfo.ScrollStep : FreeCamFOVInfo.ScrollStep);
            }
            else
            {
                FreeCamClass.AdjustSpeed(scrollDelta);
            }
            return false;
        }

        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void FreecamLateUpdatePatch(PlayerControllerB __instance)
        {
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return;
            if (__instance != StartOfRound.Instance.localPlayerController) return;

            var compass = StartOfRound.Instance.freeCinematicCameraTurnCompass;
            StartOfRound.Instance.freeCinematicCamera.transform.rotation = compass.rotation;
            StartOfRound.Instance.freeCinematicCamera.transform.position = compass.position;
        }

        [HarmonyPatch("PlayerLookInput")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static void PlayerLookInputPatch(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return;

            originalSensitivity = IngamePlayerSettings.Instance.settings.lookSensitivity;
            IngamePlayerSettings.Instance.settings.lookSensitivity = Mathf.Clamp(Mathf.RoundToInt(IngamePlayerSettings.Instance.settings.lookSensitivity * FreeCamClass.GetFOVSensitivityMultiplier()), 1, originalSensitivity);
        }

        [HarmonyPatch("PlayerLookInput")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Postfix(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return;

            // Restore original sensitivity immediately after
            IngamePlayerSettings.Instance.settings.lookSensitivity = originalSensitivity;
        }
        
        [HarmonyPatch("Crouch_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchCrouch(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("Jump_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchJump(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchInteract(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ClickHoldInteraction")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool BlockHoldInteract(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ItemSecondaryUse_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchItemSecUSe(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ItemTertiaryUse_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchTertiary(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ActivateItem_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchActivateItemPerf(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ActivateItem_canceled")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchActivateItemCanc(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("Discard_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchDiscard(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("ScrollMouse_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchScroll(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("InspectItem_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchInspect(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("Emote1_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchEmote1(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("Emote2_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchEmote2(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("UseUtilitySlot_performed")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool PatchUtility(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            return false;
        }

        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool BlockHoverTip(PlayerControllerB __instance)
        {
            if (__instance != StartOfRound.Instance.localPlayerController) return true;
            if (!FreeCamClass.isInFreeCam || FreeCamClass.lockFreeCam) return true;

            if (__instance.hoveringOverTrigger != null)
            {
                __instance.hoveringOverTrigger.StopInteraction();
                __instance.previousHoveringOverTrigger = __instance.hoveringOverTrigger;
                __instance.hoveringOverTrigger = null;
            }
            __instance.cursorIcon.enabled = false;
            __instance.cursorTip.text = "";

            return false;
        }
    }
}
