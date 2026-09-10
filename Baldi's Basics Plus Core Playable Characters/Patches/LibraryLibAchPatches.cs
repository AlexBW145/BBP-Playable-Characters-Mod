using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;

namespace BBP_Playables.Core.Patches;

[ConditionalPatchMod("skid.librarylib"), HarmonyPatch]
internal static class LibraryLibAchPatches
{
    [HarmonyPatch(typeof(PlaceholderWinManager), "BeginPlay"), HarmonyPostfix]
    private static void CallInVictory()
    {
        AchievementTracker.Instance?.QueueAchievement((int)AchievementTracker.AchievementQueue.HIDEANDSEEK_CLEAR);
    }
    [HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel"), HarmonyPostfix]
    private static void FloorCleared()
    {
        AchievementTracker.Instance?.QueueAchievement((int)AchievementTracker.AchievementQueue.LEVEL_CLEAR);
    }
    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.Initialize)), HarmonyPostfix]
    private static void Restart() => AchievementTracker.Instance?.RestartTriggers();
    [HarmonyPatch(typeof(TestSubjectMan), nameof(TestSubjectMan.MessUpAndIncreaseTimeScale)), HarmonyPostfix]
    private static void MessUpAndLock()
    {
        AchievementTracker.Instance?.QueueAchievement((int)AchievementTracker.AchievementQueue.MAINTAINED_INCORRECT);
    }
    [HarmonyPatch(typeof(BackpackerBackpack), "Update"), HarmonyPostfix]
    private static void TooMuchAndLock(PlayerManager ___pm, ValueModifier ___modifier)
    {
        if (___pm != null && !BaseGameManager.Instance.InPitstop())
        {
            if (___modifier.multiplier <= 0.75f && !AchievementTracker.Instance.HasTrigger((int)AchievementTracker.AchievementTrigger.ANTIHOARDING_FAILED))
                AchievementTracker.Instance?.RegisterTrigger((int)AchievementTracker.AchievementTrigger.ANTIHOARDING_FAILED);
        }
    }
    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebooks)), HarmonyPostfix]
    private static void CheatedOrNot(BaseGameManager __instance)
    {
        if (__instance.FoundNotebooks >= __instance.Ec.notebookTotal && __instance.Ec.notebookTotal >= 9)
            AchievementTracker.Instance?.QueueAchievement((int)AchievementTracker.AchievementQueue.NOCHEATING_CLEARED);
    }
    [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.AddPoints), [typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool)]), HarmonyPostfix]
    private static void RicherMoment(int points, int player, bool playAnimation, bool includeInLevelTotal, bool multiply, int[] ___thoughtPoints)
    {
        if (PlrPlayableCharacterVars.GetPlayable(player)?.GetPlayer().GetComponent<ThinkerAbility>() != null)
        {
            if (AchievementTracker.Instance != null)
            {
                AchievementTracker.Instance.ytpsTotalRun += points;
                if (AchievementTracker.Instance.ytpsTotalRun >= 25000)
                    AchievementTracker.Instance?.QueueAchievement((int)AchievementTracker.AchievementQueue.MAXEDOUTPROFIT);
            }
        }
    }
}
