using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine.UI;
using UnityEngine;
using EndlessFloorsForever;
using MTM101BaldAPI.Registers;
using System.Linq;
using System;
using System.Collections.Generic;
using EndlessFloorsForever.Components;
using MTM101BaldAPI.PlusExtensions;
using System.Collections;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.Components;
using System.Reflection.Emit;
using TMPro;
using System.Reflection;
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

    [ConditionalPatchMod("alexbw145.baldiplus.arcadeendlessforever"), HarmonyPatch]
    class UpgradePatches
    {
        static TimeScaleModifier benderTime = new TimeScaleModifier();
        [HarmonyPatch(typeof(TestSubjectMan), "SpoopBegin"), HarmonyPostfix]
        static void TimeBenderStart(ref PlayerManager ___pm)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("testsubject_timebender"))
                ___pm.ec.AddTimeScale(benderTime);
        }

        [HarmonyPatch(typeof(TestSubjectMan), "MessUpAndIncreaseTimeScale"), HarmonyPrefix]
        static bool PenaltyLess(TestSubjectMan __instance)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("testsubject_penaltyremoval"))
            {
                __instance.subjectScale.npcTimeScale += 0.025f;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TestSubjectMan), "Update"), HarmonyPostfix]
        static void TimeBenderMan(ref HashSet<GameObject> ___spottedNPC, ref PlayerManager ___pm)
        {
            var plm = ___pm.plm;
            if (EndlessForeverPlugin.Instance.HasUpgrade("testsubject_timebender"))
            {
                var npcsInCurrentRoom = ___pm.ec.Npcs.Count(npc => npc.Navigator?.Entity.CurrentRoom == plm.Entity.CurrentRoom && npc is not Student);
                var npcCountNoStudents = ___pm.ec.Npcs.Count(npc => npc is not Student);
                float a = ((float)npcsInCurrentRoom + (float)npcCountNoStudents) / (float)npcCountNoStudents - 1;
                float b = ((float)___spottedNPC.Count / (float)(___pm.ec.Npcs.Count + 1));
                benderTime.npcTimeScale = Mathf.Max(0.5f, Mathf.Min(Mathf.Abs(a - b), 1f));
                benderTime.environmentTimeScale = Mathf.Max(0.5f, Mathf.Min(Mathf.Abs(a - b), 1f));
            }
        }

        private static MethodInfo Teleport = AccessTools.DeclaredMethod(typeof(ITM_Teleporter), "Teleport");
        private static IEnumerator ChanceCaughtSequence(PlayerManager player, Baldi baldi)
        {
            float time = 0f;
            float time2 = 1.71f;
            float glitchRate = 2.5f;
            var telportman = ItemMetaStorage.Instance.FindByEnum(Items.Teleporter).value.item as ITM_Teleporter;
            while (time2 > 0f)
            {
                time += Time.unscaledDeltaTime * 2.5f;
                time2 -= Time.unscaledDeltaTime;
                if (!PlayerFileManager.Instance.reduceFlashing)
                {
                    glitchRate -= Time.unscaledDeltaTime;
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(time, 2.2f));

                    if (glitchRate <= 0f)
                        glitchRate = 0.55f - time * 0.1f;
                }
                else
                {
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", time * 2f);
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", time * 2f);
                }
                yield return null;
            }
            CoreGameManager.Instance.ResetShaders();
            CoreGameManager.Instance.audMan.FlushQueue(true);
            CoreGameManager.Instance.GetCamera(0).UpdateTargets(null, 0);
            CoreGameManager.Instance.GetCamera(0).offestPos = Vector3.zero;
            CoreGameManager.Instance.GetCamera(0).SetControllable(true);
            CoreGameManager.Instance.GetCamera(0).matchTargetRotation = true;
            CoreGameManager.Instance.audMan.volumeModifier = 1f;
            Time.timeScale = 1f;
            CoreGameManager.Instance.disablePause = false;
            for (int i = 0; i < 3; i++)
            {
                var teleporter = UnityEngine.Object.Instantiate(telportman, null, false);
                teleporter.Use(player);
                Teleport.Invoke(teleporter, []);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSecondsNPCTimescale(baldi, 0.5f);
            if (baldi.GetType().IsSubclassOf(typeof(Baldi)))
            {
                var types = baldi.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
                if (types.Exists(x => x.Name == "GetAngryState"))
                    baldi.behaviorStateMachine.ChangeState((NpcState)types.Find(x => x.Name == "GetAngryState").Invoke(baldi, []));
                else
                    baldi.behaviorStateMachine.ChangeState(new Baldi_Chase(baldi, baldi));
            }
            else if (baldi.behaviorStateMachine.currentState is Baldi_Attack)
                baldi.behaviorStateMachine.ChangeState(new Baldi_Chase(baldi, baldi));
        }

        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.EndGame)), HarmonyPrefix, HarmonyPriority(Priority.First)]
        static bool ChanceFakeDeath(Transform player, Baldi baldi, CoreGameManager __instance)
        {
            var component = player.GetComponent<CYLN_LOONComponent>();
            if (component != null && EndlessForeverPlugin.Instance.HasUpgrade("cylnloon_onechance") && !component.LastChance)
            {
                component.LastChance = true;
                Time.timeScale = 0f;
                __instance.disablePause = true;
                __instance.GetCamera(0).UpdateTargets(baldi.transform, 0);
                __instance.GetCamera(0).offestPos = (player.position - baldi.transform.position).normalized * 2f + Vector3.up;
                __instance.GetCamera(0).SetControllable(value: false);
                __instance.GetCamera(0).matchTargetRotation = false;
                __instance.audMan.volumeModifier = 0.6f;
                AudioManager audioManager = __instance.audMan;
                audioManager.PlayRandomAudio(PlayableCharsPlugin.assetMan.Get<SoundObject[]>("LoseLastChanceSnds"));
                __instance.StartCoroutine(ChanceCaughtSequence(player.GetComponent<PlayerManager>(), baldi));
                InputManager.Instance.Rumble(1f, 1.71f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CYLN_LOONComponent), "CylnTime"), HarmonyPostfix]
        static void UpgradedCools(ref float __result)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("cylnloon_thorwablesrespawn"))
                __result = UnityEngine.Random.Range(30f * EndlessForeverPlugin.Instance.GetUpgradeCount("cylnloon_thorwablesrespawn"), 60f) / EndlessForeverPlugin.Instance.GetUpgradeCount("cylnloon_thorwablesrespawn");
        }

        [HarmonyPatch(typeof(BackpackerBackpack), "Update"), HarmonyPostfix]
        static void HikerUpgrade(ref PlayerManager ___pm, ref ValueModifier ___modifier)
        {
            if (___pm != null && EndlessForeverPlugin.Instance.HasUpgrade("backpacker_penaltyremoval"))
            {
                ___modifier.multiplier += 0.2f * EndlessForeverPlugin.Instance.GetUpgradeCount("backpacker_penaltyremoval");
            }
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
                        if (!ModdedFileManager.Instance.saveData.saveAvailable)
                            EndlessForeverPlugin.Instance.Counters["slots"] += 1;
                        if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "magicalstudent")
                            __instance.itm.items[0] = PlayableCharsPlugin.assetMan.Get<ItemObject>("MagicalWandTimesCharacter");
                        break;
                    case "thetinkerneer":
                        if (!ModdedFileManager.Instance.saveData.saveAvailable)
                            EndlessForeverPlugin.Instance.Counters["slots"] += 2;
                        __instance.itm.LockSlot(0, true);
                        break;
                    case "thespeedrunner":
                        ((SpeedrunnerSpeedyUpgrade)EndlessForeverPlugin.Upgrades["speedrunner_speedy"]).SetValue();
                        break;
                }
            }
            if (EndlessForeverPlugin.Instance.Counters["slots"] > PlayableCharsPlugin.Instance.Character.maxSlots)
                EndlessForeverPlugin.Instance.Counters["slots"] = (byte)PlayableCharsPlugin.Instance.Character.maxSlots;
            __instance.itm.maxItem = EndlessForeverPlugin.Instance.Counters["slots"] - 1;
            PlayableCharsGame.prevSlots = EndlessForeverPlugin.Instance.Counters["slots"];
            CoreGameManager.Instance.GetHud(__instance.playerNumber).UpdateInventorySize(EndlessForeverPlugin.Instance.Counters["slots"]);
        }

        [HarmonyPatch(typeof(PlrPlayableCharacterVars), nameof(PlrPlayableCharacterVars.Init)), HarmonyPostfix]
        static void SpeedyAdd(PlayerManager pm, PlrPlayableCharacterVars __instance)
        {
            if (__instance.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thespeedrunner")
            {
                var speedystat = (SpeedrunnerSpeedyUpgrade)EndlessForeverPlugin.Upgrades["speedrunner_speedy"];
                pm.GetMovementStatModifier().AddModifier("walkSpeed", speedystat.speedyStat);
                pm.GetMovementStatModifier().AddModifier("runSpeed", speedystat.speedyStat);
            }
        }
        [HarmonyPatch("EndlessFloorsForever.ArcadeEndlessForeverSave, ArcadeEndlessForever", "SellUpgrade"), HarmonyPostfix] // Class is internal and I have no idea why I made it that way.
        static void SpeedyRemove(string id)
        {
            if (EndlessForeverPlugin.Upgrades["speedrunner_speedy"].id == id)
                ((SpeedrunnerSpeedyUpgrade)EndlessForeverPlugin.Upgrades["speedrunner_speedy"]).SetValue();
        }

        [HarmonyPatch(typeof(StandardUpgrade), nameof(StandardUpgrade.ShouldAppear)), HarmonyPostfix]
        static void MaxedOutMan(int currentLevel, StandardUpgrade __instance, ref bool __result)
        {
            if (__instance.id == "slots")
                __result = currentLevel < PlayableCharsPlugin.Instance.Character.maxSlots;
        }

        static ValueModifier speedStatModifier = new ValueModifier(2f);
        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.AddStamina)), HarmonyPostfix]
        static void StaminalessBoost(float value, bool limited, PlayerMovement __instance)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("playablechars_staminaless") && __instance.staminaMax <= 0)
                __instance.StartCoroutine(StaminalessBoosting(__instance));
        }
        [HarmonyPatch(typeof(ITM_ZestyBar), nameof(ITM_ZestyBar.Use)), HarmonyPostfix]
        static void ZestyBarWhy(PlayerManager pm)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("playablechars_staminaless") && pm.plm.staminaMax <= 0)
            {
                pm.plm.stamina = 100f;
                pm.StartCoroutine(StaminalessBoosting(pm.plm));
            }
        }

        static IEnumerator StaminalessBoosting(PlayerMovement __instance)
        {
            var statModifier = __instance.pm.GetMovementStatModifier();
            if (statModifier.modifiers["walkSpeed"].Contains(speedStatModifier)) yield break;
            statModifier.AddModifier("walkSpeed", speedStatModifier);
            while (__instance.stamina > 0)
            {
                if (__instance.Entity.InternalMovement.magnitude > 0f)
                    __instance.stamina -= 10f * Time.deltaTime * __instance.pm.PlayerTimeScale;
                yield return null;
            }
            __instance.stamina = 0;
            statModifier.RemoveModifier(speedStatModifier);
            yield break;
        }

        [HarmonyPatch(typeof(ThinkerAbility), nameof(ThinkerAbility.ThinkerDrain), MethodType.Enumerator), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ThinkerIsDrainSlow(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs(15f))
                    yield return Transpilers.EmitDelegate<Func<float>>(() => EndlessForeverPlugin.Instance.HasUpgrade("thinker_smartersave") ? 30f : 15f);
                else
                    yield return instruction;
            }
            yield break;
        }

        [HarmonyPatch(typeof(MathMachine), nameof(MathMachine.Completed), [typeof(int), typeof(bool)]), HarmonyPostfix]
        static void SmarterNoCheat(int player, bool correct, ref TMP_Text ___answerText, ref int ___normalPoints)
        {
            if (correct && ___answerText.color != Color.green
                && PlrPlayableCharacterVars.GetPlayable(player)?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thethinker" && EndlessForeverPlugin.Instance.HasUpgrade("thinker_smartersave"))
            {
                CoreGameManager.Instance.AddPoints(___normalPoints * 2, player, true);
            }
        }

        [HarmonyPatch(typeof(ThinkerAbility), "Update"), HarmonyPostfix]
        static void ThinkerLessTime(ref PlayerManager ___pm, ref bool ___mathMachineVisible, ref float ___timeLooking)
        {
            if (EndlessForeverPlugin.Instance.HasUpgrade("thinker_fasterthinking") && ___mathMachineVisible && ___pm.plm.Entity.CurrentRoom.category == RoomCategory.Class)
                ___timeLooking += 0.5f * (___pm.PlayerTimeScale * Time.deltaTime);
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