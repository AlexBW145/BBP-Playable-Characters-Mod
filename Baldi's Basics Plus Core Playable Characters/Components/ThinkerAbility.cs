using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
#if !DEMO
namespace BBP_Playables.Core
{
    // Looker code wtf
    [RequireComponent(typeof(PlayerManager), typeof(PlayerClick), typeof(PlayerEntity))]
    public class ThinkerAbility : PlayableCharacterComponent
    {
        private Fog ThinkerBlind = new Fog()
        {
            color = Color.black,
            startDist = 5,
            maxDist = 50,
            strength = 0.5f,
            priority = 16,
        };
        private Camera cam;
        private Ray ray;
        private RaycastHit[] hits = new RaycastHit[32];
        private int hitCount;
        public List<Transform> _hitTransforms = new List<Transform>();
        private bool mathMachineVisible = false;
        private float timeLooking = 0f;
        private AudioManager musicMan;

        protected override void Start()
        {
            base.Start();
            musicMan = Instantiate(CoreGameManager.Instance.musicMan, transform, false);
            cam = CoreGameManager.Instance.GetCamera(pm.playerNumber).camCom;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (CoreGameManager.Instance.GetPoints(pm.playerNumber) < 50)
                CoreGameManager.Instance.AddPoints(Mathf.Abs(CoreGameManager.Instance.GetPoints(pm.playerNumber)) + Mathf.Min(50, 50 - Mathf.Abs(CoreGameManager.Instance.GetPoints(pm.playerNumber))), 0, false, true);
        }
        public override void SpoopBegin(BaseGameManager manager) => StartCoroutine(ThinkerDrain());

        private static FieldInfo
            ___answerText = AccessTools.DeclaredField(typeof(MathMachine), "answerText"),
            ___answer = AccessTools.DeclaredField(typeof(MathMachine), "answer"),
            ___countTmp = AccessTools.DeclaredField(typeof(BalloonBuster), "countTmp"),
            ___solution = AccessTools.DeclaredField(typeof(BalloonBuster), "solution"),
            ___startingTotal = AccessTools.DeclaredField(typeof(BalloonBuster), "startingTotal");

        void Update()
        {
            if (pm.ec.Active && pm.plm.Entity.CurrentRoom != null)
            {
                mathMachineVisible = false;
                if (CoreGameManager.Instance.GetPoints(0) <= 0) return;
                ray.origin = transform.position;
                ray.direction = cam.transform.forward;
                hitCount = Physics.RaycastNonAlloc(ray, hits, Mathf.Min(pm.pc.reach*5, pm.ec.MaxRaycast), LayerMask.GetMask("Default", "Block Raycast", "Windows"), QueryTriggerInteraction.Collide);
                _hitTransforms.Clear();
                for (int i = 0; i < hitCount; i++)
                {
                    _hitTransforms.Add(hits[i].transform);
                    if (hits[i].collider.gameObject.GetComponentInParent<Activity>() != null)
                        mathMachineVisible = true;
                }

                if (mathMachineVisible && pm.plm.Entity.CurrentRoom.category == RoomCategory.Class)
                {
                    timeLooking += pm.PlayerTimeScale * Time.deltaTime;
                    if (timeLooking > 1f && _hitTransforms.Exists(x => x.gameObject.GetComponentInParent<Activity>() != null))
                    {
                        Activity machine = _hitTransforms.Find(x => x.gameObject.GetComponentInParent<Activity>() != null).gameObject.GetComponentInParent<Activity>();
                        if (machine is MathMachine)
                        {
                            MathMachine mathMachine = machine as MathMachine;
                            TMP_Text answer = (TMP_Text)___answerText.GetValue(mathMachine);
                            if (answer.text == "?")
                            {
                                int correct = (int)___answer.GetValue(mathMachine);
                                answer.text = correct.ToString();
                                answer.color = Color.green;
                                // 10+ answers from Times are fucky...
                                answer.autoSizeTextContainer = false;
                                answer.autoSizeTextContainer = true;
                                mathMachine.gameObject.GetComponent<AudioManager>().PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "CashBell"));
                            }
                            else
                                timeLooking = 0f;
                        }
                        else if (machine is BalloonBuster)
                        {
                            BalloonBuster busterMachine = machine as BalloonBuster;
                            TMP_Text answer = (TMP_Text)___countTmp.GetValue(busterMachine);
                            int correct = (int)___solution.GetValue(busterMachine);
                            if (answer.text == "")
                            {
                                int alltotal = (int)___startingTotal.GetValue(busterMachine);
                                answer.text = (alltotal - correct).ToString();
                                answer.color = Color.green;
                                busterMachine.gameObject.GetComponent<AudioManager>().PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "CashBell"));
                            }
                            else
                                timeLooking = 0f;
                        }
                    }
                }
                else if (timeLooking > 0f)
                    timeLooking -= 1.5f * (pm.PlayerTimeScale * Time.deltaTime);
            }
        }

        public IEnumerator ThinkerDrain()
        {
            bool blinded = false;
            float time = 0f;
            while (BaseGameManager.Instance.Ec.Active)
            {
                time = 15f;
                while (CoreGameManager.Instance.GetPoints(pm.playerNumber) > 0)
                {
                    time = 15f;
                    if (blinded)
                    {
                        blinded = false;
                        BaseGameManager.Instance.Ec.RemoveFog(ThinkerBlind);
                        musicMan.FlushQueue(true);
                    }
                    while (time > 0f)
                    {
                        if (CoreGameManager.Instance.GetPoints(pm.playerNumber) <= 0)
                            time = 0f;
                        time -= pm.PlayerTimeScale * Time.deltaTime;
                        yield return null;
                    }
                    if (!BaseGameManager.Instance.Ec.Active)
                        yield break;
                    if (CoreGameManager.Instance.GetPoints(pm.playerNumber) > 0) CoreGameManager.Instance.AddPoints(CoreGameManager.Instance.GetPoints(pm.playerNumber) < 10 ? -CoreGameManager.Instance.GetPoints(pm.playerNumber) : -10, 0, true, true);
                    yield return null;
                }
                if (CoreGameManager.Instance.GetPoints(pm.playerNumber) <= 0 && !blinded)
                {
                    blinded = true;
                    BaseGameManager.Instance.Ec.AddFog(ThinkerBlind);
                    musicMan.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "LAt_BlindStart"), true);
                    musicMan.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "LAt_BlindLoop"));
                    musicMan.SetLoop(true);
                }
                yield return null;
            }
            yield break;
        }
    }
}
#endif