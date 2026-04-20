using HarmonyLib;

namespace SimpleFreeCam.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void OnGameEntered(StartOfRound __instance)
        {
            __instance.StartCoroutine(FreeCamClass.SetupFreecam());
        }

        //reset the freecam on level change
        [HarmonyPatch("ChangeLevel")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void OnChangeLevel(StartOfRound __instance)
        {
            FreeCamClass.ResetFreeCam();
        }
    }
}
