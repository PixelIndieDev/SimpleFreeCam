using HarmonyLib;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void OnGameEntered(StartOfRound __instance)
        {
            __instance.StartCoroutine(FreeCamClass.SetupFreecam());
        }
    }
}
