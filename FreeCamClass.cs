using GameNetcodeStuff;
using SimpleFreeCam.Patches;
using SimpleFreeCam.UI;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SimpleFreeCam
{
    internal static class FreeCamClass
    {
        private static GameObject HUDObject;
        private static SkinnedMeshRenderer playerModel;
        private static Transform playerAudioListener;

        private static PlayerControllerB playerController = null;
        internal static bool shouldChangeFOV = false;

        private static Transform mainCameraTransform;
        private static Transform freeCameraTransform;

        private static Camera FreeCam;

        internal static bool lockFreeCam = false;
        internal static bool isInFreeCam = false;

        internal static float freecamSpeed = FreeCamSpeedInfo.DefaultSpeed;
        private static bool hasChangedSpeed = false;
        private static bool hasBootedAlready = false;

        private static float PlayerSavedCameraUp = 0f;
        public static float FreeCamSavedCameraUp = 0f;
        private static float freecamFOV = 60f;

        private static readonly FieldInfo cameraUpField = typeof(PlayerControllerB).GetField("cameraUp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerator SetupFreecam()
        {
            yield return new WaitUntil(() => StartOfRound.Instance.localPlayerController != null);

            playerController = StartOfRound.Instance.localPlayerController;

            HUDObject = GameObject.Find("Systems").transform.Find("Rendering/PlayerHUDHelmetModel").gameObject;
            playerModel = playerController.transform.Find("ScavengerModel/LOD1").GetComponent<SkinnedMeshRenderer>();

            mainCameraTransform = playerController.transform.Find("ScavengerModel/metarig/CameraContainer/MainCamera");
            freeCameraTransform = GameObject.Find("FreeCameraCinematic").transform;

            FreeCam = StartOfRound.Instance.freeCinematicCamera;
            playerAudioListener = playerController.transform.Find("ScavengerModel/metarig/CameraContainer/MainCamera/PlayerAudioListener");

            if (FreeCamHUD.Instance == null)
            {
                var hudGo = new GameObject("SimpleFreeCamHUD");
                hudGo.AddComponent<FreeCamHUD>();
            }
        }

        public static void SetFreeCamera_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
            {
                return;
            }

            typeof(PlayerControllerB).GetMethod("SetFreeCamera_performed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(playerController, new object[] { obj });

            if (!playerController.inTerminalMenu && !playerController.isTypingChat)
            {
                if (isInFreeCam)
                {
                    DisableFreecam();
                }
                else if (!playerController.isPlayerDead)
                {
                    EnableFreecam();
                }
            }
        }

        public static void ToggleFOVSwitch_performed(bool isHeldDown)
        {
            if (playerController == null || !isInFreeCam)
            {
                return;
            }

            shouldChangeFOV = isHeldDown;
        }

        public static void ToggleFreecamMovement_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
            {
                return;
            }

            if (playerController == null || !isInFreeCam)
            {
                return;
            }

            // player is in freecam
            lockFreeCam = !lockFreeCam;
            DisableFreecamOnLock();
        }

        public static void ResetFOV_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
            {
                return;
            }

            if (playerController == null || !isInFreeCam)
            {
                return;
            }

            freecamFOV = FreeCamFOVInfo.DefaultFOV;
            FreeCam.fieldOfView = FreeCamFOVInfo.DefaultFOV;
        }

        public static void ResetFreeCam()
        {
            hasBootedAlready = false;
            FreeCamSavedCameraUp = 0f;
        }

        public static void TeleportFreeCamToPlayer_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
            {
                return;
            }

            if (playerController == null || !isInFreeCam)
            {
                return;
            }

            TeleportFreeCamToPlayer();
        }

        public static void SetFreecamLocation(Vector3 position)
        {
            if (isInFreeCam && !lockFreeCam)
            {
                freeCameraTransform.position = position;
                StartOfRound.Instance.freeCinematicCameraTurnCompass.position = position;
            }
        }

        public static void SetFreecamFOV(float fov)
        {
            freecamFOV = Mathf.Clamp(fov, FreeCamFOVInfo.MinFOV, FreeCamFOVInfo.MaxFOV);
            if (isInFreeCam) FreeCam.fieldOfView = freecamFOV;
        }

        public static float GetFreecamFOV() => freecamFOV;

        public static void TeleportFreeCamToPlayer()
        {
            TeleportFreeCamToPlayer(-0.5f);
        }

        private static void ResetFreeCamRotation()
        {
            Quaternion newRotation;
            if (playerController != null)
            {
                newRotation = Quaternion.LookRotation(GetPlayerForwardWithoutPitch());
            } else
            {
                newRotation = default;
            }

            freeCameraTransform.rotation = newRotation;
            StartOfRound.Instance.freeCinematicCameraTurnCompass.rotation = newRotation;
        }

        private static void ResetFreeCamTransform(float offset)
        {
            TeleportFreeCamToPlayer(offset);
            ResetFreeCamRotation();
        }

        public static void TeleportFreeCamToPlayer(float offset)
        {
            Vector3 playerCamPosition = playerController.gameplayCamera.transform.position;
            playerCamPosition += GetPlayerForwardWithoutPitch() * offset;
            SetFreecamLocation(playerCamPosition);
        }

        private static Vector3 GetPlayerForwardWithoutPitch()
        {
            Vector3 forward = playerController.gameplayCamera.transform.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        private static void EnableFreecam()
        {
            //alays start with freecamMovement true
            lockFreeCam = false;
            isInFreeCam = true;
            shouldChangeFOV = false;

            FreeCam.cullingMask = playerController.gameplayCamera.cullingMask;

            if (!hasBootedAlready)
            {
                hasBootedAlready = true;
                freecamSpeed = SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloat.Value;

                //teleport freecam to player on first "boot"
                ResetFreeCamTransform(-1);
            }

            //reset on each open
            if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryResetTransform.Value)
            {
                ResetFreeCamTransform(-1);
            } else
            {
                //reset when too far away
                if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryResetDistance.Value)
                {
                    //camera is further away
                    if (ShouldDistanceReset())
                    {
                        ResetFreeCamTransform(-1);
                    }
                }
            }

            //reset each time on enable
            if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntry.Value)
            {
                freecamSpeed = SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloat.Value;
            } else
            {
                if (!hasChangedSpeed)
                {
                    freecamSpeed = SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloat.Value;
                }
            }

            SwitchCameraUpField(true);

            FreeCam.fieldOfView = freecamFOV;

            playerController.isFreeCamera = true;

            StartOfRound.Instance.freeCinematicCameraTurnCompass.rotation = freeCameraTransform.rotation;
            StartOfRound.Instance.SwitchCamera(FreeCam);

            FreeCamHUD.ResetHideUI();
            HUDObject.SetActive(false);
            playerController.thisPlayerModelArms.enabled = false;
            playerModel.shadowCastingMode = ShadowCastingMode.On;
            playerAudioListener.SetParent(freeCameraTransform, false);

            HUDManager.Instance.HideHUD(true);
        }

        private static bool ShouldDistanceReset()
        {
            if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntryResetDistance.Value)
            {
                float SqrDistanceThreshold = SimpleFreeCamPatchBase.instance.FreeCamConfigEntryInt.Value * SimpleFreeCamPatchBase.instance.FreeCamConfigEntryInt.Value;
                Vector3 camDistanceBetween = playerController.transform.localPosition - FreeCam.transform.localPosition;
                return camDistanceBetween.sqrMagnitude > SqrDistanceThreshold;
            }
            return false;
        }

        public static void TickDistanceWarning()
        {
            FreeCamHUD.TriggerDistanceWarning(ShouldDistanceReset());
        }

        private static void SwitchCameraUpField(bool switchToFreeCam)
        {
            //should the freecam pitch be used
            if (switchToFreeCam)
            {
                //save player character pitch
                PlayerSavedCameraUp = (float)cameraUpField.GetValue(playerController);

                //set camera pitch to freecam pitch
                cameraUpField.SetValue(playerController, FreeCamSavedCameraUp);
            } else
            {
                //save freecam pitch
                FreeCamSavedCameraUp = (float)cameraUpField.GetValue(playerController);

                //set to player character pitch
                cameraUpField.SetValue(playerController, PlayerSavedCameraUp);
            }
        }

        public static void DisableFreecam()
        {
            if (!lockFreeCam)
            {
                SwitchCameraUpField(false);
            }

            lockFreeCam = false;
            isInFreeCam = false;

            playerController.isFreeCamera = false;
            StartOfRound.Instance.freeCinematicCamera.enabled = false;
            StartOfRound.Instance.SwitchCamera(playerController.isPlayerDead ? StartOfRound.Instance.spectateCamera : playerController.gameplayCamera);

            HUDObject.SetActive(true);
            playerController.thisPlayerModelArms.enabled = true;
            playerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            playerAudioListener.SetParent(mainCameraTransform, false);

            HUDManager.Instance.HideHUD(playerController.isPlayerDead);
        }

        public static void DisableFreecamOnLock()
        {
            isInFreeCam = true;
            SwitchCameraUpField(!lockFreeCam);
            playerController.isFreeCamera = !lockFreeCam;
        }

        internal static void AdjustSpeed(float scrollDelta)
        {
            if (!isInFreeCam || lockFreeCam) return;

            freecamSpeed = Mathf.Clamp(freecamSpeed + (scrollDelta > 0 ? FreeCamSpeedInfo.ScrollStep : -FreeCamSpeedInfo.ScrollStep), FreeCamSpeedInfo.MinSpeed, FreeCamSpeedInfo.MaxSpeed);
            if (!hasChangedSpeed) hasChangedSpeed = true;
        }

        internal static void UpdateSpeed(float newSpeed)
        {
            if (!hasChangedSpeed) freecamSpeed = newSpeed;
        }

        internal static bool IsInVehicle()
        {
            return playerController != null && playerController.inVehicleAnimation;
        }

        internal static float GetSpeed() => freecamSpeed;

        internal static void AdjustFOV(float delta)
        {
            if (!isInFreeCam || lockFreeCam) return;
            SetFreecamFOV(freecamFOV + delta);
        }

        internal static float GetFOVSensitivityMultiplier()
        {
            return freecamFOV / FreeCamFOVInfo.DefaultFOV;
        }
    }
}
