using BBP_Playables.Core;
using MTM101BaldAPI;
using System.Collections;
using UnityEngine;
#if !DEMO
namespace BBP_Playables.Modded.BCPP
{
    public class ITM_FirewallBlaster : Item, IEntityTrigger
    {
        private static bool shooting = false;
        public SoundObject shootSnd;
        public static SoundObject effectSnd;
        public Entity entity;
        public EnvironmentController ec;
        private MovementModifier moveMod = new MovementModifier(default(Vector3), 0.5f);

        public override bool Use(PlayerManager pm)
        {
            ec = pm.ec;
            entity.enabled = false;
            if (pm.GetPlayable().GetCurrentPlayable().name.ToLower().Replace(" ", "") == "themainprotagonist" && !shooting)
                pm.StartCoroutine(Shoot(pm));
            Destroy(gameObject);
            return false;
        }

        private IEnumerator Shoot(PlayerManager pm)
        {
            shooting = true;
            while (InputManager.Instance.GetDigitalInput("UseItem", false))
            {
                pm.RuleBreak("Bullying", 0.9f);
                var gobject = Instantiate(PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster").item.gameObject).GetComponent<ITM_FirewallBlaster>();
                gobject.ec = pm.ec;
                gobject.transform.localScale = Vector3.one/2f;
                gobject.transform.position = pm.transform.position;
                gobject.transform.forward = CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward;
                gobject.entity.Initialize(gobject.ec, gobject.transform.position);
                CoreGameManager.Instance.audMan.PlaySingle(gobject.shootSnd);
                yield return new WaitForSecondsEnvironmentTimescale(pm.ec, 0.11f);
            }
            shooting = false;
            yield break;
        }

        private void Update()
        {
            entity.UpdateInternalMovement(transform.forward * 80f * ec.EnvironmentTimeScale);
        }

        public void EntityTriggerEnter(Collider other, bool isValid)
        {
            if (other.CompareTag("NPC") && other.GetComponent<NPC>())
            {
                other.GetComponent<NPC>().Navigator?.Entity?.MoveWithCollision(transform.forward * 0.9f);
                Destroy(gameObject);
                other.GetComponent<AudioManager>()?.PlaySingle(effectSnd);
                other.GetComponent<NPC>().StopCoroutine(movemodEffct(other.GetComponent<NPC>()));
                other.GetComponent<NPC>().StartCoroutine(movemodEffct(other.GetComponent<NPC>()));
            }
        }

        private IEnumerator movemodEffct(NPC npc)
        {
            if (!npc.gameObject.GetComponent<Entity>().ExternalActivity.moveMods.Contains(moveMod))
                npc.gameObject.GetComponent<Entity>().ExternalActivity.moveMods.Add(moveMod);
            float time = 1f;
            while (time > 0f)
            {
                if (npc == null)
                    yield break;
                time -= 1f * (npc.TimeScale * Time.deltaTime);
                yield return null;
            }
            npc.gameObject.GetComponent<Entity>().ExternalActivity.moveMods.Remove(moveMod);
            yield break;
        }

        public void EntityTriggerExit(Collider other, bool isValid)
        {
        }

        public void EntityTriggerStay(Collider other, bool isValid)
        {
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Wall") && other.gameObject.activeSelf && other.enabled)
                Destroy(gameObject, 0.5f);
        }
    }
}
#endif