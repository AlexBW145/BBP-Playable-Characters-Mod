using System.Collections;
using UnityEngine;
using System.Linq;
using BBP_Playables.Core;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;

namespace BBP_Playables.Modded.BBTimes
{
    public class ITM_MagicWandMagical : Item, IEntityTrigger
    {
        private EnvironmentController ec;
        public Entity entity;
        public SpriteRenderer spriteRenderer;
        private static float cooldown = 0f;
        private float existCooldown;
        public override bool Use(PlayerManager pm)
        {
            if (pm.GetPlayable().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "Magical Student".ToLower().Replace(" ", ""))
            {
                Destroy(gameObject);
                CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Last(x => x.name == "BAL_Break"));
                return true;
            }
            if (cooldown > 0f)
            {
                Destroy(gameObject);
                return false;
            }
            ec = pm.ec;
            transform.position = pm.transform.position;
            transform.forward = CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward;
            spriteRenderer.enabled = false;
            entity.Initialize(ec, transform.position);
            entity.SetFrozen(true);
            pm.StartCoroutine(MagicShoot(pm));
            return false;
        }

        private IEnumerator MagicShoot(PlayerManager pm)
        {
            PlayableCharsPlugin.Instance.StartCoroutine(Cooldown());
            ValueModifier modifier = new ValueModifier(0.1f);
            pm.GetComponent<PlayerMovementStatModifier>().AddModifier("walkSpeed", modifier);
            pm.GetComponent<PlayerMovementStatModifier>().AddModifier("runSpeed", modifier);
            float cool = 1.5f;
            while (cool > 0f)
            {
                if (!CoreGameManager.Instance.audMan.QueuedAudioIsPlaying) // I ain't loopin' dat.
                    CoreGameManager.Instance.audMan.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(a => a.name == "Vfx_MGS_PrepMagic"), true);
                cool -= pm.PlayerTimeScale * Time.deltaTime;
                yield return null;
            }

            cool = 0.3f;
            CoreGameManager.Instance.audMan.FlushQueue(true);
            CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(a => a.name == "Vfx_MGS_Magic"));
            existCooldown = 5f;
            spriteRenderer.enabled = true;
            entity.SetFrozen(false);
            transform.position = pm.transform.position;
            transform.forward = CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward;
            entity.ExternalActivity.moveMods.Clear();
            entity.SetHeight(5f);
            while (cool > 0f)
            {
                cool -= pm.PlayerTimeScale * Time.deltaTime;
                yield return null;
            }
            modifier.multiplier = 1f;
            pm.GetComponent<PlayerMovementStatModifier>().RemoveModifier(modifier);
        }

        private void Update()
        {
            if (spriteRenderer.enabled)
            {
                entity.UpdateInternalMovement(transform.forward * 45f * ec.EnvironmentTimeScale);
                if (existCooldown <= 0f)
                    Destroy(gameObject);
                else
                    existCooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
            }

        }

        public void EntityTriggerEnter(Collider other, bool isValid)
        {
            if (!spriteRenderer.enabled)
                return;

            bool flag = other.CompareTag("NPC");
            if (!other.isTrigger)
                return;

            if (flag)
            {
                NPC component = other.GetComponent<NPC>();
                if (component != null && ec.offices.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, ec.offices.Count);
                    component.Navigator.Entity.Teleport(ec.offices[index].RandomEntitySafeCellNoGarbage().CenterWorldPosition);
                    component.SentToDetention();
                    Destroy(gameObject);
                }
            }
        }

        public void EntityTriggerStay(Collider other, bool isValid)
        {
        }

        public void EntityTriggerExit(Collider other, bool isValid)
        {
        }

        /*void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Wall") && other.name.Contains("WallCollider") && other.gameObject.activeSelf && spriteRenderer.enabled && other.enabled)
                Destroy(gameObject, 2f);
        }*/

        static IEnumerator Cooldown()
        {
            cooldown = 15.5f;
            while (cooldown > 0f)
            {
                if (BaseGameManager.Instance.Ec == null)
                {
                    cooldown = 0f;
                    yield break;
                }
                cooldown -= BaseGameManager.Instance.Ec.EnvironmentTimeScale * Time.deltaTime;
                yield return null;
            }
            cooldown = 0f;
        }
    }
}
