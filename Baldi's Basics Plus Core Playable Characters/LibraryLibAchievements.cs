using LibraryLibrary.AchievementsAPI;
using System.Collections;

namespace BBP_Playables.Core;

internal static class LibraryLibAchievements
{
    internal static IEnumerator CreateAchievements(this PlayableCharsPlugin plugin)
    {
        yield return "Adding Achievements...";
        LLAchievementManager.AddAchievement("playablecharactersmod_cylnloon_turnedtables", new LLAchievement("ach_cylnloon_turnedtables", "ach_cylnloon_turnedtables_desc", "playablecharactersmod_cylnloon_turnedtables", AchievementSection.Other, AchievementRarity.Uncommon, false));
        LLAchievementManager.AddAchievement("playablecharactersmod_partygoer_falsegameover", new LLAchievement("ach_partygoer_falsegameover", "ach_partygoer_falsegameover_desc", "playablecharactersmod_partygoer_falsegameover", AchievementSection.Characters, AchievementRarity.Uncommon, true));
        LLAchievementManager.AddAchievement("playablecharactersmod_thinker_maxedoutprofit", new LLAchievement("ach_thinker_maxedoutprofit", "ach_thinker_maxedoutprofit_desc", "playablecharactersmod_thinker_maxedoutprofit", AchievementSection.Other, AchievementRarity.Epic, false));
        LLAchievementManager.AddAchievement("playablecharactersmod_thinker_nocheating", new LLAchievement("ach_thinker_nocheating", "ach_thinker_nocheating_desc", "playablecharactersmod_thinker_nocheating", AchievementSection.Activities, AchievementRarity.Rare, true));
        LLAchievementManager.AddAchievement("playablecharactersmod_backpacker_antihoarding", new LLAchievement("ach_backpacker_antihoarding", "ach_backpacker_antihoarding_desc", "playablecharactersmod_backpacker_antihoarding", AchievementSection.Other, AchievementRarity.Epic, false));
        LLAchievementManager.AddAchievement("playablecharactersmod_tinkereer_fooledbaldi", new LLAchievement("ach_tinkereer_fooledbaldi", "ach_tinkereer_fooledbaldi_desc", "playablecharactersmod_tinkereer_fooledbaldi", AchievementSection.Characters, AchievementRarity.Uncommon, false));
        LLAchievementManager.AddAchievement("playablecharactersmod_thetestsubject_maintained", new LLAchievement("ach_thetestsubject_maintained", "ach_thetestsubject_maintained_desc", "playablecharactersmod_thetestsubject_maintained", AchievementSection.Other, AchievementRarity.Legendary, false));
        LLAchievementManager.AddAchievement("playablecharactersmod_thetestsubject_impossiblesticker", new LLAchievement("ach_thetestsubject_impossiblesticker", "ach_thetestsubject_impossiblesticker_desc", "playablecharactersmod_thetestsubject_impossiblesticker", AchievementSection.Stickers, AchievementRarity.Legendary, true)); // By how the decision for this rarity achievement to be a mythic is not reasonable.
        LLAchievementManager.AddAchievement("playablecharactersmod_magicalstudent_jobless", new LLAchievement("ach_magicalstudent_jobless", "ach_magicalstudent_jobless_desc", "playablecharactersmod_magicalstudent_jobless", AchievementSection.Characters, AchievementRarity.Common, false));
        plugin.gameObject.AddComponent<AchievementTracker>();
    }
}