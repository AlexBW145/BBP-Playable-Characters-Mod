using BBP_Playables.Core;
using HarmonyLib;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BBP_Playables.Extra.Foxo
{
    public class WrathFoxo : Baldi
    {
        internal InsanityComponent localPlayer;
        private Color[] insanityFades = [Color.white, new Color(1f, 1f, 1f, 0.75f), new Color(1f,1f,1f,0.5f), new Color(1f,1f,1f,0.25f), Color.clear];
        public new AudioManager AudMan;
        [SerializeField] internal SoundObject slap;

        private float timeToNextAnger;
        [SerializeField]
        private float baldiAngerRate = 1f;
        [SerializeField]
        private float baldiAngerAmount = 0.1f;
        [SerializeField] internal CustomSpriteRendererAnimator animator;
        public override void Initialize()
        {
            Navigator.Initialize(ec); // ResetSprite() fucked up custom teachers again.
            Navigator.Entity.SetHeight(6.5f);
            Navigator.Entity.SetResistAddend(true);
            animator.SetDefaultAnimation("idle", 1f);
            behaviorStateMachine.ChangeState(new HalluFoxo_Chase(this, this));
            behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
            localPlayer = ec.Players[0].GetComponent<InsanityComponent>();
            AudMan = gameObject.GetComponent<AudioManager>();
            AudMan.ReflectionSetVariable("disableSubtitles", true);
            Navigator.passableObstacles.AddRange([PassableObstacle.Bully, PassableObstacle.LockedDoor]);
        }
        [SerializeField] private InsanityModifier modifier = new InsanityModifier(-100f);
        protected override void VirtualUpdate()
        {
            spriteRenderer[0].color = localPlayer.IsAfraid ? insanityFades[0] : localPlayer.InsanityEffects[5] ? insanityFades[1] : localPlayer.InsanityEffects[4] ? insanityFades[2] : localPlayer.InsanityEffects[1] ? insanityFades[3] : insanityFades[4];
            AudMan.volumeModifier = localPlayer.IsAfraid ? 1f : localPlayer.InsanityEffects[5] ? 0.6f : localPlayer.InsanityEffects[4] ? 0.3f : localPlayer.InsanityEffects[1] ? 0.1f : 0f;
            foreach (var fox in FindObjectsOfType<InsanityComponent>(false))
                if (fox.IsAfraid && (transform.position - fox.transform.position).magnitude < 90f && !fox.modifiers.Contains(modifier))
                    fox.modifiers.Add(modifier);
                else if (fox.modifiers.Contains(modifier))
                    fox.modifiers.Remove(modifier);
            if (!localPlayer.IsAfraid) return;
            timeToNextAnger -= Time.deltaTime * ec.NpcTimeScale;
            if (timeToNextAnger <= 0f)
            {
                timeToNextAnger = baldiAngerRate + timeToNextAnger;
                GetAngry(baldiAngerAmount);
            }
        }
        public override void Despawn()
        {
            foreach (var fox in FindObjectsOfType<InsanityComponent>(false))
                fox.modifiers.Remove(modifier);
            base.Despawn();
        }
        /*protected override void VirtualOnTriggerEnter(Collider other) => VirtualOnTriggerStay(other);
        protected override void VirtualOnTriggerExit(Collider other) => VirtualOnTriggerStay(other);
        protected override void VirtualOnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player")) return;
            var entity = other.GetComponent<Entity>();
            if (entity != null && !entity.Equals(typeof(PlayerEntity)))
                if (!navigator.Entity.IsIgnoring(entity)) navigator.Entity.IgnoreEntity(entity, true);
        }*/

        public override void CaughtPlayer(PlayerManager player)
        {
            behaviorStateMachine.ChangeState(new Baldi_Attack(this, this));
            AudMan.FlushQueue(true);
            Time.timeScale = 0f;
            CoreGameManager.Instance.disablePause = true;
            CoreGameManager.Instance.GetCamera(0).UpdateTargets(transform, 0);
            CoreGameManager.Instance.GetCamera(0).offestPos = (player.transform.position - transform.position).normalized * 2f + Vector3.up;
            CoreGameManager.Instance.GetCamera(0).SetControllable(value: false);
            CoreGameManager.Instance.GetCamera(0).matchTargetRotation = false;
            CoreGameManager.Instance.audMan.volumeModifier = 0.6f;
            AudioManager audioManager = CoreGameManager.Instance.audMan;
            audioManager.PlaySingle(WeightedSelection<SoundObject>.RandomSelection(loseSounds));
            IEnumerator EndSequence()
            {
                var basegameman = BaseGameManager.Instance;
                Color prevColor = Shader.GetGlobalColor("_SkyboxColor");
                Tuple<float, float> prevcamPlane = new Tuple<float, float>(CoreGameManager.Instance.GetCamera(0).camCom.farClipPlane, CoreGameManager.Instance.GetCamera(0).billboardCam.farClipPlane);
                float time2 = 1f;
                Shader.SetGlobalColor("_SkyboxColor", Color.black);
                while (time2 > 0f)
                {
                    time2 -= Time.unscaledDeltaTime;
                    CoreGameManager.Instance.GetCamera(0).camCom.farClipPlane = 500f * time2;
                    CoreGameManager.Instance.GetCamera(0).billboardCam.farClipPlane = 500f * time2;
                    yield return null;
                }

                CoreGameManager.Instance.GetCamera(0).camCom.farClipPlane = 1000f;
                CoreGameManager.Instance.GetCamera(0).billboardCam.farClipPlane = 1000f;
                CoreGameManager.Instance.GetCamera(0).StopRendering(true);
                CoreGameManager.Instance.audMan.FlushQueue(true);
                AudioListener.volume = 0f;
                time2 = 2f;
                while (time2 > 0f)
                {
                    time2 -= Time.unscaledDeltaTime;
                    yield return null;
                }
                time2 = 120f;
                player.plm.Entity.SetFrozen(true);
                Time.timeScale = 1f;
                var timescale = new TimeScaleModifier(55f, 55f, 55f);
                ec.AddTimeScale(timescale);
                while (time2 > 0f)
                {
                    time2 -= Time.deltaTime * ec.EnvironmentTimeScale;
                    yield return null;
                }
                ec.RemoveTimeScale(timescale);
                CoreGameManager.Instance.disablePause = false;
                AudioListener.volume = 1f;
                CoreGameManager.Instance.audMan.volumeModifier = 1f;
                if (basegameman == null) yield break;
                CoreGameManager.Instance.GetCamera(0).camCom.farClipPlane = prevcamPlane.Item1;
                CoreGameManager.Instance.GetCamera(0).billboardCam.farClipPlane = prevcamPlane.Item2;
                Shader.SetGlobalColor("_SkyboxColor", prevColor);
                CoreGameManager.Instance.GetCamera(0).UpdateTargets(null, 0);
                CoreGameManager.Instance.GetCamera(0).offestPos = Vector3.zero;
                CoreGameManager.Instance.GetCamera(0).SetControllable(true);
                CoreGameManager.Instance.GetCamera(0).matchTargetRotation = true;
                CoreGameManager.Instance.GetCamera(0).StopRendering(false);
                localPlayer.WeDoneWithThatShit();
                player.plm.Entity.SetFrozen(false);
                Despawn();
                yield break;
            }
            CoreGameManager.Instance.StartCoroutine(EndSequence());
            InputManager.Instance.Rumble(1f, 2f);
        }

        public new void Slap()
        {
            animator.SetDefaultAnimation("idle", 1f);
            animator.Play("slap", 1f);
            AudMan.PlaySingle(slap);
            SlapRumble();
        }
    }

    public class HalluFoxo_Chase : Baldi_Chase // ResetSprite() fucked up custom teachers again.
    {
        private WrathFoxo foxo;
        public HalluFoxo_Chase(NPC npc, Baldi baldi)
        : base(npc, baldi)
        {
            foxo = baldi as WrathFoxo;
        }

        public override void Enter()
        {
            if (!initialized)
                Initialize();
            else
                Resume();
            AccessTools.DeclaredField(typeof(Baldi_Chase), "delayTimer").SetValue(this, baldi.Delay);
            baldi.ResetSlapDistance();
        }

        public override void Hear(GameObject source, Vector3 position, int value)
        {
        }

        protected override void ActivateSlapAnimation()
        {
            foxo.Slap();
        }

        public override void PlayerInSight(PlayerManager player)
        {
            if (foxo.localPlayer.IsAfraid)
                base.PlayerInSight(player);
        }

        public override void DoorHit(StandardDoor door)
        {
            if (foxo.localPlayer.IsAfraid)
                door.Unlock();
        }

        public override void Update()
        {
            base.Update();
            if (foxo.localPlayer.IsAfraid)
            {
                if (!foxo.behaviorStateMachine.CurrentNavigationState.Equals(typeof(NavigationState_TargetPlayer))) foxo.behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPlayer(foxo, 99, foxo.localPlayer.transform.position));
                foxo.behaviorStateMachine.CurrentNavigationState.UpdatePosition(foxo.localPlayer.transform.position);
            }
        }

        public override void DestinationEmpty()
        {
            if (!foxo.localPlayer.IsAfraid && !foxo.behaviorStateMachine.CurrentNavigationState.Equals(typeof(NavigationState_WanderRandom)))
            {
                foxo.SetAnger(foxo.baseAnger);
                foxo.behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(foxo, 64));
            }
        }

        public override void OnStateTriggerStay(Collider other, bool isValid)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlrPlayableCharacterVars>()?.GetCurrentPlayable() != FoxoPlayablePlugin.FoxoPlayable)
                return;

            baldi.looker.Raycast(other.transform, Vector3.Magnitude(baldi.transform.position - other.transform.position), out var targetSighted);
            if (!targetSighted)
                return;

            PlayerManager component = other.GetComponent<PlayerManager>();
            var insane = other.GetComponent<InsanityComponent>();
            if (insane != null && !component.invincible && insane.IsAfraid)
                baldi.CaughtPlayer(component);
        }
    }
}
