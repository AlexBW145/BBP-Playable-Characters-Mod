using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BBP_Playables.Core.Patches
{
    /*[HarmonyPatch(typeof(ItemManager), "UseItem"), HarmonyPriority(Priority.VeryHigh)]
    class NeverRemoveDat
    {
        static bool Prefix(ItemManager __instance, ref bool ___disabled)
        {
            // Infinite Uses!!
            if (!___disabled && ItemMetaStorage.Instance.Get(__instance.items[__instance.selectedItem]).tags.Contains("CharacterItemImportant"))
            {
                GameObject.Instantiate(__instance.items[__instance.selectedItem].item).Use(__instance.pm);
                return false;
            }
            return true;
        }
    }*/

    [HarmonyPatch(typeof(BaseGameManager))]
    class ManagerPatches
    {

        [HarmonyPatch("Initialize"), HarmonyPostfix]
        static void ResetStuff(BaseGameManager __instance)
        {
            if (CoreGameManager.Instance.musicMan.QueuedAudioIsPlaying)
                CoreGameManager.Instance.musicMan.FlushQueue(true);
        }

        [HarmonyPatch("BeginPlay"), HarmonyPostfix]
        static void BeginPostfix(BaseGameManager __instance)
        {
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                CoreGameManager.Instance.GetPlayer(i).GetComponent<PlayableCharacterComponent>()?.GameBegin(__instance);
        }

        [HarmonyPatch("BeginSpoopMode"), HarmonyPostfix]
        static void SpoopModePostfix(BaseGameManager __instance)
        {
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                CoreGameManager.Instance.GetPlayer(i).GetComponent<PlayableCharacterComponent>()?.SpoopBegin(__instance);
        }
        static FieldInfo timeScaleModifiers = AccessTools.DeclaredField(typeof(EnvironmentController), "timeScaleModifiers");
        [HarmonyPatch("ActivityCompleted"), HarmonyPostfix]
        static void TestSubjectTimeScaleIncrease(bool correct, Activity activity, BaseGameManager __instance)
        {
            var tsm = (List<TimeScaleModifier>)timeScaleModifiers.GetValue(__instance.Ec);
            if (!correct)
                for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                    if (CoreGameManager.Instance.GetPlayer(i).GetComponent<TestSubjectMan>() != null && tsm.Contains(CoreGameManager.Instance.GetPlayer(i).GetComponent<TestSubjectMan>().subjectScale))
                    {
                        CoreGameManager.Instance.GetPlayer(i).GetComponent<TestSubjectMan>().MessUpAndIncreaseTimeScale();
                        break;
                    }
        }
    }

    [HarmonyPatch(typeof(Activity))]
    class ThinkerPatches
    {
        private static FieldInfo 
            answerText = AccessTools.DeclaredField(typeof(MathMachine), "answerText"),
            countedText = AccessTools.DeclaredField(typeof(BalloonBuster), "countTmp"),
            ___audioManager = AccessTools.DeclaredField(typeof(MatchActivityBalloon), "audioManager"),
            ___spriteRenderer = AccessTools.DeclaredField(typeof(MatchActivityBalloon), "spriteRenderer");

        [HarmonyPatch(nameof(Activity.Completed), [typeof(int), typeof(bool)]), HarmonyPostfix]
        static void ThinkerDidNotThink(int player, bool correct, Activity __instance)
        {
            if (!correct && PlrPlayableCharacterVars.GetPlayable(player)?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thethinker"
                && CoreGameManager.Instance.GetPoints(player) > 0)
                CoreGameManager.Instance.AddPoints(CoreGameManager.Instance.GetPoints(player) < 50 ? -CoreGameManager.Instance.GetPoints(player) : -50, player, true);
        }

        [HarmonyPatch(nameof(Activity.SetBonusMode))]
        [HarmonyPatch("ResetActivity")]
        [HarmonyPostfix]
        static void ResetTheColor(Activity __instance)
        {
            TMP_Text text = null;
            if (__instance.GetComponent<MathMachine>() != null)
                text = answerText.GetValue(__instance.GetComponent<MathMachine>()) as TMP_Text;
            else if (__instance.GetComponent<BalloonBuster>() != null)
                text = countedText.GetValue(__instance.GetComponent<BalloonBuster>()) as TMP_Text;
            if (text != null && text.color == Color.green)
                text.color = Color.white;
        }

        [HarmonyPatch(typeof(MatchActivityBalloon), nameof(MatchActivityBalloon.RevealConfirm)), HarmonyPostfix]
        static void IRemember(MatchActivityBalloon __instance, ref MatchActivity ___activity)
        {
            var me = PlrPlayableCharacterVars.GetLocalPlayable();
            if (me == null) return;
            if (__instance.MatchingBalloon.LastChance 
                && me.GetPlayer().plm.Entity.CurrentRoom == ___activity.room && me.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thethinker")
            {
                var thatBloon = __instance.MatchingBalloon;
                var sprRend = (SpriteRenderer)___spriteRenderer.GetValue(thatBloon);
                if (sprRend.color != Color.green)
                {
                    sprRend.color = Color.green;
                    ((AudioManager)___audioManager.GetValue(thatBloon)).PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "CashBell"));
                }
            }
        }
        [HarmonyPatch(typeof(MatchActivityBalloon), nameof(MatchActivityBalloon.Matched))]
        [HarmonyPatch(typeof(MatchActivityBalloon), nameof(MatchActivityBalloon.Unreveal))]
        [HarmonyPostfix]
        static void ResetTheColorBloon(ref SpriteRenderer ___spriteRenderer)
        {
            if (___spriteRenderer.color == Color.green)
                ___spriteRenderer.color = Color.white;
        }
    }

    [HarmonyPatch(typeof(LookAtGuy))]
    class TestSubjectTestBlindPatch
    {
        [HarmonyPatch(nameof(LookAtGuy.Blind))]
        static bool Prefix(LookAtGuy __instance, ref SpriteRenderer ___sprite, ref int ___ogLayer, ref AudioManager ___audMan, 
            ref SoundObject ___audExplode, ref QuickExplosion ___explosionPrefab, ref Transform ___billboardedTransform,
            ref AnimatedSpriteRotator ___spriteRotator, ref Sprite ___crumbledSprite, ref Transform ___headTransform)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thetestsubject")
            {
                __instance.FreezeNPCs(false);
                __instance.Navigator.maxSpeed = 0f;
                ___ogLayer = __instance.gameObject.layer;
                __instance.gameObject.layer = 20;
                ___audMan.PlaySingle(___audExplode);
                __instance.SetTargetTimeScale(1f);
                __instance.Respawn();
                GameObject.Instantiate(___explosionPrefab, ___billboardedTransform).transform.localPosition += Vector3.forward * 0.015f;
                ___spriteRotator.enabled = false;
                ___sprite.sprite = ___crumbledSprite;
                ___headTransform.gameObject.SetActive(false);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(LookAtGuy.Respawn))]
        static void Postfix(ref AudioManager ___audMan)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thetestsubject")
                ___audMan.FlushQueue(true);
        }
    }

    [HarmonyPatch(typeof(MathMachine), nameof(MathMachine.Completed), [typeof(int)])]
    class TestSubjectExtra
    {
        static void Prefix(int player, ref bool ___givePoints, ref int ___normalPoints)
        {
            if (___givePoints && PlrPlayableCharacterVars.GetPlayable(player)?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetestsubject")
                CoreGameManager.Instance.AddPoints(___normalPoints, player, true);
        }
    }

    [HarmonyPatch]
    class BackpackerNPCPatches
    {
        [HarmonyPatch(typeof(Baldi_Chase), nameof(Baldi_Chase.OnStateTriggerStay)), HarmonyPrefix]
        static bool AppleFromBackpack(Collider other, ref Baldi ___baldi)
        {
            if (other.CompareTag("Player") && other.GetComponent<PlayerManager>() != null)
            {
                ___baldi.looker.Raycast(other.transform, Vector3.Magnitude(___baldi.transform.position - other.transform.position), out var targetSighted);
                if (targetSighted && !other.GetComponent<PlayerManager>().invincible)
                {
                    BackpackerBackpack component = other.GetComponent<BackpackerBackpack>();
                    if (component != null && component.items.ToList().Exists(x => x.itemType == Items.Apple))
                    {
                        List<ItemObject> listItem = component.items.ToList();
                        listItem[listItem.FindIndex(x => x.itemType == Items.Apple)] = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
                        component.items = [..listItem];
                        ___baldi.TakeApple();
                        return false;
                    }
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.RemoveItemSlot)), HarmonyPrefix]
        static void ShrinkingPatch(int val, ItemManager __instance)
        {
            var backpack = __instance.gameObject.GetComponent<BackpackerBackpack>();
            if (backpack == null) return;
            for (int i = val; i < __instance.maxItem; i++)
                backpack.items[i] = backpack.items[i + 1];

            backpack.items[__instance.maxItem] = __instance.nothing;
        }
    }

    [HarmonyPatch]
    class PartygoerPatches
    {
        [HarmonyPatch(typeof(Pickup), nameof(Pickup.Start)), HarmonyPostfix]
        static void YouSureHeExists(Pickup __instance)
        {
            if (__instance.item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentUnwrapped"))
            {
                for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                    if (PlrPlayableCharacterVars.GetPlayable(i)?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Partygoer".ToLower().Replace(" ", ""))
                        return;
                __instance.AssignItem(ItemMetaStorage.Instance.FindByEnum(Items.None).value);
                __instance.Hide(true);
            }
        }
        [HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.StartGenerate)), HarmonyPostfix]
        static void NewWayTho(LevelGenerator __instance)
        {
            __instance.ld.forcedItems.DoIf(x => x.item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentUnwrapped"), present =>
            {
                for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                    if (PlrPlayableCharacterVars.GetPlayable(i)?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Partygoer".ToLower().Replace(" ", ""))
                        return;
                __instance.ld.forcedItems.Remove(present);
            });
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetItemSelect)), HarmonyPostfix]
        static void WhoIsItFor(int value, string key, HudManager __instance, ref RawImage[] ___itemBackgrounds, ref TMP_Text ___itemTitle)
        {
            if (___itemBackgrounds[value] != null && ___itemTitle != null && CoreGameManager.Instance.GetPlayer(__instance.hudNum)?.itm.items[value]?.itemType.ToStringExtended() == "PartygoerPresent")
                ___itemTitle.text = string.Format(LocalizationManager.Instance.GetLocalizedText(key), LocalizationManager.Instance.GetLocalizedText(NPCMetaStorage.Instance.Get(CoreGameManager.Instance.GetPlayer(0).itm.items[value].item.GetComponent<ITM_PartygoerPresent>().Character).value.Poster.textData[0].textKey));
        }
        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.EndGame)), HarmonyPrefix]
        static bool DeathFakeout(Transform player, Baldi baldi, CoreGameManager __instance)
        {
            ItemManager itm = player.gameObject.GetComponent<ItemManager>();
            if (itm.Has(EnumExtensions.GetFromExtendedName<Items>("PartygoerPresent")))
            {
                ItemObject present = itm.items.ToList().Find(item => item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Baldi.ToString()));
                if (present != null && baldi.Character == Character.Baldi)
                {
                    Tuple<string, double, bool> prevMidi = new Tuple<string, double, bool>("", 0, false);
                    if (MusicManager.Instance.MidiPlaying)
                        prevMidi = new Tuple<string, double, bool>(MusicManager.Instance.MidiPlayer.MPTK_MidiName, MusicManager.Instance.MidiPlayer.MPTK_Position, MusicManager.Instance.MidiPlayer.MPTK_Loop);
                    Time.timeScale = 0f;
                    MusicManager.Instance.StopMidi();
                    __instance.disablePause = true;
                    __instance.GetCamera(0).UpdateTargets(baldi.transform, 0);
                    __instance.GetCamera(0).offestPos = (player.position - baldi.transform.position).normalized * 2f + Vector3.up;
                    __instance.GetCamera(0).SetControllable(false);
                    __instance.GetCamera(0).matchTargetRotation = false;
                    __instance.audMan.volumeModifier = 0.6f;
                    AudioManager audioManager = __instance.audMan;
                    WeightedSelection<SoundObject>[] loseSounds = baldi.loseSounds;
                    audioManager.PlaySingle(WeightedSelection<SoundObject>.RandomSelection(loseSounds));
                    IEnumerator FakeoutSequence()
                    {
                        Color prevColor = Shader.GetGlobalColor("_SkyboxColor");
                        float time2 = 0.5f;
                        Tuple<float, float> prevcamPlane = new Tuple<float, float>(__instance.GetCamera(0).camCom.farClipPlane, __instance.GetCamera(0).billboardCam.farClipPlane);
                        Shader.SetGlobalColor("_SkyboxColor", Color.black);
                        while (time2 > 0f)
                        {
                            time2 -= Time.unscaledDeltaTime;
                            __instance.GetCamera(0).camCom.farClipPlane = 500f * time2;
                            __instance.GetCamera(0).billboardCam.farClipPlane = 500f * time2;
                            yield return null;
                        }
                        __instance.GetCamera(0).camCom.farClipPlane = prevcamPlane.Item1;
                        __instance.GetCamera(0).billboardCam.farClipPlane = prevcamPlane.Item2;
                        Shader.SetGlobalColor("_SkyboxColor", prevColor);
                        itm.RemoveItem(itm.items.ToList().FindIndex(item => item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Baldi.ToString())));
                        baldi.AudMan.PlaySingle(ITM_PartygoerPresent.RewardedSound[Character.Baldi]);
                        baldi.behaviorStateMachine.ChangeState(new NPC_Present(baldi, new Baldi_Chase(baldi, baldi), 15f));
                        audioManager.FlushQueue(true);
                        Time.timeScale = 1f;
                        if (!string.IsNullOrWhiteSpace(prevMidi.Item1))
                        {
                            MusicManager.Instance.PlayMidi(prevMidi.Item1, prevMidi.Item3);
                            MusicManager.Instance.MidiPlayer.MPTK_Position = prevMidi.Item2;
                        }
                        __instance.disablePause = false;
                        __instance.GetCamera(0).UpdateTargets(null, 0);
                        __instance.GetCamera(0).offestPos = Vector3.zero;
                        __instance.GetCamera(0).SetControllable(true);
                        __instance.GetCamera(0).matchTargetRotation = true;
                        __instance.audMan.volumeModifier = 1f;
                        yield break;
                    }
                    __instance.StartCoroutine(FakeoutSequence());
                    InputManager.Instance.Rumble(1f, 1f);
                    return false;
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(Bully), nameof(Bully.StealItem)), HarmonyPrefix]
        static bool HeStoleGift(PlayerManager pm, Bully __instance, ref float ___minDelay, ref float ___maxDelay, ref AudioManager ___audMan)
        {
            ItemManager itm = pm.gameObject.GetComponent<ItemManager>();
            if (itm.Has(EnumExtensions.GetFromExtendedName<Items>("PartygoerPresent")))
            {
                ItemObject present = itm.items.ToList().Find(item => item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Bully.ToString()));
                if (present != null)
                {
                    itm.RemoveItem(itm.items.ToList().FindIndex(item => item == PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Bully.ToString())));
                    ___audMan.PlaySingle(ITM_PartygoerPresent.RewardedSound[Character.Bully]);
                    __instance.behaviorStateMachine.ChangeState(new NPC_Present(__instance, new Bully_Wait(__instance, __instance, UnityEngine.Random.Range(___minDelay, ___maxDelay)), 15f));
                    __instance.ClearGuilt();
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch]
    class TroublemakerPatches
    {
        [HarmonyPatch(typeof(Bully), nameof(Bully.StealItem)), HarmonyPrefix, HarmonyPriority(Priority.High)]
        static bool TheyFriendsTho(PlayerManager pm)
        {
            if (pm.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                return false;
            return true;
        }
        [HarmonyPatch(typeof(Bully_Active), nameof(Bully_Active.PlayerSighted)), HarmonyPrefix]
        static bool LazyPatch(PlayerManager player) => player.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") != "The Troublemaker".ToLower().Replace(" ", "");
        [HarmonyPatch(typeof(ITM_AlarmClock), nameof(ITM_AlarmClock.Use))]
        [HarmonyPatch(typeof(ITM_NanaPeel), nameof(ITM_NanaPeel.Use))]
        [HarmonyPostfix]
        static void IsThisAnAbsolutePrank(PlayerManager pm)
        {
            if (pm.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                pm.RuleBreak("Bullying", 5f, 0.3f);
        }
        [HarmonyPatch(typeof(TapePlayer), nameof(TapePlayer.InsertItem)), HarmonyPostfix]
        static void YeahNoYouAreInTrouble(PlayerManager player, EnvironmentController ec)
        {
            if (player.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                player.RuleBreak("Bullying", 5f, 0.3f);
        }

        [HarmonyPatch(typeof(Principal), nameof(Principal.SendToDetention)), HarmonyPostfix]
        static void OhNoHow(Principal __instance, ref PlayerManager ___targetedPlayer, ref int ___detentionLevel, ref SoundObject[] ___audTimes)
        {
            if (__instance.ec.offices.Count > 0 && ___targetedPlayer.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                ___detentionLevel = Mathf.Min(___detentionLevel + 1, ___audTimes.Length - 1);
        }

        [HarmonyPatch(typeof(ITM_BSODA), nameof(ITM_BSODA.Use)), HarmonyPrefix]
        static void BuffedBSODA(PlayerManager pm, ref float ___time)
        {
            if (pm.GetPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                ___time *= 2f;
        }
    }
    // Affects the culling manager, useless
    /*[HarmonyPatch(typeof(LevelBuilder), "LoadRoom")]
    class ObjectBecomesNullThrowable
    {
        static void Postfix(RoomAsset asset, IntVector2 position, IntVector2 roomPivot, Direction direction, bool lockTiles, LevelBuilder __instance, ref RoomController __result)
        {
            if (BasePlugin.Instance.Character.name.ToLower().Replace(" ", "") != "CYLN_LOON".ToLower().Replace(" ", "")
                || __result.category != RoomCategory.Class) return;
            Debug.Log(__result.objectObject.GetComponentsInChildren<RendererContainer>().Length);
            foreach (var _object in __result.objectObject.GetComponentsInChildren<RendererContainer>(true))
            {
                Debug.Log(_object.gameObject.name);
                if (!(__instance.controlledRNG.NextDouble() < (double)0.3f && _object.transform.childCount <= 0) || _object.gameObject.layer == LayerMask.NameToLayer("Billboard") || _object.gameObject.name.Contains("B"))
                    continue;
                Debug.Log("Adding throwable object!");
                _object.gameObject.AddComponent<ThrowableObject>();
            }
        }
    }*/
}
