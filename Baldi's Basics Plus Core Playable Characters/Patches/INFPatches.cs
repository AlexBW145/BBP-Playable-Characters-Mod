using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using UnityEngine.UI;
using UnityEngine;
using EndlessFloorsForever;
using MTM101BaldAPI.Registers;
using System.Linq;
using System;
using System.Collections.Generic;
using EndlessFloorsForever.Components;
#if !DEMO
namespace BBP_Playables.Core.Patches
{
    public class EndlessFloorsFuncs
    {
        internal static void ArcadeModeAdd()
        {
            EndlessForeverPlugin.AddGeneratorAction(PlayableCharsPlugin.Instance.Info, (data) =>
            {
                data.items.Add(new() { selection = PlayableCharsPlugin.assetMan.Get<ItemObject>("TinkerneerWrench"), weight = 75 });
                if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                    data.items.Add(new() { selection = PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"), weight = 11 });
            });
        }

        internal static bool Is99()
        {
            return EndlessForeverPlugin.Instance.InGameMode;
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.arcadeendlessforever"), HarmonyPatch(typeof(PlayerManager))]
    class CharacterINFPatches
    {
        [HarmonyPatch("Start"), HarmonyPrefix, HarmonyPriority(Priority.VeryHigh)]
        static void INFModeStarts(PlayerManager __instance)
        {
            if (!EndlessForeverPlugin.Instance.InGameMode) return;
            if (!PlayableCharsPlugin.gameStarted)
            {
                switch (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", ""))
                {
                    default:
                        PlayableCharsPlugin.Instance.Character.OnInitAction?.Invoke(__instance, true);
                        break;
                    /*case "thepartygoer":
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
                        break;*/
                    case "thetroublemaker":
                        __instance.itm.items[0] = ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value;
                        if (__instance.itm.maxItem > 0) __instance.itm.items[1] = ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value;
                        break;
                    case "thebackpacker" or "magicalstudent":
                        EndlessForeverPlugin.Instance.Counters["slots"] += 1;
                        if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "magicalstudent")
                            __instance.itm.items[0] = PlayableCharsPlugin.assetMan.Get<ItemObject>("MagicalWandTimesCharacter");
                        break;
                    case "thetinkerneer":
                        EndlessForeverPlugin.Instance.Counters["slots"] += 2;
                        break;
                }
            }
            if (EndlessForeverPlugin.Instance.Counters["slots"] > PlayableCharsPlugin.Instance.Character.maxSlots)
                EndlessForeverPlugin.Instance.Counters["slots"] = (byte)PlayableCharsPlugin.Instance.Character.maxSlots;
            __instance.itm.maxItem = EndlessForeverPlugin.Instance.Counters["slots"] - 1;
            PlayableCharsGame.prevSlots = EndlessForeverPlugin.Instance.Counters["slots"];
            CoreGameManager.Instance.GetHud(__instance.playerNumber).UpdateInventorySize(EndlessForeverPlugin.Instance.Counters["slots"]);
        }

        [HarmonyPatch(typeof(StandardUpgrade), nameof(StandardUpgrade.ShouldAppear)), HarmonyPostfix]
        static void MaxedOutMan(int currentLevel, StandardUpgrade __instance, ref bool __result)
        {
            if (__instance.id == "slots")
                __result = currentLevel < PlayableCharsPlugin.Instance.Character.maxSlots;
        }
    }

    [ConditionalPatchMod("alexbw145.baldiplus.arcadeendlessforever"), HarmonyPatch(typeof(HudManager), nameof(HudManager.UpdateInventorySize))]
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