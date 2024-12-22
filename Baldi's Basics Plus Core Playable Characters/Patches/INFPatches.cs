using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using UnityEngine.UI;
using UnityEngine;
using BaldiEndless;
using MTM101BaldAPI.Registers;
using System.Linq;
using System;
using System.Collections.Generic;
#if !DEMO
namespace BBP_Playables.Core.Patches
{
    public class EndlessFloorsFuncs
    {
        internal static void ArcadeModeAdd()
        {
            EndlessFloorsPlugin.AddGeneratorAction(PlayableCharsPlugin.Instance.Info, (data) =>
            {
                data.items.Add(new() { selection = PlayableCharsPlugin.assetMan.Get<ItemObject>("TinkerneerWrench"), weight = 75 });
                if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                    data.items.Add(new() { selection = PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"), weight = 11 });
            });
        }

        internal static bool Is99()
        {
            return CoreGameManager.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode;
        }
    }

    [ConditionalPatchMod("mtm101.rulerp.baldiplus.endlessfloors"), HarmonyPatch(typeof(PlayerManager))]
    class CharacterINFPatches
    {
        [HarmonyPatch("Start"), HarmonyPrefix, HarmonyPriority(Priority.VeryHigh)]
        static void INFModeStarts(PlayerManager __instance)
        {
            if (EndlessFloorsFuncs.Is99()) return;
            if (!PlayableCharsPlugin.gameStarted)
            {
                switch (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", ""))
                {
                    default:
                        break;
                    case "thepartygoer": // I'm not allowed to insert a random `EndlessFloorsPlugin.presentObject` onto an `Core.BasePlugin.Awake()`
                        WeightedItemObject[] array = ITM_Present.potentialObjects.ToArray();
                        int weightAverage = array.Sum((WeightedItemObject x) => x.weight) / array.Length;
                        Dictionary<WeightedItemObject, int> ogWeights = new Dictionary<WeightedItemObject, int>();
                        array.Do(delegate (WeightedItemObject obj) // Starting with the defualt luck, ever...
                        {
                            ogWeights.Add(obj, obj.weight);
                            if (obj.weight > weightAverage)
                                obj.weight = Mathf.FloorToInt(obj.weight / 1f);
                            else
                                obj.weight = Mathf.CeilToInt(obj.weight * 1f);
                        });
                        WeightedSelection<ItemObject>[] items = array;
                        __instance.itm.items[0] = WeightedSelection<ItemObject>.RandomSelection(items);
                        break;
                    case "thetroublemaker":
                        __instance.itm.items[0] = ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value;
                        if (__instance.itm.maxItem > 0) __instance.itm.items[1] = ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value;
                        break;
                    case "thebackpacker" or "magicalstudent":
                        EndlessFloorsPlugin.currentSave.Counters["slots"] += 1;
                        if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "magicalstudent")
                            __instance.itm.items[0] = PlayableCharsPlugin.assetMan.Get<ItemObject>("MagicalWandTimesCharacter");
                        break;
                    case "thetinkerneer":
                        EndlessFloorsPlugin.currentSave.Counters["slots"] += 2;
                        break;
                }
            }
            __instance.itm.maxItem = EndlessFloorsPlugin.currentSave.itemSlots - 1;
            PlayableCharsGame.prevSlots = EndlessFloorsPlugin.currentSave.Counters["slots"];
            CoreGameManager.Instance.GetHud(__instance.playerNumber).UpdateInventorySize(EndlessFloorsPlugin.currentSave.Counters["slots"]);
        }
    }

    [ConditionalPatchMod("mtm101.rulerp.baldiplus.endlessfloors"), HarmonyPatch("BaldiEndless.PreventSelectPatch, BaldiEndless", "Prefix")] // Couldn't get the assembly from an existing INF class but hey, that'd gonna work...
    class ShutUpPatch
    {
        static bool Prefix()
        {
            if (CoreGameManager.Instance == null || CoreGameManager.Instance.GetPlayer(0) == null) return true;
            CoreGameManager.Instance.GetHud(0).SetItemSelect(CoreGameManager.Instance.GetPlayer(0).itm.selectedItem, CoreGameManager.Instance.GetPlayer(0).itm.items[CoreGameManager.Instance.GetPlayer(0).itm.selectedItem].nameKey);
            return false;
        }
    }

    [ConditionalPatchMod("mtm101.rulerp.baldiplus.endlessfloors"), HarmonyPatch(typeof(HudManager), nameof(HudManager.UpdateInventorySize))]
    class BackpackerBackpackBugfix // Happens on PIT stop item slot upgrades
    {
        static void Postfix()
        {
            if (CoreGameManager.Instance.GetPlayer(0) == null || Time.timeScale == 0f) return;
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thebackpacker" && CoreGameManager.Instance.GetPlayer(0).itm.items[CoreGameManager.Instance.GetPlayer(0).itm.maxItem].itemType != PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackClosed").itemType) { // Backpacker got some issues while having duplicate backpacks.
                CoreGameManager.Instance.GetPlayer(0).itm.LockSlot(CoreGameManager.Instance.GetPlayer(0).itm.items.ToList().FindIndex(x => x.itemType == PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackClosed").itemType), false);
                CoreGameManager.Instance.GetPlayer(0).itm.SetItem(CoreGameManager.Instance.GetPlayer(0).itm.nothing, CoreGameManager.Instance.GetPlayer(0).itm.items.ToList().FindIndex(x => x.itemType == PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackClosed").itemType));
                CoreGameManager.Instance.GetPlayer(0).itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackClosed"), CoreGameManager.Instance.GetPlayer(0).itm.maxItem);
                CoreGameManager.Instance.GetPlayer(0).itm.LockSlot(CoreGameManager.Instance.GetPlayer(0).itm.maxItem, true);
                CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>().items[CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>().items.ToList().FindIndex(x => x.itemType == PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackOpen").itemType)] = CoreGameManager.Instance.GetPlayer(0).itm.nothing;
                CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>().items[CoreGameManager.Instance.GetPlayer(0).itm.maxItem] = PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackOpen");
            }
        }
    }
}
#endif