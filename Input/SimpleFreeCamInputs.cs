using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace SimpleFreeCam.Input
{
    public class SimpleFreeCamInputs : LcInputActions
    {
        [InputAction(KeyboardControl.C, Name = "Enable/Disable FreeCam")]
        public InputAction PixelIndieDev_FreeCamKey { get; set; }

        [InputAction(KeyboardControl.Z, Name = "Enable/Disable FreeCam Player Movement")]
        public InputAction PixelIndieDev_FreeCamMovementKey { get; set; }
    }
}
