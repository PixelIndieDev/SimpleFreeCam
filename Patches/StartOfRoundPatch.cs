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

        //reset the freecam on level change
        [HarmonyPatch("ChangeLevel")]
        [HarmonyPostfix]
        private static void OnChangeLevel(StartOfRound __instance)
        {
            FreeCamClass.ResetFreeCam();
        }
    }
}
