using BBP_Playables.Core;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace BBP_Playables.Extra.Foxo
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", MTM101BaldiDevAPI.VersionNumber)]
    [BepInDependency("alexbw145.baldiplus.playablecharacters", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("alexbw145.baldiplus.teacherapi", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("BALDI.exe")]
    [BepInProcess("Baldi's Basics Plus Prerelease.exe")]
    public class FoxoPlayablePlugin : BaseUnityPlugin
    {
        internal static AssetManager assetMan = new AssetManager();
        public static PlayableCharacter FoxoPlayable { get; private set; }
        public static WrathFoxo Foxo { get; private set; }
        private void Awake()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAllConditionals();

            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoad, LoadingEventOrder.Post);
            ModdedSaveGame.AddSaveHandler(Info);
            AssetLoader.LocalizationFromMod(this);
        }

        IEnumerator PreLoad() // IMPORTANT!!
        {
            yield return 1;
            yield return "Adding insanity character";
            FoxoPlayable = new PlayableCharacterBuilder<InsanityComponent>(Info)
                .SetNameAndDesc("The Traumatized", "Desc_Traumatized")
                .SetPortrait(PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Placeholder"))
                .SetStats(sd: 5f, sr: 25f, maxslots: 9)
                .Build();
            assetMan.AddRange([ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "InsaneMus", "Eerie.wav"), "Eerie", SoundType.Music, Color.clear, 0f),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "InsaneMus", "wrath1.wav"), "Wrath", SoundType.Music, Color.clear, 0f),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "wrathslap.wav"), "??", SoundType.Effect, Color.gray)],
            ["Insane/Eerie", "Insane/Wrath", "HalluFoxo/Slap"]);
            Foxo = new NPCBuilder<WrathFoxo>(Info)
                .SetName("Hallucino Foxo")
                .SetEnum("Foxo")
                .SetMetaTags(["teacher"])
                .SetForcedSubtitleColor(Color.gray)
                .DisableNavigationPrecision()
                .AddTrigger()
                .AddLooker()
                .Build();
            Foxo.loseSounds = [new WeightedSoundObject()
            {
                selection = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "scare.wav"), "Sfx_Lose_Buzz", SoundType.Effect, Color.gray, 1.5f),
                weight = 100
            }];
            Foxo.slap = assetMan.Get<SoundObject>("HalluFoxo/Slap");
            Foxo.baseAnger = 0.1f + 1;
            Foxo.baseSpeed = 5f * 0.6f;
            Foxo.speedMultiplier = 1.652f;
            Foxo.ReflectionSetVariable("speedCurve", NPCMetaStorage.Instance.Get(Character.Baldi).prefabs["Baldi_Main3"].ReflectionGetVariable("speedCurve"));
            Foxo.ReflectionSetVariable("slapCurve", NPCMetaStorage.Instance.Get(Character.Baldi).prefabs["Baldi_Main3"].ReflectionGetVariable("slapCurve"));
            assetMan.Add("HallucinoFoxo", Foxo);
            var sprites = AssetLoader.SpritesFromSpritesheet(3, 1, 30f, Vector2.one / 2, AssetLoader.TextureFromMod(this, "Texture2D", "hallucinofoxo.png"));
            var animator = Foxo.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = Foxo.spriteRenderer[0];
            Foxo.ReflectionSetVariable("animator", animator);
            Foxo.spriteRenderer[0].sprite = sprites.First();
            assetMan.Add("HallucinoFoxoSprites", sprites);
        }
        // Pre-defined stuff for foods and drinks
        void PostLoad()
        {
            foreach (var food in ItemMetaStorage.Instance.FindAllWithTags(false, "food"))
            {
                switch (food.value.itemType.ToStringExtended().ToLower())
                {
                    case "zestybar":
                        food.tags.Add("playablechars_sanityconsumable_10");
                        break;
                    case "cottoncandy":
                        food.tags.Add("playablechars_sanityconsumable_5");
                        break;
                    case "bsoda":
                    case "dietbsoda":
                        food.tags.Add("playablechars_sanityconsumable_-8.5");
                        break;
                    case "nanapeel":
                        break;
                }
            }
            foreach (var drink in ItemMetaStorage.Instance.FindAllWithTags(false, "drink"))
            {
                switch (drink.value.itemType.ToStringExtended().ToLower())
                {
                    case "speedpotion":
                        drink.tags.Add("playablechars_sanityconsumable_-10");
                        break;
                    case "waterbottle":
                        drink.tags.Add("playablechars_sanityconsumable_15.5");
                        break;
                    case "hotchocolate":
                        drink.tags.Add("playablechars_sanityconsumable_2");
                        break;
                    case "bsed":
                        drink.tags.Add("playablechars_sanityconsumable_-8.5");
                        break;
                }
            }
        }
    }

    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "alexbw145.baldiplus.playablecharacters.foxo";
        public const string PLUGIN_NAME = "Custom Playable Characters in Baldi's Basics Plus (Extra - Foxo)";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}
