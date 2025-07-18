﻿using BBP_Playables.Core;
using BCarnellChars;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using MTM101BaldAPI.Reflection;
using BCarnellChars.OtherStuff;

namespace BBP_Playables.Modded.Patches
{
    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel")]
    class BCPPUnlocks
    {
        static void Prefix(BaseGameManager __instance)
        {
            if (CoreGameManager.Instance.currentMode == Mode.Free || __instance.levelObject == null) return;
            if (__instance is BasementGameManager && CoreGameManager.Instance.SaveEnabled && MTM101BaldiDevAPI.SaveGamesEnabled
                && __instance.levelObject.name == BasePlugin.Instance.lBasement.name 
                && CoreGameManager.Instance.currentMode != Mode.Free && __instance.levelObject.finalLevel)
                PlayableCharsPlugin.UnlockCharacter(Plugin.coreInfo, "The Dweller");
            if (__instance.levelObject.finalLevel && (PlrPlayableCharacterVars.GetLocalPlayable().GetPlayer().itm.items.Contains(PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"))
                || CoreGameManager.Instance.currentLockerItems.Contains(PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"))))
                PlayableCharsPlugin.UnlockCharacter(Plugin.coreInfo, "The Main Protagonist");
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(MainMenu), "Start")]
    class DwellerAlreadyUnlockedCheck
    {
        static void Postfix()
        {
            /*if (DwellerAbility.crisped.Count > 0)
                foreach (var crisp in DwellerAbility.crisped)
                {
                    DwellerAbility.crisped.Remove(crisp.Key);
                    GameObject.Destroy(crisp.Value);
                }*/
            if (!PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Dweller").value.unlocked && BCPPSave.Instance.basementCompleted)
                PlayableCharsPlugin.UnlockCharacter(Plugin.coreInfo, "The Dweller");
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(Pickup))]
    class DwellerAbility
    {
        internal static Dictionary<ItemObject, Sprite> crisped = new Dictionary<ItemObject, Sprite>();
        [HarmonyPatch("Start")]
        [HarmonyPatch(nameof(Pickup.AssignItem))]
        static void Postfix(Pickup __instance)
        {
            if (PlayableCharsPlugin.IsRandom && !CoreGameManager.Instance.readyToStart) return;
            if (CoreGameManager.Instance.GetPlayer(0) != null && PlrPlayableCharacterVars.GetLocalPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thedweller"
                && __instance.icon != null && __instance.item.itemType != Items.None) {
                Sprite crisp = crisped.ContainsKey(__instance.item) ? crisped[__instance.item] : Sprite.Create(__instance.item.itemSpriteLarge.texture, new Rect(0f, 0f, __instance.item.itemSpriteLarge.texture.width, __instance.item.itemSpriteLarge.texture.height), Vector2.one / 2f, 80f, 0u, SpriteMeshType.FullRect);
                crisp.name = "CrispedItemIconSpr_" + __instance.item.name;
                if (!crisped.ContainsKey(__instance.item))
                    crisped.Add(__instance.item, crisp);
                __instance.icon.spriteRenderer.sprite = crisp;
                //__instance.icon.spriteRenderer.material = PlayableCharsPlugin.assetMan.Get<Material>("DwellerMapMat");
            }
        }
    }
}