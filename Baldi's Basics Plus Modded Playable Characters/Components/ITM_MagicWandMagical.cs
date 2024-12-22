using System.Collections;
using UnityEngine;
using System.Linq;
using BBP_Playables.Core;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using BBTimes.CustomContent.NPCs;
using static UnityEngine.GraphicsBuffer;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.CustomItems;
using static Entity;

namespace BBP_Playables.Modded.BBTimes
{
    public class ITM_MagicWandMagical : Item, IEntityTrigger
    {
        private EnvironmentController ec;
        public Entity entity;
        public SpriteRenderer spriteRenderer;
        private static float cooldown = 0f;
        public override bool Use(PlayerManager pm)
        {
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
            return PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") != "magicalstudent";
        }

        private IEnumerator MagicShoot(PlayerManager pm)
        {
            PlayableCharsPlugin.Instance.StartCoroutine(Cooldown());
            ValueModifier modifier = new ValueModifier(0.1f);
            pm.GetComponent<PlayerMovementStatModifier>().AddModifier("walkSpeed", modifier);
            pm.GetComponent<PlayerMovementStatModifier>().AddModifier("runSpeed", modifier);
            float cool = 4.5f;
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
                entity.UpdateInternalMovement(transform.forward * 45f * ec.EnvironmentTimeScale);
        }

        public void EntityTriggerEnter(Collider other)
        {
            if (!spriteRenderer.enabled)
                return;

            bool flag = other.CompareTag("NPC");
            if (!other.isTrigger)
                return;

            if (flag)
            {
                NPC component = other.GetComponent<NPC>();
                if (component != null)
                {
                    int index = UnityEngine.Random.Range(0, ec.offices.Count);
                    component.Navigator.Entity.Teleport(ec.offices[index].RandomEntitySafeCellNoGarbage().CenterWorldPosition);
                    component.SentToDetention();
                    Destroy(gameObject);
                }
            }
        }

        public void EntityTriggerStay(Collider other)
        {
        }

        public void EntityTriggerExit(Collider other)
        {
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Wall") && other.name.Contains("WallCollider") && other.gameObject.activeSelf && spriteRenderer.enabled && other.enabled)
                Destroy(gameObject, 2f);
        }

        static IEnumerator Cooldown()
        {
            cooldown = 23.5f;
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
