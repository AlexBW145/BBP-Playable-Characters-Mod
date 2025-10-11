using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BBP_Playables.Core;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.ModPatches;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace BBP_Playables.Modded.Patches
{
    [ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.bbextracontent"), HarmonyPatch(typeof(MagicObject), "EntityTriggerEnter")]
    class UnlockMagical
    {
        static void Postfix(Collider other, ref MagicalStudent ___student, ref bool ___leftStudent)
        {
            if (other.transform == ___student.transform && ___leftStudent)
                PlayableCharsPlugin.UnlockCharacter(Plugin.coreInfo, "Magical Student");
        }
    }
    [ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.bbextracontent"), HarmonyPatch(typeof(PlayerVisual), nameof(PlayerVisual.Initialize))]
    class PlayerVisualPatch
    {
        private static FieldInfo _emotions = AccessTools.DeclaredField(typeof(PlayerVisual), "emotions");
        public static readonly Dictionary<PlayableCharacter, Sprite[]> playableEmotions = new Dictionary<PlayableCharacter, Sprite[]>();
        private static void Postfix(PlayerVisual __instance, ref Sprite[] ___emotions)
        {
            if (!playableEmotions.ContainsKey(PlayableCharsPlugin.Instance.Character)) return;
            ___emotions = playableEmotions[PlayableCharsPlugin.Instance.Character];
            __instance.SetEmotion(0);
        }
        [HarmonyPatch(typeof(PlayableCharacterComponent), "Start"), HarmonyPostfix]
        private static void RandomizerSetVisual(ref PlayerManager ___pm)
        {
            if (!playableEmotions.ContainsKey(PlayableCharsPlugin.Instance.Character) || !PlayableCharsPlugin.IsRandom) return;
            var visual = PlayerVisual.GetPlayerVisual(___pm.playerNumber);
            _emotions.SetValue(visual, playableEmotions[PlayableCharsPlugin.Instance.Character]);
            visual.SetEmotion(0);
        }
    }
}
