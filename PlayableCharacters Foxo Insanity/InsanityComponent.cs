using BBP_Playables.Core;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace BBP_Playables.Extra.Foxo
{
    public class InsanityModifier
    {
        public float insaneAura;
        public int priority;
        public InsanityModifier(float aura)
        {
            insaneAura = aura;
            priority = -1; // Can stack
        }

        public InsanityModifier(float aura, int prior)
        {
            insaneAura = aura;
            priority = prior; // Can't stack
        }
    }

    public class InsanityAura : MonoBehaviour
    {
        [SerializeField] internal float radius;
        [SerializeField] internal InsanityModifier modifier = new InsanityModifier(-0.5f, 1);
        [SerializeField] internal bool lookOnly;
        private Looker looker;
        void Update()
        {
            foreach (var fox in FindObjectsOfType<InsanityComponent>(false))
                if ((!lookOnly && (transform.position - fox.transform.position).magnitude < radius) || (lookOnly && looker?.PlayerInSight(fox.PlayerManager) == true))
                {
                    if (!fox.modifiers.Contains(modifier))
                        fox.modifiers.Add(modifier);
                }
                else if (fox.modifiers.Contains(modifier))
                    fox.modifiers.Remove(modifier);
        }

        private void RemoveMods()
        {
            foreach (var fox in FindObjectsOfType<InsanityComponent>(false))
                fox.modifiers.Remove(modifier);
        }
        void OnDisable() => RemoveMods();
        void OnDestroy() => RemoveMods();
        void Start() => looker = gameObject.GetComponent<Looker>();
    }

    [RequireComponent(typeof(PlayerManager))]
    public class InsanityComponent : PlayableCharacterComponent
    {
        public PlayerManager PlayerManager => pm;
        internal HudGauge gauge { private get; set; }
        public List<InsanityModifier> modifiers = new List<InsanityModifier>();
        private float insanity;
        [SerializeField] private float maxInsan = 150f;
        public float Anxiety
        {
            get => insanity;
            set => insanity = value;
        }
        private float insanRounded => insanity / maxInsan * 100;
        public bool[] InsanityEffects => [
            insanRounded <= 80f, // Doing the math formulas are hard to think.
            insanRounded <= 75f,
            insanRounded <= 60f,
            insanRounded <= 50f,
            insanRounded <= 45f,
            insanRounded <= 40f,
            insanRounded <= 15f,
            insanRounded <= 10f];
        public bool IsAfraid => (InsanityEffects[6] || currentmus == Wrath) && insanRounded <= 17.5f; // May be stupid...

        private AudioMixer master;
        private AudioManager musicMan;

        private SoundObject Eeerie, Wrath;

        void Awake()
        {
            Eeerie = FoxoPlayablePlugin.assetMan.Get<SoundObject>("Insane/Eerie");
            Wrath = FoxoPlayablePlugin.assetMan.Get<SoundObject>("Insane/Wrath");
        }

        protected override void Start()
        {
            base.Start();
            if (CoreGameManager.Instance.currentMode == Mode.Free || CoreGameManager.Instance.lifeMode == LifeMode.Explorer) return;
            pm.ec.OnEnvironmentBeginPlay += () =>
            {
                if (!pm.ec.npcsToSpawn.Exists(x => (x is Baldi || x.GetType().IsSubclassOf(typeof(Baldi))) && x is not WrathFoxo) && !pm.ec.Npcs.Exists(x => (x is Baldi || x.GetType().IsSubclassOf(typeof(Baldi))) && x is not WrathFoxo)) return;
                if (!pm.ec.Npcs.Exists(x => x.Character == FoxoPlayablePlugin.Foxo.Character)
                && !pm.ec.npcsToSpawn.Exists(x => x.Character == FoxoPlayablePlugin.Foxo.Character)
                && !pm.ec.npcsLeftToSpawn.Exists(x => x.Character == FoxoPlayablePlugin.Foxo.Character))
                    pm.ec.SpawnNPC(FoxoPlayablePlugin.Foxo, pm.ec.RandomCell(false, false, true).position);
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            //modstats = pm.GetMovementStatModifier();
            insanity = maxInsan;
            master = Resources.FindObjectsOfTypeAll<AudioMixer>().ToList().Find(x => x.name == "Master");
            musicMan = Instantiate(CoreGameManager.Instance.musicMan, transform, false);
            //modstats.baseStats.Add("foxoSanity", insanity);
            //modstats.modifiers.Add("foxoSanity", new List<ValueModifier>());

            if (AudioListener.volume != 1f)
                AudioListener.volume = 1f;

            RectTransform stamino = (RectTransform)CoreGameManager.Instance.GetHud(pm.playerNumber).ReflectionGetVariable("staminaNeedle");
            stamino = stamino.parent as RectTransform;
            gauge = CoreGameManager.Instance.GetHud(0).gaugeManager.ActivateNewGauge(PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentUnwrapped").itemSpriteLarge, insanity);
        }

        //private float GetModifiedStat() => modstats.baseStats["foxoSanity"] * modstats.modifiers["foxoSanity"].Multiplier() + modstats.modifiers["foxoSanity"].Addend();

        private IEnumerator wavy;
        private IEnumerator WavyShit()
        {
            bool ohnope = false;
            while (IsAfraid)
            {
                var randomvalue = UnityEngine.Random.Range(0.005f, 0.01f);
                if (MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch > 0.8f && ohnope)
                    MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch -= 0.005f + randomvalue;
                else if (MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch < 1.8f && !ohnope)
                    MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch += 0.005f + randomvalue;
                if (MusicManager.Instance.MidiPlayer.MPTK_Speed > 0.8f && ohnope)
                    MusicManager.Instance.MidiPlayer.MPTK_Speed -= 0.005f + randomvalue;
                else if (MusicManager.Instance.MidiPlayer.MPTK_Speed < 1.8f && !ohnope)
                    MusicManager.Instance.MidiPlayer.MPTK_Speed += 0.005f + randomvalue;
                if (((MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch <= 0.8f || MusicManager.Instance.MidiPlayer.MPTK_Speed <= 0.8f) && ohnope)
                    || ((MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch >= 1.8f || MusicManager.Instance.MidiPlayer.MPTK_Speed >= 1.8f) && !ohnope))
                    ohnope = !ohnope;
                yield return null;
            }
        }

        void Update()
        {
            int num = 0;
            foreach (InsanityModifier insanemods in modifiers)
            {
                if (insanemods.priority > num || insanemods.priority <= -1f)
                    insanity += (insanemods.insaneAura / 60f) * (Time.deltaTime * pm.PlayerTimeScale);
                if (insanemods.priority > num)
                    num = insanemods.priority;
            }
            //insanity = GetModifiedStat();
            if (insanity < 0f) insanity = 0f;
            else if (insanity > maxInsan) insanity = maxInsan;
            //UpdateInsaneBar();
            gauge?.SetValue(maxInsan, insanity);
            master.GetFloat("EchoWetMix", out var mixval);
            if (InsanityEffects[3]) {
                master.SetFloat("EchoWetMix", InsanityEffects[4] ? 1f : 0.5f);
                //master.SetFloat("EchoDecay", InsanityEffects[5] ? 1f : 0.5f);
                if (InsanityEffects[6] && wavy == null)
                {
                    wavy = WavyShit();
                    StartCoroutine(wavy);
                }
                else if (!IsAfraid)
                {
                    if (wavy != null)
                    {
                        StopCoroutine(wavy);
                        wavy = null;
                    }
                    if (MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch > 0.8f)
                        MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch -= 0.0005f;
                    else if (MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch < 0.8f)
                        MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch = 0.8f;
                    if (MusicManager.Instance.MidiPlayer.MPTK_Speed > 0.8f)
                        MusicManager.Instance.MidiPlayer.MPTK_Speed -= 0.0005f;
                    else if (MusicManager.Instance.MidiPlayer.MPTK_Speed < 0.8f)
                        MusicManager.Instance.MidiPlayer.MPTK_Speed = 0.8f;
                }
            }
            else if (mixval == 1f || mixval == 0.5f)
            {
                if (wavy != null)
                {
                    StopCoroutine(wavy);
                    wavy = null;
                }
                master.SetFloat("EchoWetMix", 0f);
                //master.SetFloat("EchoDecay", 0f);
                MusicManager.Instance.MidiPlayer.CoreAudioSource.pitch = 1f;
                MusicManager.Instance.MidiPlayer.MPTK_Speed = (float)MusicManager.Instance.ReflectionGetVariable("speed");
            }
            if (InsanityEffects[5])
            {
                //float value = Mathf.Min(Mathf.Floor((insanity / maxInsan) * insanity) / (maxInsan / insanity), 1f);
                //musicMan.pitchModifier = Mathf.Max(1f, Mathf.Min(value, 0.66f));
            }
            musicMan.pitchModifier = IsAfraid ? 0.66f : 1f;
        }
        // Why am I doing this?
        private bool fading;
        private SoundObject currentmus;
        void LateUpdate()
        {
            musicMan.volumeModifier = MusicManager.Instance.MidiPlaying ? 0.55f : 1f;
            bool foxExists = pm.ec.Npcs.Exists(x => x.Character == FoxoPlayablePlugin.Foxo.Character);
            if (foxExists && IsAfraid && currentmus != Wrath)
            {
                currentmus = Wrath;
                musicMan.FlushQueue(true);
                musicMan.QueueAudio(Wrath, true);
                musicMan.SetLoop(true);
            }
            else if (foxExists && !IsAfraid && InsanityEffects[4] && currentmus != Eeerie)
            {
                currentmus = Eeerie;
                musicMan.FlushQueue(true);
                musicMan.QueueAudio(Eeerie, true);
                musicMan.SetLoop(true);
                //MusicManager.Instance.ModulateSpeed(0.5f, -1f, -2f, 0.66f, false);
            }
            else if (!foxExists || (!IsAfraid && !InsanityEffects[4]))
            {
                if (musicMan.QueuedAudioIsPlaying && !fading)
                {
                    fading = true;
                    musicMan.FadeOut(1.5f);
                    //MusicManager.Instance.ModulateSpeed(0.1f, 1f, 2f, 1f, false);
                }
                else if (!musicMan.QueuedAudioIsPlaying)
                {
                    fading = false;
                    currentmus = null;
                }
            }
        }

        internal void WeDoneWithThatShit()
        {
            gauge.Deactivate();
            modifiers.Add(new InsanityModifier(float.PositiveInfinity));
        }

        /*private void UpdateInsaneBar()
        {
            float num = barspeed * Time.deltaTime;
            barvalue = Mathf.Max(barvalue - num, Mathf.Min(barvalue + num, insanity / maxInsan));
            if (insanebar != null)
            {
                int staminaMinPos = 25;
                int staminaMaxPos = 158;
                int staminaOverPos = 166;
                _size = insanebar.sizeDelta;
                if (barvalue > 1f)
                    _size.y = Mathf.RoundToInt(staminaMaxPos + Mathf.Min((staminaOverPos - staminaMaxPos) * (barvalue - 1f), staminaOverPos));
                else
                    _size.y = Mathf.RoundToInt(staminaMinPos + (staminaMaxPos - staminaMinPos) * barvalue);

                insanebar.sizeDelta = _size;
            }
        }*/
    }
}
