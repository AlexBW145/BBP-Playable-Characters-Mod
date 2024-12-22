using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Types;
#if !DEMO
namespace BBP_Playables.Core
{
    public class TestInvention : TinkerneerObject, IItemAcceptor
    {
        private AudioManager audMan;
        private DijkstraMap dijkstraMap;
        private List<NavigationState_WanderFleeOverride> fleeStates = new List<NavigationState_WanderFleeOverride>();
        private EnvironmentController ec;
        public SpriteRenderer render;
        private bool playing;
        public override void Create(ItemManager itm)
        {
            audMan = gameObject.AddComponent<AudioManager>();
            audMan.audioDevice = gameObject.AddComponent<AudioSource>();
            audMan.audioDevice.playOnAwake = false;
            audMan.audioDevice.dopplerLevel = 0f;
            audMan.audioDevice.spatialBlend = 1f;
            audMan.audioDevice.rolloffMode = AudioRolloffMode.Linear;
            ec = itm.pm.ec;
            dijkstraMap = new DijkstraMap(ec, PathType.Const, base.transform);
            InsertItem(itm.pm, ec);
        }

        public void InsertItem(PlayerManager player, EnvironmentController ec)
        {
            playing = true;
            render.sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("Inventions/TapeQuarterPlayerClosed");
            ec.MakeSilent(30f);
            audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "AntiHearing"));
            StartCoroutine(Cooldown());
            dijkstraMap.QueueUpdate();
            dijkstraMap.Activate();

            if (ec.GetBaldi() != null)
                ec.GetBaldi().Distract();
        }

        public bool ItemFits(Items item)
        {
            return (item == Items.Quarter || item == Items.Tape) && !playing;
        }

        private IEnumerator Cooldown()
        {
            while (dijkstraMap.PendingUpdate)
                yield return null;

            foreach (NPC npc in ec.Npcs)
            {
                if (npc.Navigator.enabled)
                {
                    NavigationState_WanderFleeOverride navigationState_WanderFleeOverride = new NavigationState_WanderFleeOverride(npc, 31, dijkstraMap);
                    fleeStates.Add(navigationState_WanderFleeOverride);
                    npc.navigationStateMachine.ChangeState(navigationState_WanderFleeOverride);
                }
            }

            while (audMan.QueuedAudioIsPlaying)
                yield return null;

            foreach (NavigationState_WanderFleeOverride fleeState in fleeStates)
                fleeState.End();

            dijkstraMap.Deactivate();
            fleeStates.Clear();
            playing = false;
            render.sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("Inventions/TapeQuarterPlayerOpen");
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") != "thetinkerneer")
                Destroy(gameObject);
        }
    }

    public class FakeStudentInvention : TinkerneerObject
    {
        private Baldi baldman;
        private bool recognizedAsFake = false;
        public override void Create(ItemManager itm)
        {
            baldman = BaseGameManager.Instance.Ec.GetBaldi();
        }

        void Update()
        {
            if (baldman == null) return;
            baldman.looker.Raycast(transform, Mathf.Min((baldman.transform.position - transform.position).magnitude + baldman.Navigator.Velocity.magnitude, baldman.looker.distance, baldman.ec.MaxRaycast), LayerMask.GetMask("Default", "Block Raycast", "Windows"), out var sighted);
            if (sighted && !recognizedAsFake)
                if (!baldman.looker.PlayerInSight())
                    baldman.Hear(transform.position, 114, false);
                else if (baldman.looker.PlayerInSight() || Vector3.Distance(transform.position, baldman.transform.position) < Mathf.Min(baldman.looker.distance, baldman.ec.MaxRaycast)) {
                    recognizedAsFake = true;
                    baldman.ClearSoundLocations();
                }
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("NPC") && (other.GetComponent<NPC>().Character == Character.Baldi || other.gameObject.GetComponent<Baldi>()))
            {
                WeightedSelection<SoundObject>[] losesnds = baldman.loseSounds;
                baldman.AudMan.PlaySingle(WeightedSelection<SoundObject>.RandomSelection(losesnds));
                Destroy(gameObject);
            }
        }
    }

    public class RealStudentInvention : TinkerneerObject
    {
        private bool Apple = false;
        private Baldi baldman;

        public override void Create(ItemManager itm)
        {
            Apple = requiredItems.Contains(ItemMetaStorage.Instance.FindByEnum(Items.Apple).value);
            baldman = BaseGameManager.Instance.Ec.GetBaldi();
        }

        void Update()
        {
            if (baldman == null) return;
            baldman.looker.Raycast(transform, Mathf.Min((baldman.transform.position - transform.position).magnitude + baldman.Navigator.Velocity.magnitude, baldman.looker.distance, baldman.ec.MaxRaycast), LayerMask.GetMask("Default", "Block Raycast", "Windows"), out var sighted);
            if (sighted && !baldman.looker.PlayerInSight()) {
                baldman.ClearSoundLocations();
                baldman.Hear(transform.position, 127, false);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("NPC") && (other.GetComponent<NPC>().Character == Character.Baldi || other.gameObject.GetComponent<Baldi>()))
            {
                if (Apple)
                    other.gameObject.GetComponent<Baldi>().TakeApple();
                WeightedSelection<SoundObject>[] losesnds = baldman.loseSounds;
                baldman.AudMan.PlaySingle(WeightedSelection<SoundObject>.RandomSelection(losesnds));
                Destroy(gameObject);
            }
        }
    }

    public class MathMachineRegen : TinkerneerObject
    {
        private EnvironmentController ec;
        public SpriteRenderer render;
        private AudioManager audMan;
        private float[] times = [
            50f,
            140f,
            230f,
            320f
        ];
        private int amountTimes;

        public override void Create(ItemManager itm)
        {
            ec = itm.pm.ec;
            audMan = gameObject.AddComponent<PropagatedAudioManager>();
            audMan.audioDevice = gameObject.AddComponent<AudioSource>();
            audMan.audioDevice.spatialBlend = 1f;
            audMan.audioDevice.playOnAwake = false;
            audMan.audioDevice.dopplerLevel = 0f;
            audMan.audioDevice.rolloffMode = AudioRolloffMode.Linear;
            audMan.audioDevice.maxDistance = 250f;
            audMan.pitchModifier = 1.5f;
            audMan.volumeModifier = 4f;
            audMan.positional = true;
            audMan.ReflectionSetVariable("disableSubtitles", true);

            StartCoroutine(Sensor());
        }

        private IEnumerator Sensor()
        {
            RoomController room = ec.CellFromPosition(transform.position).room;
            if (room.category != RoomCategory.Class || !ec.activities.Exists(x => x.room == room) || !ec.activities.Find(x => x.room == room).GetType().Equals(typeof(MathMachine)))
            {
                render.color = Color.gray;
                yield break;
            }
            while (!ec.activities.Find(x => x.room == room).IsCompleted)
                yield return null;
            yield return new WaitForSecondsEnvironmentTimescale(ec, times[amountTimes]);
            ec.GetBaldi()?.Hear(transform.position, 78, true);
            ec.activities.Find(x => x.room == room).Corrupt(false);
            ec.activities.Find(x => x.room == room).SetBonusMode(true);
            render.sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("Inventions/BonusGenActive");
            audMan.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "Elv_Buzz"), true);
            audMan.SetLoop(true);
            amountTimes++;
            if (amountTimes > times.Length - 1) amountTimes = times.Length - 1;
            while (!ec.activities.Find(x => x.room == room).IsCompleted)
                yield return null;
            render.sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("Inventions/BonusGen");
            audMan.FlushQueue(true);
            StartCoroutine(Sensor());
            yield break;
        }
    }
}
#endif