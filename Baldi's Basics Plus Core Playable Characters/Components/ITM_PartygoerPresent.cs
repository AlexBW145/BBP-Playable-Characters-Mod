#if DEBUG
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class ITM_PartygoerPresent : Item
    {
        [SerializeField] private bool Unwrapped;
        [SerializeField] private Character Gift = Character.Null;
        public Character Character => Gift;
        public static Dictionary<BepInEx.PluginInfo, Func<ItemObject, bool>> modAction = new Dictionary<BepInEx.PluginInfo, Func<ItemObject, bool>>();
        public static Dictionary<Character, SoundObject> RewardedSound = new Dictionary<Character, SoundObject>();
        public static HashSet<Type> disallowedStates = new HashSet<Type>()
        {
            typeof(DrReflex_Angry),
            typeof(DrReflex_Hunting),
            typeof(NoLateTeacher_TimeOut),
            typeof(NoLateTeacher_AttackDelay),
            typeof(NoLateTeacher_Triggered),
            typeof(NoLateTeacher_Returning),
            typeof(NoLateTeacher_AngryTeach),
            typeof(NoLateTeacher_Inform),
            typeof(Principal_WhistleApproach),
            typeof(Baldi_Apple),
            typeof(Baldi_Attack),
            typeof(Baldi_Chase_Tutorial),
            typeof(ArtsAndCrafters_Teleporting),
            typeof(LookAtGuy_Blinding),
            typeof(Beans_Watch),
            typeof(Playtime_Playing)
        };

        public override bool Use(PlayerManager pm)
        {
            if (Unwrapped && Physics.Raycast(pm.transform.position, CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, LayerMask.GetMask("Default")))
            {
                var item = hit.transform.GetComponent<Pickup>();
                if (item != null && item.transform.GetComponentInParent<StorageLocker>() == null
                    && item.price == 0 && item.free)
                {
                    if (new Items[] { Items.Apple, Items.Wd40, 
                        Items.PrincipalWhistle, Items.DetentionKey, 
                        Items.Scissors, 
                        Items.AlarmClock, Items.Teleporter, 
                        Items.Boots, 
                        Items.Nametag, 
                        Items.ZestyBar, 
                        Items.Bsoda, Items.DietBsoda, 
                        Items.GrapplingHook, Items.Tape }.Contains(item.item.itemType))
                        SetPickup(item); // I'm not inserting that function to all of them.
                    switch (item.item.itemType)
                    {
                        default:
                            foreach (var action in modAction)
                                if (action.Value.Invoke(item.item))
                                {
                                    SetPickup(item);
                                    return false;
                                }
                            break;
                        case Items.Apple or Items.Wd40: // Baldi
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Baldi.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.PrincipalWhistle or Items.DetentionKey: // Principal of the Thing
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Principal.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.Scissors: // Playtime
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Playtime.ToString()), pm.itm.selectedItem);
                            return false;
                        /*case Items.AlarmClock or Items.Teleporter: // Arts and Crafters
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Crafters.ToString()), pm.itm.selectedItem);
                            return false;*/
                        case Items.Boots: // Gotta Sweep
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Sweep.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.Nametag: // Mrs. Pomp
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Pomp.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.ZestyBar: // It's a Bully
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Bully.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.Bsoda or Items.DietBsoda: // Beans
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Beans.ToString()), pm.itm.selectedItem);
                            return false;
                        case Items.GrapplingHook or Items.Tape: // Dr. Reflex
                            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.DrReflex.ToString()), pm.itm.selectedItem);
                            return false;
                            /*case Items.BusPass: // Johnny
                                pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_Johnny"), pm.itm.selectedItem);
                                return false;
                            case Items.Quarter:
                                pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentGift_" + Character.Null.ToString()), pm.itm.selectedItem);
                                return false;*/
                    }
                }
            }
            else if (Physics.Raycast(pm.transform.position, CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, LayerMask.GetMask("NPCs")))
            {
                var npc = hit.transform.GetComponent<NPC>();
                if (npc != null && npc.Character == Gift && npc.behaviorStateMachine.currentState is not NPC_Present && npc.behaviorStateMachine.currentState is not NPC_PresentAftermath
                    && !disallowedStates.Contains(npc.behaviorStateMachine.currentState.GetType()))
                {
                    if (RewardedSound.TryGetValue(npc.Character, out var soundObject))
                        npc.GetComponent<AudioManager>()?.PlaySingle(soundObject);
                    npc.behaviorStateMachine.ChangeState(new NPC_Present(npc, npc.behaviorStateMachine.currentState, 15f));
                    return true;
                }
            }
            return false;
        }

        private void SetPickup(Pickup pickup)
        {
            if (pickup.item.itemType != 0)
            {
                pickup.ReflectionSetVariable("stillHasItem", false);
                pickup.gameObject.SetActive(value: false);
                if (pickup.icon != null) pickup.icon.spriteRenderer.enabled = false;
            }
        }
    }

    public class NPC_Present : NpcState // What's the point for Dealer's item to be open sourced...
    {
        private NpcState previousState;
        private float time;
        private MovementModifier stopMoving = new MovementModifier(Vector3.zero, 0f);
        private float introTime = 2.4f;
        internal static SoundObject[] sounds = new SoundObject[2];
        private AudioManager audman;
        public NPC_Present(NPC npc, NpcState previousState, float time) : base(npc)
        {
            this.previousState = previousState;
            this.time = time;
            audman = npc.gameObject.GetComponent<AudioManager>();
            if (npc.GetComponent<Animator>() != null) npc.GetComponent<Animator>().enabled = false;
            if (ITM_PartygoerPresent.RewardedSound.TryGetValue(npc.Character, out var soundObject))
                introTime += soundObject.subDuration;
        }

        public override void Enter()
        {
            base.Enter();
            //npc.Navigator.Entity.SetInteractionState(false);
            npc.Navigator.Entity.ExternalActivity.moveMods.Add(stopMoving);
            npc.StopAllCoroutines();
        }

        public override void Resume()
        {
            //npc.Navigator.Entity.SetInteractionState(false);
            if (!npc.Navigator.Entity.ExternalActivity.moveMods.Contains(stopMoving))
                npc.Navigator.Entity.ExternalActivity.moveMods.Add(stopMoving);
        }

        public override void Exit()
        {
            npc.Navigator.Entity.ExternalActivity.moveMods.Remove(stopMoving);
        }

        public override void Update()
        {
            time -= Time.deltaTime * npc.TimeScale;
            if (introTime <= 0)
            {
                audman?.PlayRandomAudio(sounds);
                introTime = UnityEngine.Random.Range(0.04f, 0.07f);
            }
            else
                introTime -= Time.deltaTime * npc.TimeScale;
            if (time <= 0f)
                npc.behaviorStateMachine.ChangeState(new NPC_PresentAftermath(npc, previousState, stopMoving));
        }
    }

    public class NPC_PresentAftermath : NpcState
    {
        private NpcState previousState;
        public NpcState PreviousState => previousState;
        private AudioManager audman;
        public static Dictionary<Character, Func<NPC, float>> actions = new Dictionary<Character, Func<NPC, float>>();
        private float time = 5f;
        private MovementModifier stopMoving;
        public NPC_PresentAftermath(NPC npc, NpcState previousState, MovementModifier stopMove) : base(npc) {
            this.previousState = previousState;
            audman = npc.gameObject.GetComponent<AudioManager>();
            stopMoving = stopMove;
        }

        public override void Enter()
        {
            base.Enter();
            npc.Navigator.Entity.ExternalActivity.moveMods.Add(stopMoving);
            actions.TryGetValue(npc.Character, out var action);
            if (action != null)
                time = action.Invoke(npc);
        }

        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime * npc.TimeScale;
            if (time <= 0f)
                npc.behaviorStateMachine.ChangeState(previousState);
        }

        public override void Exit()
        {
            //npc.Navigator.Entity.SetInteractionState(true);
            npc.Navigator.Entity.ExternalActivity.moveMods.Remove(stopMoving);
            if (npc.GetComponent<Animator>() != null) npc.GetComponent<Animator>().enabled = true;
        }
    }
}
#endif