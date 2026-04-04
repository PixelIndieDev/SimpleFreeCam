using GameNetcodeStuff;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace SimpleFreeCam
{
    internal static class FreeCamClass
    {
        private static GameObject HUDObject;
        private static SkinnedMeshRenderer playerModel;
        private static Transform playerAudioListener;

        private static PlayerControllerB playerController = null;
        internal static bool isInVehicle = false;

        private static Transform mainCameraTransform;
        private static Transform freeCameraTransform;

        private static Camera FreeCam;

        internal static bool lockFreeCam = false;
        internal static bool isInFreeCam = false;

        internal static float freecamSpeed = 3f;
        internal static bool hasBootedAlready = false;

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
        }

        public static void SetFreeCamera_performed(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
            {
                return;
            }

            typeof(PlayerControllerB).GetMethod("SetFreeCamera_performed", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(playerController, new object[] { obj });

            if (isInFreeCam)
            {
                DisableFreecam();
            }
            else if (!playerController.isPlayerDead)
            {
                EnableFreecam();
            }
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

        public static void SetPlayerController(PlayerControllerB controller)
        {
            playerController = controller;
        }

        public static void SetInVehicle(bool inVehicle)
        {
            isInVehicle = inVehicle;
        }

        private static void EnableFreecam()
        {
            //alays start with freecamMovement true
            lockFreeCam = false;
            isInFreeCam = true;

            //reset each time on enable
            if (SimpleFreeCamPatchBase.instance.FreeCamConfigEntry.Value || !hasBootedAlready)
            {
                hasBootedAlready = true;
                freecamSpeed = SimpleFreeCamPatchBase.instance.FreeCamConfigEntryFloat.Value;
            }

            playerController.isFreeCamera = true;
            StartOfRound.Instance.SwitchCamera(FreeCam);

            HUDObject.SetActive(false);
            playerController.thisPlayerModelArms.enabled = false;
            playerModel.shadowCastingMode = ShadowCastingMode.On;
            playerAudioListener.SetParent(freeCameraTransform, false);

            HUDManager.Instance.HideHUD(true);
        }

        public static void DisableFreecam()
        {
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
            if (lockFreeCam)
            {
                playerController.isFreeCamera = false;
            }
            else
            {
                playerController.isFreeCamera = true;
            }
        }

        internal static void AdjustSpeed(float scrollDelta)
        {
            if (!isInFreeCam || lockFreeCam) return;

            freecamSpeed = Mathf.Clamp(freecamSpeed + (scrollDelta > 0 ? FreeCamSpeedInfo.ScrollStep : -FreeCamSpeedInfo.ScrollStep), FreeCamSpeedInfo.MinSpeed, FreeCamSpeedInfo.MaxSpeed);
        }

        internal static float GetSpeed() => freecamSpeed;
    }
}
