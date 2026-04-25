using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using SimpleFreeCam.Input;
using SimpleFreeCam.Patches;
using SimpleFreeCam.UI;
using UnityEngine;

namespace SimpleFreeCam
{
    [BepInPlugin(ModInfo.modGUID, ModInfo.modName, ModInfo.modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils")]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class SimpleFreeCamPatchBase : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(ModInfo.modGUID);
        public static SimpleFreeCamPatchBase instance;
        internal static SimpleFreeCamInputs InputActionsInstance;
        internal ConfigEntry<bool> FreeCamConfigEntry;
        internal ConfigEntry<float> FreeCamConfigEntryFloat;
        internal ConfigEntry<int> FreeCamConfigEntryInt;
        internal ConfigEntry<bool> FreeCamConfigEntryResetTransform;
        internal ConfigEntry<bool> FreeCamConfigEntryResetDistance;
        internal ConfigEntry<bool> FreeCamConfigEntryHideUI;
        internal ConfigEntry<bool> FreeCamConfigEntryDefaultUIVisiblity;
        internal ConfigEntry<float> FreeCamConfigEntryFloatOpacity;
        internal ConfigEntry<bool> FreeCamConfigDisableShowWarningDistance;
        internal ConfigEntry<bool> FreeCamConfigDisableShowWarningReset;

        internal ManualLogSource logSource;

        void Awake()
        {
            logSource = BepInEx.Logging.Logger.CreateLogSource(ModInfo.modGUID);

            if (instance == null)
            {
                instance = this;
            }

            InputActionsInstance = new SimpleFreeCamInputs();
            InputActionsInstance.PixelIndieDev_FreeCamKey.performed += FreeCamClass.SetFreeCamera_performed;
            InputActionsInstance.PixelIndieDev_FreeCamMovementKey.performed += FreeCamClass.ToggleFreecamMovement_performed;
            InputActionsInstance.PixelIndieDev_FreeCamFOVModifierKey.performed += ctx => FreeCamClass.ToggleFOVSwitch_performed(true);
            InputActionsInstance.PixelIndieDev_FreeCamFOVModifierKey.canceled += ctx => FreeCamClass.ToggleFOVSwitch_performed(false);
            InputActionsInstance.PixelIndieDev_FreeCamResetFOVKey.performed += FreeCamClass.ResetFOV_performed;
            InputActionsInstance.PixelIndieDev_FreeCamTeleportToPlayerKey.performed += FreeCamClass.TeleportFreeCamToPlayer_performed;
            InputActionsInstance.PixelIndieDev_FreeCamHideUI.performed += FreeCamHUD.HideUI_performed;

            FreeCamConfigEntryResetTransform = Config.Bind("Transform", "Reset freecam location and rotation on freecam", false, "When enabled, the freecam will teleport to the player's current position and rotation every time freecam is activated.");
            var checkbox2 = new BoolCheckBoxConfigItem(FreeCamConfigEntryResetTransform, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox2);

            FreeCamConfigEntryResetDistance = Config.Bind("Transform", "Reset freecam when too far away", false, $"When enabled, the freecam will automatically teleport to the player's position and rotation if it exceeds the 'Max freecam distance'.");
            var checkbox3 = new BoolCheckBoxConfigItem(FreeCamConfigEntryResetDistance, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox3);

            FreeCamConfigEntryInt = Config.Bind("Transform", "Max freecam distance", 100, $"The maximum distance (in meters) the freecam can be from the player when activating freecam before it resets.");
            var slider2 = new IntSliderConfigItem(FreeCamConfigEntryInt, new IntSliderOptions
            {
                Min = 10,
                Max = 300,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(slider2);

            FreeCamConfigEntry = Config.Bind("Speed", "Reset speed on freecam", false, "When enabled, the freecam movement speed resets to the default speed every time freecam is activated.");
            var checkbox = new BoolCheckBoxConfigItem(FreeCamConfigEntry, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox);

            FreeCamConfigEntryFloat = Config.Bind("Speed", "Default freecam speed", FreeCamSpeedInfo.DefaultSpeed, $"The base movement speed of the freecam. Scroll while in freecam to adjust speed (range: {FreeCamSpeedInfo.MinSpeed} - {FreeCamSpeedInfo.MaxSpeed}, in steps of  {FreeCamSpeedInfo.ScrollStep}).");
            FreeCamConfigEntryFloat.SettingChanged += (sender, args) =>
            {
                FreeCamClass.UpdateSpeed(FreeCamConfigEntryFloat.Value);
            };
            var slider = new FloatStepSliderConfigItem(FreeCamConfigEntryFloat, new FloatStepSliderOptions
            {
                Min = FreeCamSpeedInfo.MinSpeed,
                Max = FreeCamSpeedInfo.MaxSpeed,
                Step = FreeCamSpeedInfo.ScrollStep,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(slider);

            FreeCamConfigEntryFloatOpacity = Config.Bind("UI", "Opacity", 0.9f, "Controls the UI opacity while in freecam");
            FreeCamConfigEntryFloatOpacity.SettingChanged += (sender, args) =>
            {
                FreeCamHUD.UpdateOpacity(FreeCamConfigEntryFloatOpacity.Value);
            };
            var sliderOpacity = new FloatStepSliderConfigItem(FreeCamConfigEntryFloatOpacity, new FloatStepSliderOptions
            {
                Min = 0.05f,
                Max = 1f,
                Step = 0.05f,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(sliderOpacity);

            FreeCamConfigEntryDefaultUIVisiblity = Config.Bind("UI", "Is UI visible by default", true, $"When enabled, the freecam UI will be visible by default.");
            var checkbox5 = new BoolCheckBoxConfigItem(FreeCamConfigEntryDefaultUIVisiblity, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox5);

            FreeCamConfigEntryHideUI = Config.Bind("UI", "Reset UI visibility on freecam", false, "When enabled, the freecam UI resets to the default visibility every time freecam is activated.");
            var checkbox4 = new BoolCheckBoxConfigItem(FreeCamConfigEntryHideUI, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox4);

            FreeCamConfigDisableShowWarningReset = Config.Bind("Warnings", "Disable showing reset transform warning", false, "When enabled, the freecam UI will never show the reset transform warning on screen.");
            var checkbox7 = new BoolCheckBoxConfigItem(FreeCamConfigDisableShowWarningReset, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox7);

            FreeCamConfigDisableShowWarningDistance = Config.Bind("Warnings", "Disable showing distance warning", false, "When enabled, the freecam UI will never show the far away distance warning on screen.");
            var checkbox6 = new BoolCheckBoxConfigItem(FreeCamConfigDisableShowWarningDistance, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox6);

            harmony.PatchAll(typeof(SimpleFreeCamPatchBase));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(PlayerControllerPatch));
            harmony.PatchAll(typeof(VehicleControllerPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));

            harmony.PatchAll(typeof(NetworkPatch));

            logSource.LogInfo(ModInfo.modName + " (version - " + ModInfo.modVersion + ")" + ": patches applied successfully");
        }
    }
}
