using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
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

        void Update()
        {
            if (pm.ec.Active)
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
                    if (hits[i].collider.gameObject.GetComponentInParent<MathMachine>() != null)
                        mathMachineVisible = true;
                }

                if (mathMachineVisible && pm.plm.Entity.CurrentRoom.category == RoomCategory.Class)
                {
                    timeLooking += pm.PlayerTimeScale * Time.deltaTime;
                    if (timeLooking > 1f && _hitTransforms.Exists(x => x.gameObject.GetComponentInParent<MathMachine>() != null))
                    {
                        TMP_Text answer = (TMP_Text)_hitTransforms.Find(x => x.gameObject.GetComponentInParent<MathMachine>() != null).gameObject.GetComponentInParent<MathMachine>().ReflectionGetVariable("answerText");
                        if (answer.text == "?") {
                            int correct = (int)_hitTransforms.Find(x => x.gameObject.GetComponentInParent<MathMachine>() != null).gameObject.GetComponentInParent<MathMachine>().ReflectionGetVariable("answer");
                            answer.text = correct.ToString();
                            answer.color = Color.green;
                            // 10+ answers from Times are fucky...
                            answer.autoSizeTextContainer = false;
                            answer.autoSizeTextContainer = true;
                            _hitTransforms.Find(x => x.gameObject.GetComponentInParent<MathMachine>() != null).gameObject.GetComponentInParent<MathMachine>().gameObject.GetComponent<AudioManager>().PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "CashBell"));
                        }
                        else
                            timeLooking = 0f;
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
                    if (CoreGameManager.Instance.GetPoints(pm.playerNumber) > 0) CoreGameManager.Instance.AddPoints(CoreGameManager.Instance.GetPoints(pm.playerNumber) < 10 ? -CoreGameManager.Instance.GetPoints(pm.playerNumber) : -10, 0, true);
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