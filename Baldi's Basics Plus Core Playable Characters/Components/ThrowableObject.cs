using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace BBP_Playables.Core
{
    // BBCR Code wtf
    public class ThrowableObject : MonoBehaviour, IClickable<int>
    {
        public bool ClickableHidden()
        {
            return PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") != "CYLN_LOON".ToLower().Replace(" ", "") || !ready || held;
        }

        public bool ClickableRequiresNormalHeight()
        {
            return true;
        }

        public void ClickableSighted(int player)
        {
        }

        public void ClickableUnsighted(int player)
        {
        }

        public void Clicked(int player)
        {
            if (!ready || held ||
                CoreGameManager.Instance.GetPlayer(player).GetComponent<PlrPlayableCharacterVars>().GetCurrentPlayable().name.ToLower().Replace(" ", "") != "CYLN_LOON".ToLower().Replace(" ", "")) return;
            clickBuffer = true;
            held = true;
            heldSelf = true;
            this.player = CoreGameManager.Instance.GetPlayer(player);
        }

        private bool ready = false;
        private bool thrown = false;
        private static bool held = false;
        private bool heldSelf = false;
        private bool clickBuffer = false;
        private EnvironmentController ec;
        private PlayerManager player;
        private LayerMask initLayer;
        private float life = 10f;
        private MovementModifier moveMod = new MovementModifier(default(Vector3), 0.12f);
        private MaterialPropertyBlock spriteProperties;

        void Start()
        {
            held = false;
            ec = BaseGameManager.Instance.Ec;
            initLayer = gameObject.layer;
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 4f;
            Destroy(gameObject?.GetComponent<BoxCollider>());
            /*gameObject.AddComponent<CapsuleCollider>().radius = 0.5f;
            gameObject.GetComponent<CapsuleCollider>().height = 8f;
            gameObject.GetComponent<CapsuleCollider>().direction = 1;*/
            Destroy(gameObject?.GetComponent<NavMeshObstacle>());
            //gameObject.AddComponent<ActivityModifier>();
            gameObject.AddComponent<Rigidbody>().mass = 1f;
            gameObject.GetComponent<Rigidbody>().angularDrag = 0f;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            ready = true;
        }

        void Update()
        {
            if (!ready) return;
            if (!heldSelf && !thrown) transform.localPosition = new Vector3(transform.localPosition.x, 3.5f + PickupBobValue.bobVal, transform.localPosition.z);
            if (heldSelf && player != null)
            {
                gameObject.layer = 29;
                transform.localEulerAngles = CoreGameManager.Instance.GetCamera(player.playerNumber).transform.localEulerAngles;
                if (!clickBuffer && InputManager.Instance.GetDigitalInput("Interact", true))
                {
                    thrown = true;
                    transform.position = new Vector3(player.transform.position.x, 3.5f, player.transform.position.z);
                    transform.forward = CoreGameManager.Instance.GetCamera(player.playerNumber).transform.forward * 4f;
                    held = false;
                    heldSelf = false;
                    /*if (gameObject.GetComponent<Entity>() == null)
                    {
                        gameObject.SetActive(false);
                        Entity entity = gameObject.AddComponent<Entity>();
                        {
                            //entity.ReflectionSetVariable("rendererBase", transform);
                            entity.ReflectionSetVariable("externalActivity", gameObject.GetComponent<ActivityModifier>());
                            entity.ReflectionSetVariable("collider", gameObject.GetComponent<CapsuleCollider>());
                            entity.ReflectionSetVariable("trigger", gameObject.GetComponent<SphereCollider>());
                            entity.ReflectionSetVariable("grounded", false);
                            entity.ReflectionSetVariable("externalFriction", 0f);
                        }
                        gameObject.SetActive(true);
                        entity.Initialize(ec, transform.position);
                    }*/
                }
                clickBuffer = false;
            }
            else
                gameObject.layer = LayerMask.NameToLayer("Default");
            if (thrown)
            {
                transform.position += transform.forward * 100f * (ec.EnvironmentTimeScale * Time.deltaTime);
                life -= (ec.EnvironmentTimeScale * Time.deltaTime);
                if (life <= 0f)
                    Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (heldSelf && !thrown && player != null)
                transform.position = (player.transform.position + (Vector3.down * 3f)) + (CoreGameManager.Instance.GetCamera(player.playerNumber).transform.forward * 4f);
        }

        void OnEnable()
        {
            if (!ready || heldSelf) return;
            transform.localPosition = new Vector3(transform.localPosition.x, PickupBobValue.bobVal, transform.localPosition.z);
        }

        void OnTriggerEnter(Collider other)
        {
            if (thrown && other.CompareTag("NPC") && other.isTrigger)
            {
                other.gameObject.GetComponent<AudioManager>()?.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "Lose_Buzz"));
                other.gameObject.GetComponent<Entity>().ExternalActivity.moveMods.Add(moveMod);
                spriteProperties = new MaterialPropertyBlock();
                foreach (SpriteRenderer render in other.GetComponent<NPC>().spriteRenderer)
                    render.GetPropertyBlock(spriteProperties);
                spriteProperties.SetInt("_SpriteColorGlitching", 1);
                spriteProperties.SetFloat("_SpriteColorGlitchPercent", 0.9f);
                spriteProperties.SetFloat("_SpriteColorGlitchVal", UnityEngine.Random.Range(0f, 4096f));
                foreach (SpriteRenderer render in other.GetComponent<NPC>().spriteRenderer)
                    render.SetPropertyBlock(spriteProperties);
                other.gameObject.GetComponent<Entity>().StartCoroutine(waitUntilMovemod(other.gameObject.GetComponent<Entity>()));
                Destroy(gameObject);
            }
            if (!thrown && other.gameObject == player?.gameObject)
            {
                held = true;
                heldSelf = true;
                clickBuffer = true;
            }
        }

        IEnumerator waitUntilMovemod(Entity enit)
        {
            float time = 1.2f;
            while (time > 0f)
            {
                time -= ec.NpcTimeScale * Time.deltaTime;
                foreach (SpriteRenderer render in enit.gameObject.GetComponent<NPC>().spriteRenderer)
                    render.GetPropertyBlock(spriteProperties);
                spriteProperties.SetFloat("_SpriteColorGlitchVal", UnityEngine.Random.Range(0f, 4096f));
                foreach (SpriteRenderer render in enit.gameObject.GetComponent<NPC>().spriteRenderer)
                    render.SetPropertyBlock(spriteProperties);
                yield return null;
            }
            foreach (SpriteRenderer render in enit.gameObject.GetComponent<NPC>().spriteRenderer)
                render.GetPropertyBlock(spriteProperties);
            spriteProperties.SetInt("_SpriteColorGlitching", 0);
            foreach (SpriteRenderer render in enit.gameObject.GetComponent<NPC>().spriteRenderer)
                render.SetPropertyBlock(spriteProperties);
            if (enit.ExternalActivity.moveMods.Contains(moveMod)) enit.ExternalActivity.moveMods.Remove(moveMod);

            yield break;
        }
    }
}
