using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace SimpleFreeCam.Input
{
    public class SimpleFreeCamInputs : LcInputActions
    {
        [InputAction(KeyboardControl.C, Name = "Enable/Disable FreeCam (Toggle)")]
        public InputAction PixelIndieDev_FreeCamKey { get; set; }

        [InputAction(KeyboardControl.Z, Name = "Enable/Disable FreeCam Player Movement (Toggle)")]
        public InputAction PixelIndieDev_FreeCamMovementKey { get; set; }

        [InputAction(KeyboardControl.T, Name = "Teleport FreeCam To Player")]
        public InputAction PixelIndieDev_FreeCamTeleportToPlayerKey { get; set; }

        [InputAction(KeyboardControl.LeftAlt, Name = "Scroll Changes FOV (Hold)")]
        public InputAction PixelIndieDev_FreeCamFOVModifierKey { get; set; }

        [InputAction(KeyboardControl.F, Name = "Reset FOV")]
        public InputAction PixelIndieDev_FreeCamResetFOVKey { get; set; }
    }
}
