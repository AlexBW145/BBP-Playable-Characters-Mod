using BBP_Playables.Core;
using BCarnellChars;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using MTM101BaldAPI.Reflection;

namespace BBP_Playables.Modded.Patches
{
    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel")]
    class BCPPUnlocks
    {
        static void Prefix(BaseGameManager __instance)
        {
            if (CoreGameManager.Instance.currentMode == Mode.Free || __instance.levelObject == null) return;
            if (__instance.levelObject.name == "Basement1" &&
                __instance.levelObject.finalLevel)
                PlayableCharsPlugin.UnlockCharacter(Plugin.info, "The Dweller");
            if (__instance.levelObject.finalLevel && (CoreGameManager.Instance.GetPlayer(0).itm.items.Contains(PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"))
                || CoreGameManager.Instance.currentLockerItems.Contains(PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"))))
                PlayableCharsPlugin.UnlockCharacter(Plugin.info, "The Main Protagonist");
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(MainMenu), "Start")]
    class DwellerAlreadyUnlockedCheck
    {
        static void Postfix()
        {
            if (DwellerAbility.crisped.Count > 0)
                foreach (var crisp in DwellerAbility.crisped)
                {
                    DwellerAbility.crisped.Remove(crisp);
                    GameObject.Destroy(crisp);
                }
            if (BCPPSave.Instance.basementCompleted)
                PlayableCharsPlugin.UnlockCharacter(Plugin.info, "The Dweller");
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.bcarnellchars"), HarmonyPatch(typeof(Pickup), "Start")]
    class DwellerAbility
    {
        internal static List<Sprite> crisped = new List<Sprite>();
        static void Postfix(Pickup __instance)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thedweller") {
                Sprite crisp = Sprite.Create(__instance.item.itemSpriteLarge.texture, new Rect(0f,0f, __instance.item.itemSpriteLarge.texture.width, __instance.item.itemSpriteLarge.texture.height), Vector2.one/2f, 80f, 0u, SpriteMeshType.FullRect);
                crisp.name = "CrispedItemIconSpr_" + __instance.item.name;
                crisped.Add(crisp);
                __instance.icon.spriteRenderer.sprite = crisp;
                __instance.icon.spriteRenderer.material = PlayableCharsPlugin.assetMan.Get<Material>("DwellerMapMat");
            }
        }
    }
}