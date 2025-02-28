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
using UnityEngine.UIElements.UIR;

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
        private static TimeScaleModifier subjectScale = new TimeScaleModifier();

        [HarmonyPatch("Initialize"), HarmonyPostfix]
        static void ResetStuff(BaseGameManager __instance)
        {
            __instance.Ec.RemoveTimeScale(subjectScale);
            subjectScale = new TimeScaleModifier();
            if (CoreGameManager.Instance.musicMan.QueuedAudioIsPlaying)
                CoreGameManager.Instance.musicMan.FlushQueue(true);
        }

        [HarmonyPatch("BeginPlay"), HarmonyPostfix]
        static void BeginPostfix(BaseGameManager __instance)
        {
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
            {
                if (CoreGameManager.Instance.GetPlayer(i).GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable() == null) continue;
                switch (CoreGameManager.Instance.GetPlayer(i).GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", ""))
                {
                    case "thetestsubject":
                        __instance.Ec.AddTimeScale(subjectScale);
                        break;
                }
            }
        }

        [HarmonyPatch("BeginSpoopMode"), HarmonyPostfix]
        static void SpoopModePostfix(BaseGameManager __instance)
        {
            for (int i = 0; i < CoreGameManager.Instance.setPlayers; i++)
                switch (CoreGameManager.Instance.GetPlayer(i).GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", ""))
                {
                    case "thethinker":
                        CoreGameManager.Instance.GetPlayer(i).StartCoroutine(CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<ThinkerAbility>().ThinkerDrain());
                        break;
                    case "thetestsubject":
                        if (CoreGameManager.Instance.GetPlayer(i).gameObject.GetComponent<TestSubjectMan>() == null)
                            CoreGameManager.Instance.GetPlayer(i).gameObject.AddComponent<TestSubjectMan>();
                        break;
                }
        }
        static FieldInfo timeScaleModifiers = AccessTools.DeclaredField(typeof(EnvironmentController), "timeScaleModifiers");
        [HarmonyPatch("ActivityCompleted"), HarmonyPostfix]
        static void TestSubjectTimeScaleIncrease(bool correct, Activity activity, BaseGameManager __instance)
        {
            var tsm = (List<TimeScaleModifier>)timeScaleModifiers.GetValue(__instance.Ec);
            if (!correct && tsm.Contains(subjectScale))
                subjectScale.npcTimeScale += 0.2f;
        }
    }

    [HarmonyPatch(typeof(Activity), nameof(Activity.Completed), [typeof(int), typeof(bool), typeof(Activity)])]
    class ThinkerDidNotThink
    {
        static void Postfix(int player, bool correct, Activity activity, Activity __instance)
        {
            if (__instance.GetComponent<MathMachine>() != null && !correct && CoreGameManager.Instance.GetPlayer(player).GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thethinker"
                && CoreGameManager.Instance.GetPoints(player) > 0)
                CoreGameManager.Instance.AddPoints(CoreGameManager.Instance.GetPoints(player) < 50 ? -CoreGameManager.Instance.GetPoints(player) : -50, player, true);

        }
    }

    [HarmonyPatch(typeof(LookAtGuy))]
    class TestSubjectTestBlindPatch
    {
        [HarmonyPatch(nameof(LookAtGuy.Blind))]
        static bool Prefix(LookAtGuy __instance, ref SpriteRenderer ___sprite, ref int ___ogLayer, ref Animator ___animator, ref AudioManager ___audMan, ref SoundObject ___audBlindStart, ref SoundObject ___audBlindLoop, ref float ___fogTime)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thetestsubject")
            {
                __instance.FreezeNPCs(false);
                __instance.Navigator.maxSpeed = 0f;
                ___sprite.enabled = false;
                ___ogLayer = __instance.gameObject.layer;
                __instance.gameObject.layer = 20;
                ___animator.Play("Reset", -1, 0f);
                ___audMan.QueueAudio(___audBlindStart, true);
                ___audMan.QueueAudio(___audBlindLoop);
                ___audMan.SetLoop(true);
                __instance.behaviorStateMachine.ChangeState(new LookAtGuy_Blinding(__instance, ___fogTime));
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
        static void Prefix(int player, ref bool ___givePoints)
        {
            if (___givePoints && CoreGameManager.Instance.GetPlayer(player).GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetestsubject")
                CoreGameManager.Instance.AddPoints(30, player, true);
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.BeginSpoopMode))]
    class NullObjectThrowableSpawn
    {
        public static List<GameObject> prefabs = new List<GameObject>();
        static void Postfix(BaseGameManager __instance)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") != "CYLN_LOON".ToLower().Replace(" ", "")) return;
            __instance.StartCoroutine(CylnObjects());
        }

        static IEnumerator CylnObjects()
        {
            float time;
            while (BaseGameManager.Instance != null)
            {
                time = UnityEngine.Random.Range(60f, 90f);
                while (time > 0f)
                {
                    if (BaseGameManager.Instance == null)
                        yield break;
                    time -= BaseGameManager.Instance.Ec.EnvironmentTimeScale * Time.deltaTime;
                    yield return null;
                }
                if (BaseGameManager.Instance == null)
                    yield break;
                List<Cell> hallcells = new List<Cell>();
                for (int i = 0; i < BaseGameManager.Instance.Ec.levelSize.x; i++)
                {
                    for (int j = 0; j < BaseGameManager.Instance.Ec.levelSize.z; j++)
                    {
                        if (BaseGameManager.Instance.Ec.cells[i, j] != null && BaseGameManager.Instance.Ec.cells[i, j].room.category == RoomCategory.Hall 
                            && !BaseGameManager.Instance.Ec.cells[i, j].open && !BaseGameManager.Instance.Ec.cells[i, j].HasObjectBase
                            && !Physics.OverlapSphere(BaseGameManager.Instance.Ec.cells[i, j].CenterWorldPosition, 0.5f).ToList().Exists(x => x.gameObject.GetComponent<ThrowableObject>()))
                            hallcells.Add(BaseGameManager.Instance.Ec.cells[i, j]);
                    }
                }
                if (hallcells.Count > 0) {
                    GameObject table = GameObject.Instantiate(PlayableCharsPlugin.assetMan.Get<Entity>("CYLN_Throwable").gameObject, hallcells[UnityEngine.Random.Range(0, hallcells.Count)].CenterWorldPosition, default(Quaternion), null);
                    var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
                    table.GetComponentInChildren<MeshFilter>(true).sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                    table.GetComponentInChildren<MeshRenderer>(true).SetMaterialArray(prefab.GetComponent<MeshRenderer>().GetMaterialArray());
                }
                yield return null;
            }
            yield break;
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
#if DEBUG
    [HarmonyPatch]
    class PartygoerPatches
    {
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
                if (present != null)
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
#endif

    [HarmonyPatch]
    class TroublemakerPatches
    {
        [HarmonyPatch(typeof(Bully), nameof(Bully.StealItem)), HarmonyPrefix, HarmonyPriority(Priority.High)]
        static bool TheyFriendsTho(PlayerManager pm)
        {
            if (pm.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                return false;
            return true;
        }
        [HarmonyPatch(typeof(Bully_Active), nameof(Bully_Active.PlayerSighted)), HarmonyPrefix]
        static bool LazyPatch(PlayerManager player) => player.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") != "The Troublemaker".ToLower().Replace(" ", "");
        [HarmonyPatch(typeof(ITM_AlarmClock), nameof(ITM_AlarmClock.Use))]
        [HarmonyPatch(typeof(ITM_Tape), nameof(ITM_Tape.Use))]
        [HarmonyPatch(typeof(ITM_PrincipalWhistle), nameof(ITM_PrincipalWhistle.Use))]
        [HarmonyPostfix]
        static void IsThisAnAbsolutePrank(PlayerManager pm)
        {
            if (pm.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                pm.RuleBreak("Bullying", 5f, 0.3f);
        }
        [HarmonyPatch(typeof(TapePlayer), nameof(TapePlayer.InsertItem)), HarmonyPostfix]
        static void YeahNoYouAreInTrouble(PlayerManager player, EnvironmentController ec)
        {
            if (player.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                player.RuleBreak("Bullying", 5f, 0.3f);
        }

        [HarmonyPatch(typeof(Principal), nameof(Principal.SendToDetention)), HarmonyPostfix]
        static void OhNoHow(Principal __instance, ref PlayerManager ___targetedPlayer, ref int ___detentionLevel, ref SoundObject[] ___audTimes)
        {
            if (__instance.ec.offices.Count > 0 && ___targetedPlayer.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "The Troublemaker".ToLower().Replace(" ", ""))
                ___detentionLevel = Mathf.Min(___detentionLevel + 1, ___audTimes.Length - 1);
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
