using BBP_Playables.Core;
using BCarnellChars;
using HarmonyLib;
using MTM101BaldAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BBP_Playables.Modded.Patches;

// Better unlock method for Protagonist is soon, useless.
/*[ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel")]
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
}*/

[ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch]
class DwellerAlreadyUnlockedCheck
{
    [HarmonyPatch(typeof(MainMenu), "Start")]
    [HarmonyPatch(typeof(BCPPSave), "Save")]
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
        if (!PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Main Protagonist").value.unlocked && BCPPSave.Instance.death)
            PlayableCharsPlugin.UnlockCharacter(Plugin.coreInfo, "The Main Protagonist");
    }
}

[ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(Pickup))]
class DwellerAbility
{
    private static readonly Dictionary<ItemObject, Sprite> crisped = new Dictionary<ItemObject, Sprite>();
    [HarmonyPatch("Start")]
    [HarmonyPatch(nameof(Pickup.AssignItem))]
    private static void Postfix(Pickup __instance)
    {
        if (PlayableCharsPlugin.IsRandom && !CoreGameManager.Instance.readyToStart) return;
        if (CoreGameManager.Instance.GetPlayer(0) != null && PlrPlayableCharacterVars.GetLocalPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thedweller"
            && __instance.icon != null && __instance.item.itemType != Items.None) {
            Sprite crisp = crisped.ContainsKey(__instance.item) ? crisped[__instance.item] : Sprite.Create(__instance.item.itemSpriteLarge.texture, new Rect(0f, 0f, __instance.item.itemSpriteLarge.texture.width, __instance.item.itemSpriteLarge.texture.height), Vector2.one / 2f, 80f, 0u, SpriteMeshType.FullRect);
            if (!crisped.ContainsKey(__instance.item))
            {
                crisp.name = "CrispedItemIconSpr_" + __instance.item.name;
                crisped.Add(__instance.item, crisp);
            }
            __instance.icon.spriteRenderer.sprite = crisp;
            //__instance.icon.spriteRenderer.material = PlayableCharsPlugin.assetMan.Get<Material>("DwellerMapMat");
        }
    }
}