using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using SimpleFreeCam.Input;
using SimpleFreeCam.Patches;

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

        internal ManualLogSource logSource;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            InputActionsInstance = new SimpleFreeCamInputs();
            InputActionsInstance.PixelIndieDev_FreeCamKey.performed += FreeCamClass.SetFreeCamera_performed;
            InputActionsInstance.PixelIndieDev_FreeCamMovementKey.performed += FreeCamClass.ToggleFreecamMovement_performed;
            InputActionsInstance.PixelIndieDev_FreeCamFOVModifierKey.performed += ctx => FreeCamClass.ToggleFOVSwitch_performed(true);
            InputActionsInstance.PixelIndieDev_FreeCamFOVModifierKey.canceled += ctx => FreeCamClass.ToggleFOVSwitch_performed(false);
            InputActionsInstance.PixelIndieDev_FreeCamResetFOVKey.performed += FreeCamClass.ResetFOV_performance;
            InputActionsInstance.PixelIndieDev_FreeCamTeleportToPlayerKey.performed += FreeCamClass.TeleportFreeCamToPlayer_performance;

            FreeCamConfigEntry = Config.Bind("General", "Reset speed on freecam", false, "When enabled, the freecam movement speed resets to the default speed every time freecam is activated.");
            var checkbox = new BoolCheckBoxConfigItem(FreeCamConfigEntry, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(checkbox);
            FreeCamConfigEntryFloat = Config.Bind("General", "Default freecam speed", FreeCamSpeedInfo.DefaultSpeed, $"The base movement speed of the freecam. Scroll while in freecam to adjust speed (range: {FreeCamSpeedInfo.MinSpeed} - {FreeCamSpeedInfo.MaxSpeed}, in steps of  {FreeCamSpeedInfo.ScrollStep}).");
            var slider = new FloatStepSliderConfigItem(FreeCamConfigEntryFloat, new FloatStepSliderOptions
            {
                Min = FreeCamSpeedInfo.MinSpeed,
                Max = FreeCamSpeedInfo.MaxSpeed,
                Step = FreeCamSpeedInfo.ScrollStep,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(slider);

            logSource = BepInEx.Logging.Logger.CreateLogSource(ModInfo.modGUID);

            harmony.PatchAll(typeof(SimpleFreeCamPatchBase));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(PlayerControllerPatch));
            harmony.PatchAll(typeof(VehicleControllerPatch));

            harmony.PatchAll(typeof(NetworkPatch));

            logSource.LogInfo(ModInfo.modName + " (version - " + ModInfo.modVersion + ")" + ": patches applied successfully");
        }
    }
}
