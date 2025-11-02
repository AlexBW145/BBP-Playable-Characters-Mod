using BBP_Playables.Core.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityCipher;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BBP_Playables.Core
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", "9.0.0.0")]
    [BepInDependency("mtm101.rulerp.baldiplus.criminalpackroot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("alexbw145.baldiplus.arcadeendlessforever", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("BALDI.exe")]
    [BepInProcess("Baldi's Basics Plus Prerelease.exe")]
    public class PlayableCharsPlugin : BaseUnityPlugin
    {
        private const string PLUGIN_GUID = "alexbw145.baldiplus.playablecharacters";
        private const string PLUGIN_NAME = "Custom Playable Characters in Baldi's Basics Plus (Core - Base Game)";
        private const string PLUGIN_VERSION = "0.1.3.3"; // UPDATE EVERY TIME!!

        public static PlayableCharsPlugin Instance { get; private set; }
        public static PlayableCharacterMetaStorage playablesMetaStorage { get; private set; } = new PlayableCharacterMetaStorage();
        internal static bool gameStarted = false;

        public static AssetManager assetMan = new AssetManager();
        [Obsolete("Use PlayableCharacterMetaStorage.Instance.All() instead.", true)] public static List<PlayableCharacter> characters => playablesMetaStorage.All().ToValues().ToList();
        public static bool IsRandom => Instance?.extraSave?.Item1.componentType == typeof(PlayableRandomizer);
        internal bool unlockedCylnLoon { get; private set; } = false;
        private static SoundObject NewCharacterUnlocked => (SoundObject)assetMan["Music/NewPlayerUnlocked"];
        private static PlayableUnlockedUI PlayableUnlockedUI => (PlayableUnlockedUI)assetMan["UnlockedCharacterUI"];
        public PlayableCharacter Character => PlayableCharsGame.Character;
        internal PlayableCharsGame gameSave = new PlayableCharsGame();
        internal Tuple<PlayableCharacter> extraSave;

        public static ManualLogSource Log = new ManualLogSource("Playable Characters Mod Log");

        // TODO:
        /*
         * CYLN_LOON is done
         * Partygoer and his items are done
         * Troublemaker is barely done
         * Thinker is done
         * Backpacker is done
         * Tinkerneer is very not done
         * Test Subject is done
         * Speedrunner is done
         */
        private void Awake()
        {
            Log = this.Logger;
            Harmony harmony = new Harmony(PLUGIN_GUID);
            Instance = this;
            harmony.PatchAllConditionals();
            MTM101BaldiDevAPI.AddWarningScreen(@"<color=orange>THIS MOD IS UNFINISHED!</color>
Playable Characters Mod is a unfinished and this build is the <color=orange>full mod public demo</color> edition, things are subject to change!
There will be improvements and additions once new updates come out, but some characters currently does not have such exclusivity.", false);
            LoadingEvents.RegisterOnAssetsLoaded(Info, BBCRDataLoad(), LoadingEventOrder.Start);
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, ExtraLoad(), LoadingEventOrder.Post);
            ModdedSaveGame.AddSaveHandler(gameSave);
            AssetLoader.LocalizationFromMod(this);

            assetMan.AddRange<Sprite>([
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Placehold.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Default.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Fanon.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "FanonTimes.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Cylnloon.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Partygoer.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Thinker.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "TestSubject.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Troublemaker.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Backpacker.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Tinkerneer.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Speedrunner.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Random.png"),

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "BackpackerBackpack_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "BackpackerBackpack_Small.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "BackpackerBackpack_SmallOpen.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "TinkerneerWrench_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "TinkerneerWrench_Small.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "PartygoerPresent_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "PartygoerPresent_Small.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "PartygoerWrappingBundle_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "PartygoerWrappingBundle_Small.png"),

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "Inventions", "InventionPlayerOpen.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "Inventions", "InventionPlayerClosed.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 32f, "Texture2D", "Inventions", "StudentcrowFake.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 32f, "Texture2D", "Inventions", "StudentcrowReal.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "Inventions", "BonusGen.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "Inventions", "BonusGenActive.png"),
            ],
            [
                "Portrait/Placeholder",
                "Portrait/Default",
                "Portrait/Fanon",
                "Portrait/FanonBBT",
                "Portrait/Cylnloon", // Just a cylinder...
                "Portrait/Partygoer", // By CottyKot on Gamebanana
                "Portrait/Thinker", // I should request a redesign... Redrawn by BigThinker
                "Portrait/TestSubject",
                // By https://gamebanana.com/members/3165945
                "Portrait/Troublemaker",
                "Portrait/Backpacker",
                "Portrait/Tinkerneer",
                "Portrait/Speedrunner",
                //
                "Portrait/Random",

                "Items/BackpackerBackpack_Large",
                "Items/BackpackerBackpack_Small",
                "Items/BackpackerBackpack_SmallOpen",
                "Items/TinkerneerWrench_Large",
                "Items/TinkerneerWrench_Small",
                "Items/Present_Large",
                "Items/Present_Small",
                "Items/WrappingBundle_Large",
                "Items/WrappingBundle_Small",

                "Inventions/TapeQuarterPlayerOpen",
                "Inventions/TapeQuarterPlayerClosed",
                "Inventions/StudentcrowFake",
                "Inventions/StudentcrowReal",
                "Inventions/BonusGen",
                "Inventions/BonusGenActive",
            ]);
            assetMan.AddRange<SoundObject>([
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "mus_newplayerunlocked.mp3"), "New Player Unlocked", SoundType.Music, Color.clear),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering1.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering2.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering3.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "NPC_openingpresent1.wav"), "Sfx_ShrinkMachine_Door", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "NPC_openingpresent2.wav"), "Sfx_ShrinkMachine_Door", SoundType.Effect, Color.white)
            ],
            [
                "Music/NewPlayerUnlocked",
                "Items/Tinkerneering1",
                "Items/Tinkerneering2",
                "Items/Tinkerneering3",
                "Items/PresentOpen1",
                "Items/PresentOpen2",
            ]);
/*#if DEBUG
            assetMan.AddRange<string>([
                AssetLoader.MidiFromMod("mus_charSel", this, "mus_charSel.mid"),
                AssetLoader.MidiFromMod("mus_charSelINF", this, "mus_charSelINF.mid")
            ],
            [
                "charSel",
                "charSelINF"
            ]);
#endif*/
}

        private IEnumerator ExtraLoad()
        {
            yield return 1 + (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.arcadeendlessforever") ? 1 : 0) + (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader") ? 2 : 0) + (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio") ? 1 : 0);
            yield return "Getting compats...";
            var wrappedmeta = assetMan.Get<ItemObject>("PresentGift_Baldi").GetMeta();
            wrappedmeta.itemObjects = wrappedmeta.itemObjects.AddToArray(assetMan.Get<ItemObject>("PresentUnwrapped"));
            if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.arcadeendlessforever"))
                yield return ArcadeAdds.EndlessLoad();
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
                yield return Modded.LevelEditor.LoaderAdds.LoaderLoad();
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
                yield return Modded.LevelEditor.EditorAdds.EditorLoad();
        }

        private IEnumerator BBCRDataLoad()
        {
            yield return 1;
            yield return "Trying to get BBCR Data";
            string path = Path.Combine(Directory.GetParent(Application.persistentDataPath).ToString(), "Baldi's Basics Classic Remastered", "PlayerFile_!UnassignedFile.sav");
            if (File.Exists(path))
            {
                Log.LogInfo("BBCR save file found!");
                Log.LogInfo(path);
                var bbcrdata = JsonUtility.FromJson<PlayerFileData>(RijndaelEncryption.Decrypt(File.ReadAllText(path), "!UnassignedFile"));
                if (bbcrdata.glitchWon) {
                    Log.LogInfo("NULL is defeated! Unlocking new playable character!");
                    unlockedCylnLoon = true;
                }
                else
                    Log.LogInfo("The player sucks balls.");
            }
        }
        private IEnumerator PreLoad()
        {
            yield return 1;
            yield return "Setting up stuff";
            Resources.FindObjectsOfTypeAll<PlayerManager>().First().gameObject.AddComponent<PlrPlayableCharacterVars>();
            assetMan.Add("PlayerPrefab", Resources.FindObjectsOfTypeAll<PlayerManager>().Last());

            var unlockedUI = Instantiate(Resources.FindObjectsOfTypeAll<Gum>().ToList().First().transform.Find("GumOverlay").gameObject, MTM101BaldiDevAPI.prefabTransform).GetComponent<Canvas>();
            unlockedUI.gameObject.name = "UnlockedPlayableUI";
            unlockedUI.gameObject.SetActive(true);
            Destroy(unlockedUI.transform.Find("Image").gameObject);
            var unlockedImageParent = new GameObject("UnlockedImageParent", typeof(RectTransform)).GetComponent<RectTransform>();
            unlockedImageParent.SetParent(unlockedUI.transform);
            unlockedImageParent.anchorMin = new Vector2(1, 0);
            unlockedImageParent.anchorMax = new Vector2(1, 0);
            unlockedImageParent.pivot = new Vector2(1, 0);
            unlockedImageParent.localScale = Vector2.one / 4f;
            unlockedImageParent.sizeDelta = new Vector2(640f, 360f);
            unlockedImageParent.anchoredPosition = new Vector2(-50f, 0f);
            var unlockedTV = new GameObject("TV", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            unlockedTV.SetParent(unlockedImageParent);
            unlockedTV.gameObject.GetComponent<Image>().sprite = Resources.FindObjectsOfTypeAll<Sprite>().Last(spr => spr.name == "ScreenSwingSheet_0");
            unlockedTV.anchorMin = new Vector2(0f, 0.5f);
            unlockedTV.anchorMax = new Vector2(1f, 0.5f);
            unlockedTV.pivot = new Vector2(0.5f, 0.5f);
            unlockedTV.sizeDelta = new Vector2(0f, 360f);
            unlockedTV.localScale = new Vector3(0.6f, -1f, 1f);
            var unlockedText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            unlockedText.rectTransform.SetParent(unlockedImageParent);
            unlockedText.text = "YOU HAVE UNLOCKED\nA NEW PLAYABLE";
            unlockedText.color = Color.white;
            unlockedText.font = BaldiFonts.ComicSans18.FontAsset();
            unlockedText.fontSize = BaldiFonts.ComicSans18.FontSize();
            unlockedText.alignment = TextAlignmentOptions.Center;
            unlockedText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            unlockedText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            unlockedText.rectTransform.pivot = new Vector2(0.5f, 1f);
            unlockedText.rectTransform.anchoredPosition = new Vector2(0f, -100f);
            unlockedText.rectTransform.localScale = Vector3.one;
            var unlockedPortrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            unlockedPortrait.rectTransform.SetParent(unlockedImageParent, false);
            unlockedPortrait.sprite = assetMan.Get<Sprite>("Portrait/Partygoer");
            unlockedPortrait.rectTransform.anchorMin = Vector2.one / 2f;
            unlockedPortrait.rectTransform.anchorMax = Vector2.one / 2f;
            unlockedPortrait.rectTransform.pivot = Vector2.one / 2f;
            unlockedPortrait.rectTransform.localScale = Vector2.one * 1.2f;
            unlockedPortrait.rectTransform.anchoredPosition = new Vector2(0f, -35f);
            var unlockedComponent = unlockedUI.gameObject.AddComponent<PlayableUnlockedUI>();
            unlockedComponent.portraitMan = unlockedPortrait;
            unlockedComponent.textMan = unlockedText;
            unlockedComponent.animatorMan = unlockedUI.gameObject.AddComponent<CustomImageAnimator>();
            unlockedComponent.animatorMan.image = unlockedTV.GetComponent<Image>();
            unlockedComponent.animatorMan.useUnscaledTime = true;
            Sprite[] tvsprites = Resources.FindObjectsOfTypeAll<Sprite>().ToList().FindAll(spr => spr.name.StartsWith("ScreenSwingSheet_")).ToArray();
            for (int i = 0; i < tvsprites.Length; i++)
            {
                if (tvsprites[i].name != $"ScreenSwingSheet_{i}")
                    tvsprites[i] = Resources.FindObjectsOfTypeAll<Sprite>().Last(spr => spr.name == $"ScreenSwingSheet_{i}");
            }
            unlockedComponent.tvsprites = tvsprites;
            assetMan.Add("UnlockedCharacterUI", unlockedComponent);

            yield return "Creating specific character stuff";
            CYLN_LOONComponent.prefabs.AddRange([
                Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Table_Test"),
                Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Chair_Test"),
                //Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Decor_Banana")
                ]);
            var thorwable = new EntityBuilder()
                .AddTrigger(4f)
                .SetBaseRadius(0.5f)
                .SetName("CYLN_Throwable")
                .SetLayer("ClickableEntities")
                .AddRenderbaseFunction((entity) =>
                {
                    var renderbase = new GameObject("RenderBase", typeof(MeshFilter), typeof(MeshRenderer));
                    renderbase.transform.SetParent(entity.transform, false);
                    renderbase.layer = LayerMask.NameToLayer("Ignore Raycast B");
                    renderbase.AddComponent<MeshFilter>();
                    renderbase.AddComponent<MeshRenderer>();
                    entity.gameObject.AddComponent<RendererContainer>().renderers = [renderbase.GetComponent<MeshRenderer>()];
                    return renderbase.transform;
                })
                .Build();
            thorwable.gameObject.AddComponent<ThrowableObject>();
            assetMan.Add("CYLN_Throwable", thorwable);

            NPC_Present.sounds = [assetMan.Get<SoundObject>("Items/PresentOpen1"), assetMan.Get<SoundObject>("Items/PresentOpen2")];
            assetMan.Add<ItemObject>("PresentUnwrapped", new ItemBuilder(Info)
                    .SetItemComponent<ITM_PartygoerPresent>()
                    .SetEnum("PartygoerPresentUnwrapped")
                    .SetNameAndDescription("Itm_PartygoerPresentUnwrapped", "Desc_PartygoerPresentUnwrapped")
                    .SetShopPrice(25)
                    .SetGeneratorCost(5)
                    .SetSprites(assetMan.Get<Sprite>("Items/WrappingBundle_Small"), assetMan.Get<Sprite>("Items/WrappingBundle_Large"))
                    .SetMeta(ItemFlags.Unobtainable, ["gift", "StackableItems_NotAllowStacking"])
                    .Build());
            var present = assetMan.Get<ItemObject>("PresentUnwrapped").item as ITM_PartygoerPresent;
            present.Unwrapped = true;
            var presentmeta = new ItemMetaData(Info, []);
            presentmeta.flags = ItemFlags.Unobtainable;
            presentmeta.tags.AddRange(["gift", "StackableItems_NotAllowStacking", "recchars_gifter_blacklist"]);
            for (int npc = 1; npc <= 13; npc++)
            {
                var goodpresent = new ItemBuilder(Info)
                    .SetItemComponent<ITM_PartygoerPresent>()
                    .SetEnum("PartygoerPresent")
                    .SetNameAndDescription("Itm_PartygoerPresent", "Desc_PartygoerPresent")
                    .SetShopPrice(25)
                    .SetGeneratorCost(5)
                    .SetSprites(assetMan.Get<Sprite>("Items/Present_Small"), assetMan.Get<Sprite>("Items/Present_Large"))
                    .SetMeta(presentmeta)
                    .Build();
                goodpresent.item.GetComponent<ITM_PartygoerPresent>().Gift = (Character)npc;
                assetMan.Add<ItemObject>("PresentGift_" + ((Character)npc).ToStringExtended(), goodpresent);
            }

            #region PRESENTS
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Baldi, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BAL_Apple"));
            NPC_PresentAftermath.actions.Add(global::Character.Baldi, (npc) =>
            {
                Baldi bald = npc as Baldi;
                int chance = UnityEngine.Random.Range(1, 8);
                if (chance >= 7) // Sudden Bomb
                {
                    var animator = bald.gameObject.GetOrAddComponent<CustomSpriteAnimator>();
                    animator.spriteRenderer = bald.spriteRenderer[0];
                    animator.animations.Add("burn", new CustomAnimation<Sprite>([Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_39"), Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_40"), Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_41")], 0.2f));
                    bald.AudMan.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "Fuse"), true);
                    bald.AudMan.SetLoop(true);
                    IEnumerator BombMan()
                    {
                        float time = 1.2f;
                        bald.spriteRenderer[0].sprite = Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiApple_0");
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bald.TimeScale;
                            yield return null;
                        }
                        bald.AudMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BAL_NoPasss"));
                        time = 0.5f;
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bald.TimeScale;
                            yield return null;
                        }
                        bald.AudMan.FlushQueue(true);
                        bald.BreakRuler();
                        bald.AudMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "Explosion"));
                        bald.AudMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BAL_Ohh"));
                        animator.SetDefaultAnimation("burn", 1f);
                        bald.spriteRenderer[0].transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
                        time = 6.2f; // Don't ask...
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bald.TimeScale;
                            yield return null;
                        }
                        Destroy(animator);
                        time = 19f;
                        bald.spriteRenderer[0].transform.localScale = Vector3.one;
                        bald.GetAngry(1f);
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bald.TimeScale;
                            yield return null;
                        }
                        bald.RestoreRuler();
                        yield break;
                    }
                    bald.StartCoroutine(BombMan());
                    return 7.9f;
                }
                else if (chance >= 3 && chance <= 6) // New Ruler
                {

                }
                else // An Apple
                {
                    bald.GetComponent<Animator>().enabled = true;
                    IEnumerator WaitForApple(Baldi itsbald)
                    {
                        while (itsbald.behaviorStateMachine.CurrentState is NPC_PresentAftermath)
                            yield return null;
                        itsbald.TakeApple();
                        yield break;
                    }
                    bald.StartCoroutine(WaitForApple(bald));
                    return 0f;
                }
                return 1f;
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Principal, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "PRI_Scold3"));
            NPC_PresentAftermath.actions.Add(global::Character.Principal, (npc) =>
            {
                Principal pri = npc as Principal;
                PlayerManager player = pri.ReflectionGetVariable("targetedPlayer") as PlayerManager;
                int chance = UnityEngine.Random.Range(-1, 8);
                if (chance >= 3 && player != null && player.plm.Entity.CurrentRoom?.functionObject?.GetComponent<DetentionRoomFunction>() != null) // Free Detention Pass
                {
                    if ((bool)player.plm.Entity.CurrentRoom.functionObject.GetComponent<DetentionRoomFunction>().ReflectionGetVariable("active"))
                    {
                        player.plm.Entity.CurrentRoom.functionObject.GetComponent<DetentionRoomFunction>().Activate(0f, pri.ec);
                        foreach (Door door in player.plm.Entity.CurrentRoom.doors)
                            door.Unlock();
                        return 0f;
                    }
                }
                else if (chance <= 1)
                {
                    float nearest = float.PositiveInfinity;
                    foreach (var plr in pri.ec.Players.Where(player => player != null))
                        if (Vector3.Distance(pri.transform.position, plr.transform.position) < nearest)
                            player = plr;
                    if (player != null)
                    {
                        player.RuleBreak(chance == 1 ? "Lockers" : "Faculty", 99, -5f);
                        pri.ObservePlayer(player);
                    }
                    return 0f;
                }
                return 1f;
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Bully, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BUL_TakeThat"));
            NPC_PresentAftermath.actions.Add(global::Character.Bully, (npc) =>
            {
                Bully bul = npc as Bully;
                if (UnityEngine.Random.Range(1, 8) >= 6) // Sudden Bomb
                {
                    var Audman = bul.GetComponent<AudioManager>();
                    var animator = npc.gameObject.GetOrAddComponent<CustomSpriteAnimator>();
                    animator.spriteRenderer = npc.spriteRenderer[0];
                    animator.animations.Add("burn", new CustomAnimation<Sprite>([Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_39"), Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_40"), Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BaldiPicnic_Sheet_41")], 0.2f));
                    Audman.QueueAudio(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "Fuse"), true);
                    Audman.SetLoop(true);
                    IEnumerator BombMan()
                    {
                        float time = 1.2f;
                        //bul.spriteRenderer[0].sprite = Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "bully_final");
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bul.TimeScale;
                            yield return null;
                        }
                        Audman.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BUL_NoItems"));
                        time = 0.4f;
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bul.TimeScale;
                            yield return null;
                        }
                        Audman.FlushQueue(true);
                        Audman.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "Explosion"));
                        animator.SetDefaultAnimation("burn", 1f);
                        bul.spriteRenderer[0].transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
                        time = 6.3f; // Don't ask...
                        while (time > 0)
                        {
                            time -= Time.deltaTime * bul.TimeScale;
                            yield return null;
                        }
                        Destroy(animator);
                        time = 19f;
                        bul.spriteRenderer[0].transform.localScale = Vector3.one;
                        bul.spriteRenderer[0].sprite = Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "bully_final");
                        bul.Hide();
                        yield break;
                    }
                    bul.StartCoroutine(BombMan());
                    return 7.9f;
                }
                else // Generious
                {
                    bul.Hide();
                    bul.GetComponent<AudioManager>().PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "BUL_Donation"));
                    return 0f;
                }
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Beans, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "Ben_HippityHey"));
            NPC_PresentAftermath.actions.Add(global::Character.Beans, (npc) =>
            {
                Beans ben = npc as Beans;
                return 2f; // How tf Beans has no randomness applied??
            });
            /*NPC_PresentAftermath.actions.Add(global::Character.Crafters, (npc) =>
            {
                ArtsAndCrafters crafters = npc as ArtsAndCrafters;
                return 0f; // Same for Crafters...
            });*/
            NPC_PresentAftermath.actions.Add(global::Character.Sweep, (npc) =>
            {
                return 1f; // Can't do anything with him...
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Pomp, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "NoL_JustinTime"));
            NPC_PresentAftermath.actions.Add(global::Character.Pomp, (npc) =>
            {
                NoLateTeacher pomp = npc as NoLateTeacher;
                var curstate = pomp.behaviorStateMachine.currentState as NPC_PresentAftermath;
                if (curstate.PreviousState is NoLateTeacher_Waiting)
                    pomp.Dismiss();
                pomp.behaviorStateMachine.ChangeState(new NoLateTeacher_Cooldown(pomp, pomp.Cooldown));
                pomp.behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRounds(pomp, 0));
                return 0f;
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.Playtime, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "PT_Laugh"));
            NPC_PresentAftermath.actions.Add(global::Character.Playtime, (npc) =>
            {
                Playtime pt = npc as Playtime;
                if (UnityEngine.Random.Range(1, 6) <= 2) // HOW DARE YOU NOT GIVE HER A NEW JUMPROPE??
                    return 2f;
                pt.GetComponent<AudioManager>().PlaySingle(pt.ReflectionGetVariable("audCongrats") as SoundObject);
                pt.behaviorStateMachine.ChangeState(new Playtime_Cooldown(pt, pt, 25f));
                return 0f;
            });
            ITM_PartygoerPresent.RewardedSound.Add(global::Character.DrReflex, Resources.FindObjectsOfTypeAll<SoundObject>().Last(snd => snd.name == "DrR_GoodJob"));
            NPC_PresentAftermath.actions.Add(global::Character.DrReflex, (npc) =>
            {
                DrReflex reflex = npc as DrReflex;
                var curstate = reflex.behaviorStateMachine.currentState as NPC_PresentAftermath;
                reflex.AudioManager.FlushQueue(true);
                if ((curstate.PreviousState is DrReflex_Testing && !reflex.PlayerLeft(reflex.ec.Players[0])) || curstate.PreviousState is not DrReflex_Testing)
                    reflex.behaviorStateMachine.ChangeState(new DrReflex_Wandering(reflex, 30f));
                if (!reflex.PlayerLeft(reflex.ec.Players[0]))
                    reflex.ReflectionInvoke("WinTest", []);
                else if (curstate.PreviousState is DrReflex_Testing)
                {
                    /*reflex.behaviorStateMachine.ChangeState(new NpcState(reflex));
                    reflex.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(reflex, 127));
                    reflex.EndTest(false, reflex.players[0]);*/
                    return 0f;
                }
                return 4f;
            });
            #endregion

            assetMan.Add<ItemObject>("BackpackClosed", new ItemBuilder(Info)
                    .SetItemComponent<ITM_BackpackerBackpack>()
                    .SetEnum("BackpackerBackpack")
                    .SetNameAndDescription("Itm_BackpackerBackpack", "Desc_BackpackerBackpack")
                    .SetShopPrice(int.MaxValue)
                    .SetGeneratorCost(int.MaxValue)
                    .SetAsNotOverridable()
                    .SetSprites(assetMan.Get<Sprite>("Items/BackpackerBackpack_Small"), assetMan.Get<Sprite>("Items/BackpackerBackpack_Large"))
                    .SetMeta(ItemFlags.MultipleUse | ItemFlags.Unobtainable, ["CharacterItemImportant", "recchars_gifter_blacklist"])
                    .Build());
            assetMan.Add<ItemObject>("BackpackOpen", new ItemBuilder(Info)
                    .SetItemComponent<ITM_BackpackerBackpack>()
                    .SetEnum("BackpackerBackpack")
                    .SetNameAndDescription("Itm_BackpackerBackpack", "Desc_BackpackerBackpack")
                    .SetShopPrice(int.MaxValue)
                    .SetGeneratorCost(int.MaxValue)
                    .SetAsNotOverridable()
                    .SetSprites(assetMan.Get<Sprite>("Items/BackpackerBackpack_SmallOpen"), assetMan.Get<Sprite>("Items/BackpackerBackpack_Large"))
                    .SetMeta(ItemFlags.MultipleUse | ItemFlags.Unobtainable, ["CharacterItemImportant", "recchars_gifter_blacklist"])
                    .Build());
            assetMan.Add<ItemObject>("TinkerneerWrench", new ItemBuilder(Info)
                    .SetItemComponent<ITM_TinkerneerWrench>()
                    .SetEnum("TinkerneerWrench")
                    .SetNameAndDescription("Itm_TinkerneerWrench", "Desc_TinkerneerWrench")
                    .SetShopPrice(3000)
                    .SetGeneratorCost(75)
                    .SetSprites(assetMan.Get<Sprite>("Items/TinkerneerWrench_Small"), assetMan.Get<Sprite>("Items/TinkerneerWrench_Large"))
                    .SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [])
                    .Build());
            var wrench = assetMan.Get<ItemObject>("TinkerneerWrench").item as ITM_TinkerneerWrench;
            wrench.hudPre = Instantiate(Resources.FindObjectsOfTypeAll<Gum>().ToList().First().transform.Find("GumOverlay").gameObject, wrench.transform).GetComponent<Canvas>();
            wrench.hudPre.name = "TinkerInventHUD";
            wrench.hudPre.gameObject.GetComponentInChildren<Image>().sprite = null;
            wrench.hudPre.GetComponentInChildren<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            TextMeshProUGUI text = Instantiate(Resources.FindObjectsOfTypeAll<Jumprope>().ToList().Find(s => s.name == "Jumprope").GetComponentInChildren<TextMeshProUGUI>(), wrench.hudPre.transform, worldPositionStays: true);
            text.color = Color.white;
            text.enableAutoSizing = true;
            text.alignment = TextAlignmentOptions.Center;
            text.gameObject.SetActive(true);
            text.transform.localScale = Vector3.one;
            ITM_TinkerneerWrench.tinkerneeringSnds = [
                assetMan.Get<SoundObject>("Items/Tinkerneering1"),
                assetMan.Get<SoundObject>("Items/Tinkerneering2"),
                assetMan.Get<SoundObject>("Items/Tinkerneering3")
            ];
            #region INVENTIONS
            GameObject testInvention = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testInvention.transform.localScale = Vector3.one*3f;
            //Destroy(testInvention.GetComponent<Collider>());
            testInvention.GetComponent<Collider>().isTrigger = true;
            //Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Table_Test"), Vector3.down * 5f, default(Quaternion), testInvention.transform);
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), testInvention.transform, worldPositionStays: false);
            testInvention.CreateTinkerneeringObject<TestInvention>("Handmade Tape Player", "The most necessary invention,\nyou can make Baldi lose his hearing anytime with this player.", [ItemMetaStorage.Instance.FindByEnum(Items.Tape).value, ItemMetaStorage.Instance.FindByEnum(Items.DoorLock).value, ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value], false);
            testInvention.GetComponent<TestInvention>().render = testInvention.GetComponentInChildren<SpriteRenderer>();
            testInvention.GetComponent<TestInvention>().render.sprite = assetMan.Get<Sprite>("Inventions/TapeQuarterPlayerOpen");
            //testInvention.GetComponent<TestInvention>().render.material = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "Lit_SpriteStandard_Billboard");
            testInvention.GetComponent<TestInvention>().render.transform.localPosition = Vector3.down * 0.5f;
            testInvention.GetComponent<TestInvention>().insertTape = Resources.FindObjectsOfTypeAll<SoundObject>().Last(x => x.name == "TapeInsert");
            GameObject fakestudentInvent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fakestudentInvent.GetComponent<SphereCollider>().isTrigger = true;
            fakestudentInvent.GetComponent<SphereCollider>().radius = 2f;
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), fakestudentInvent.transform, worldPositionStays: false);
            fakestudentInvent.CreateTinkerneeringObject<FakeStudentInvention>("Studentcrow", "Can fake Baldi likely from a far hallway,\nbut not that sturdy enough to make Baldi recognize it as a \"fake\".", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value], false);
            fakestudentInvent.gameObject.layer = 0;
            SpriteRenderer rend = fakestudentInvent.GetComponentInChildren<SpriteRenderer>();
            rend.sprite = assetMan.Get<Sprite>("Inventions/StudentcrowFake");
            rend.transform.localPosition = Vector3.down * 1f;
            GameObject realstudentInvent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            realstudentInvent.GetComponent<SphereCollider>().isTrigger = true;
            realstudentInvent.GetComponent<SphereCollider>().radius = 2f;
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), realstudentInvent.transform, worldPositionStays: false);
            realstudentInvent.CreateTinkerneeringObject<RealStudentInvention>("False Student", "More better than the Studentcrow,\ncan fake Baldi from any directions!", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value]);
            realstudentInvent.gameObject.layer = 0;
            rend = realstudentInvent.GetComponentInChildren<SpriteRenderer>();
            rend.sprite = assetMan.Get<Sprite>("Inventions/StudentcrowReal");
            rend.transform.localPosition = Vector3.down * 1f;
            GameObject realstudentApple = Instantiate(realstudentInvent, MTM101BaldiDevAPI.prefabTransform, false);
            Destroy(realstudentApple.GetComponent<RealStudentInvention>());
            realstudentApple.CreateTinkerneeringObject<RealStudentInvention>("A False Student Containing An Apple", "More better than the Studentcrow,\ncan give Baldi an apple after it got attacked!", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, ItemMetaStorage.Instance.FindByEnum(Items.Apple).value]);
            realstudentApple.gameObject.layer = 0;
            GameObject bonusqGen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bonusqGen.transform.localScale = Vector3.one * 3f;
            bonusqGen.GetComponent<Collider>().isTrigger = true;
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), bonusqGen.transform, worldPositionStays: false);
            bonusqGen.CreateTinkerneeringObject<MathMachineRegen>("Bonus Question Generator", "Makes answered math machines contain a bonus question\nwithout collecting all notebooks!", [ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value, ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value, ItemMetaStorage.Instance.FindByEnum(Items.AlarmClock).value], false, RoomCategory.Class);
            bonusqGen.GetComponent<MathMachineRegen>().render = bonusqGen.GetComponentInChildren<SpriteRenderer>();
            bonusqGen.GetComponent<MathMachineRegen>().render.sprite = assetMan.Get<Sprite>("Inventions/BonusGen");
            bonusqGen.GetComponent<MathMachineRegen>().render.transform.localPosition = Vector3.down * 0.5f;
            #endregion INVENTIONS
            yield return "Adding new characters";
            new PlayableCharacterBuilder<PlayableRandomizer>(Info)
                .SetNameAndDesc("Random Playable", "Desc_RandomPlayable")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Random"))
                .SetFlags(PlayableFlags.None)
                .Build();
            var _default = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info)
                .SetNameAndDesc("The Default", "Desc_Default")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Default"))
                .SetStats(maxslots: 9)
                .Build();
            var predicted = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info)
                .SetNameAndDesc("The Predicted Fanon", "Desc_Predicted")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Fanon"))
                .SetStats(s: 3, w: 10f, r: 16, maxslots: 5)
                .Build();
            var predicted2 = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                .SetNameAndDesc("The Predicted Fanon [Times]", "Desc_PredictedBBT")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/FanonBBT"))
                .SetStats(s: 3, maxslots: 5)
                .Build();
            var glitched = new PlayableCharacterBuilder<CYLN_LOONComponent>(Info, unlockedCylnLoon)
                .SetNameAndDesc("CYLN_LOON", "Desc_LOON")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Cylnloon"))
                .SetStats(s: 2, w: 24f, r: 48f, sd: 25f, sr: 15, sm: 200f, maxslots: 3)
                .SetFlags(PlayableFlags.None)
                .SeparatePrefab()
                .Build();
            glitched.prefab.gameObject.GetComponent<CapsuleCollider>().radius = 1f;
            var partyman = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, false) // Todo: Ability overhaul
                .SetNameAndDesc("The Partygoer", "Desc_Partygoer") // SCRAPPED IDEA: Party Bash\nBy finding the necessary items to host, you can host a party any room! But can alert Baldi after doing so...
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Partygoer"))
                .SetStats(s: 6, w: 19f, r: 26f, sm: 90f, sr: 15f)
                .SetStartingItems(assetMan.Get<ItemObject>("PresentUnwrapped"))
                .Build();
            var bullyman = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, false)
                .SetNameAndDesc("The Troublemaker", "Desc_Troublemaker") // SCRAPPED IDEA: Schemes of Naught\nIncreases the detention timer, allows the player to get past through Its a Bully with no items at all, and BSODA duration is increased.
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Troublemaker"))
                .SetStats(s: 3, r: 28f, sm: 110f, maxslots: 5)
                .SetFlags(PlayableFlags.None)
                .SetStartingItems(ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value)
                //.SeparatePrefab()
                .Build();
            //bullyman.prefab.gameObject.GetComponent<CapsuleCollider>().radius = 2.85f;
            var thinker = new PlayableCharacterBuilder<ThinkerAbility>(Info, false)
                .SetNameAndDesc("The Thinker", "Desc_Thinker")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Thinker"))
                .SetStats(s: 3, w: 25f, r: 25f, sm: 0f, maxslots: 6)
                .SetFlags(PlayableFlags.None)
                .Build();
            var backpacker = new PlayableCharacterBuilder<BackpackerBackpack>(Info, false)
                .SetNameAndDesc("The Backpacker", "Desc_Backpacker")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Backpacker"))
                .SetStats(s: 5, w: 19f, r: 31f, sd: 5f, sr: 15f, sm: 50f)
                .SetFlags(PlayableFlags.ContainsStartingItem)
                // there are 16 slots in total...
                //.SeparatePrefab()
                .Build();
            //backpacker.prefab.gameObject.GetComponent<CapsuleCollider>().radius = 2.5f;
            var tails = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, false)
                .SetNameAndDesc("The Tinkerneer", "Desc_Tinkerneer")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Tinkerneer"))
                .SetStats(s: 6, w: 18f, r: 28f, sd: 18f, sr: 28f, maxslots: 8)
                .SetFlags(PlayableFlags.None)
                .SetStartingItems(assetMan.Get<ItemObject>("TinkerneerWrench"))
                .Build();
            var thetestjr = new PlayableCharacterBuilder<TestSubjectMan>(Info, false)
                .SetNameAndDesc("The Test Subject", "Desc_TestSubject")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/TestSubject"))
                .SetStats(w: 40f, r: 30f, sm: 0f, maxslots: 9)
                .SetFlags(PlayableFlags.None)
                .SeparatePrefab()
                .Build();
            thetestjr.prefab.gameObject.GetComponent<CapsuleCollider>().radius = 1.5f;
            var shitass = new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, false)
                .SetNameAndDesc("The Speedrunner", "Desc_Speedrunner")
                .SetPortrait(assetMan.Get<Sprite>("Portrait/Speedrunner"))
                .SetStats(s: 1, w: 34f, r: 52f, sd: 30f, sr: 10f, sm: 200f, maxslots: 3)
                .SetFlags(PlayableFlags.Abilitiless)
                .Build();
            extraSave = new(_default);
            yield return "Creating select screen";
            /*GameObject screen = Instantiate(FindObjectsOfType<GameObject>(true).ToList().Find(x => x.name == "PickChallenge"));
            screen.ConvertToPrefab(false);
            screen.transform.Find("BackButton").gameObject.SetActive(false);
            screen.transform.Find("Speedy").gameObject.SetActive(false);
            screen.transform.Find("Stealthy").gameObject.SetActive(false);
            screen.transform.Find("Grapple").gameObject.SetActive(false);
            screen.transform.Find("ModeText").GetComponent<TextLocalizer>().enabled = false;
            screen.AddComponent<CharacterSelectScreen>().portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            screen.GetComponent<CharacterSelectScreen>().portrait.rectTransform.SetParent(screen.transform);
            screen.GetComponent<CharacterSelectScreen>().portrait.rectTransform.localPosition = new Vector3(0f, -90f, 0f);
            screen.GetComponent<CharacterSelectScreen>().portrait.rectTransform.localScale = Vector3.one;
            screen.GetComponent<CharacterSelectScreen>().desctext = screen.transform.Find("ModeText").GetComponent<TextMeshProUGUI>();
            screen.GetComponent<CharacterSelectScreen>().desctext.enableAutoSizing = true;
            screen.GetComponent<CharacterSelectScreen>().desctext.rectTransform.localPosition = new Vector3(0f, 135f, 0f);
            screen.GetComponent<CharacterSelectScreen>().nametext = Instantiate(FindObjectsOfType<GameObject>(true).ToList().Find(x => x.name == "PickMode").transform.Find("MainNew"), screen.transform).GetComponent<TextMeshProUGUI>();
            screen.GetComponent<CharacterSelectScreen>().nametext.gameObject.SetActive(true);
            screen.GetComponent<CharacterSelectScreen>().nametext.rectTransform.localPosition = new Vector3(0f, 155f, 0f);
            screen.GetComponent<CharacterSelectScreen>().nametext.enableAutoSizing = true;
            Destroy(screen.GetComponent<CharacterSelectScreen>().nametext.GetComponent<StandardMenuButton>());
            screen.GetComponent<CharacterSelectScreen>().nametext.GetComponent<TextLocalizer>().enabled = false;
            assetMan.Add<GameObject>("CharSelectScreen", screen);*/

            yield return "Doing the rest of contents";
            GeneratorManagement.Register(this, GenerationModType.Base, (title, num, scene) =>
            {
                switch (scene.nameKey)
                {
                    default:
                        scene.GetCustomLevelObjects()?.Do(lvl => lvl.SetCustomModValue(Info, "randomizeralways", false));
                        break;
                    case "Level_EndlessLooped":
                        scene.GetCustomLevelObjects()?.Do(lvl => lvl.SetCustomModValue(Info, "randomizeralways", true));
                        break;
                }
            });
            HashSet<CustomLevelObject> doneforLevels = new HashSet<CustomLevelObject>();
            GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, ld) =>
            {
                ld.shopItems = ld.shopItems.AddToArray(new() { selection = assetMan.Get<ItemObject>("TinkerneerWrench"), weight = 80 });
                if (ld.manager is MainGameManager or EndlessGameManager)
                {
                    var presentt = assetMan.Get<ItemObject>("PresentUnwrapped");
                    foreach (var level in ld.GetCustomLevelObjects())
                    {
                        if (!doneforLevels.Contains(level)) // Endless Mode level objects reuses Hide and Seek Mode level objects
                        {
                            level.forcedItems.AddRange([presentt, presentt]);
                            doneforLevels.Add(level);
                        }
                    }
                }
            });
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors"))
                EndlessFloorsFuncs.ArcadeModeAdd();
            ModdedSaveSystem.AddSaveLoadAction(this, (isSave, path) =>
            {
                PlayableCharsSave filedata;
                PlayableCharsExtraSave extrasave;
                if (File.Exists(Path.Combine(path, "extraPlayablesSaveData.dat")) && !isSave)
                {
                    try
                    {
                        extrasave = JsonUtility.FromJson<PlayableCharsExtraSave>(RijndaelEncryption.Decrypt(File.ReadAllText(Path.Combine(path, "extraPlayablesSaveData.dat")), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                    }
                    catch
                    {
                        extrasave = new PlayableCharsExtraSave();
                        extrasave.selectedChar = _default.name;
                        extrasave.charGUID = _default.info.Metadata.GUID;
                    }
                    PlayableCharacter[] array = playablesMetaStorage.FindAll(x => x.value.name.ToLower().Replace(" ", "") == extrasave.selectedChar.ToLower().Replace(" ", "") && x.value.info.Metadata.GUID == extrasave.charGUID).ToValues();
                    extraSave = new(array.Length != 0 ? array.Last() : _default);
                }
                if (isSave)
                {
                    filedata = new PlayableCharsSave();
                    if (File.Exists(Path.Combine(path, "unlockedChars.dat"))) // Issue occured with Magical Student, addin' dis!
                        filedata = JsonUtility.FromJson<PlayableCharsSave>(RijndaelEncryption.Decrypt(File.ReadAllText(Path.Combine(path, "unlockedChars.dat")), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                    filedata.modversion = new Version(PLUGIN_VERSION);
                    filedata.cyln = glitched.unlocked ? true : unlockedCylnLoon;
                    filedata.partygoer = partyman.unlocked;
                    filedata.troublemaker = bullyman.unlocked;
                    filedata.thinker = thinker.unlocked;
                    filedata.backpacker = backpacker.unlocked;
                    filedata.tinkerneer = tails.unlocked;
                    filedata.testsubject = thetestjr.unlocked;
                    filedata.speedrunner = shitass.unlocked;
                    if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.playablecharacters.modded"))
                    {
                        if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                            filedata.magical = playablesMetaStorage.Find(x => x.value.name == "Magical Student").value.unlocked;
                        if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                        {
                            filedata.protagonist = playablesMetaStorage.Find(x => x.value.name == "The Main Protagonist").value.unlocked;
                            filedata.dweller = playablesMetaStorage.Find(x => x.value.name == "The Dweller").value.unlocked;
                        }
                    }
                    File.WriteAllText(Path.Combine(path, "unlockedChars.dat"), RijndaelEncryption.Encrypt(JsonUtility.ToJson(filedata), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                    extrasave = new PlayableCharsExtraSave();
                    extrasave.selectedChar = extraSave.Item1.name;
                    extrasave.charGUID = extraSave.Item1.info.Metadata.GUID;
                    File.WriteAllText(Path.Combine(path, "extraPlayablesSaveData.dat"), RijndaelEncryption.Encrypt(JsonUtility.ToJson(extrasave), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                }
                else if (File.Exists(Path.Combine(path, "unlockedChars.dat")))
                {
                    bool flag = true;
                    try
                    {
                        filedata = JsonUtility.FromJson<PlayableCharsSave>(RijndaelEncryption.Decrypt(File.ReadAllText(Path.Combine(path, "unlockedChars.dat")), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                    }
                    catch
                    {
                        filedata = new PlayableCharsSave();
                        flag = false;
                    }

                    if (!flag)
                        return;

                    SetUnlockedCharacter(Info, "CYLN_LOON", unlockedCylnLoon ? true : filedata.cyln);
                    SetUnlockedCharacter(Info, "The Partygoer", filedata.partygoer);
                    SetUnlockedCharacter(Info, "The Troublemaker", filedata.troublemaker);
                    SetUnlockedCharacter(Info, "The Thinker", filedata.thinker);
                    SetUnlockedCharacter(Info, "The Backpacker", filedata.backpacker);
                    SetUnlockedCharacter(Info, "The Tinkerneer", filedata.tinkerneer);
                    SetUnlockedCharacter(Info, "The Test Subject", filedata.testsubject);
                    SetUnlockedCharacter(Info, "The Speedrunner", filedata.speedrunner);

                    if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.playablecharacters.modded"))
                    {
                        //BepInEx.PluginInfo info = Chainloader.PluginInfos.GetValueSafe("alexbw145.baldiplus.playablecharacters.modded");
                        // Y'see? I cannot use the plugin from the modded part of this mod because it doesn't contain a save manager action (and also the serialized save data is internal, not public)
                        // so I wouldn't let anyone modify the save data contents.
                        if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                            SetUnlockedCharacter(Info, "Magical Student", filedata.magical);
                        if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                        {
                            SetUnlockedCharacter(Info, "The Main Protagonist", filedata.protagonist);
                            SetUnlockedCharacter(Info, "The Dweller", filedata.dweller);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Unlocks a playable character
        /// </summary>
        /// <param name="info">The plugin that contains the character</param>
        /// <param name="charName">The name of the playable to unlock (not case sensitive)</param>
        public static void UnlockCharacter(BepInEx.PluginInfo info, string charName)
        {
            if (!MTM101BaldiDevAPI.SaveGamesEnabled || (CoreGameManager.Instance != null && !CoreGameManager.Instance.SaveEnabled)
                || PlayableCharacterMetaStorage.Instance.Find(x => x.value.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info) == null
                || PlayableCharacterMetaStorage.Instance.Find(x => x.value.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info).value.unlocked) return;
            if (SceneManager.GetActiveScene().name != "MainMenu")
                MusicManager.Instance.PlaySoundEffect(NewCharacterUnlocked);
            var characterToUnlock = PlayableCharacterMetaStorage.Instance.Find(x => x.value.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info);
            var unlockMan = Instantiate(PlayableUnlockedUI, null, false);
            DontDestroyOnLoad(unlockMan);
            unlockMan.portraitMan.sprite = characterToUnlock.value.sprselect;
            unlockMan.DoThang();
            characterToUnlock.value.unlocked = true;
            ModdedSaveSystem.CallSaveLoadAction(info.Instance, true, ModdedSaveSystem.GetCurrentSaveFolder(info.Instance));
        }

        internal static void TestUnlockUI()
        {
            MusicManager.Instance.PlaySoundEffect(NewCharacterUnlocked);
            var unlockMan = Instantiate(PlayableUnlockedUI, null, false);
            DontDestroyOnLoad(unlockMan);
            var randomguys = PlayableCharacterMetaStorage.Instance.All();
            unlockMan.portraitMan.sprite = randomguys[UnityEngine.Random.RandomRangeInt(0, randomguys.Length - 1)].value.sprselect;
            unlockMan.DoThang();
        }

        /// <summary>
        /// Set the character's unlocked status without calling the save action.
        /// </summary>
        /// <param name="info">The plugin that contains the character</param> <param name="charName">The character's name. (Not case sensitive)</param> <param name="unlocked">Set if the character is unlocked.</param>

        public static void SetUnlockedCharacter(BepInEx.PluginInfo info, string charName, bool unlocked) // I see you cheaters! You can't really unlock CYLN_LOON manually!
        {
            if (PlayableCharacterMetaStorage.Instance.Find(x => x.value.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info) != null)
                PlayableCharacterMetaStorage.Instance.Find(x => x.value.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info).value.unlocked = unlocked;
        }
    }

    public static class ConstructionStuff
    {
        internal static void CreateTinkerneeringObject<T>(this GameObject gobj, string name, string desc, ItemObject[] acceptables, bool tinkerneerCharExclusive = true, RoomCategory rmcategory = RoomCategory.Null) where T : TinkerneerObject
        => gobj.CreateTinkerneeringObject<T>(PlayableCharsPlugin.Instance.Info, name, desc, acceptables, tinkerneerCharExclusive, rmcategory);

        /// <summary>
        /// Converts and makes the prefab an Tinkerneering Object
        /// </summary>
        /// <typeparam name="T">The tinkerneer component to use.</typeparam>
        /// <param name="gobj">The game object of the tinkerneer component.</param> <param name="info">The plugin that you want to store for the object</param>
        /// <param name="name"></param> <param name="desc"></param> <param name="acceptables">The items required to invent and place a tinkereer component.</param>
        /// <param name="tinkerneerCharExclusive">Is this component exclusively craftable from The Tinkerneer character?</param>
        /// <param name="rmcategory">What category can this room be placed in. (Leave as <see cref="RoomCategory.Null"/> to make this component craftable anywhere.)</param>
        public static void CreateTinkerneeringObject<T>(this GameObject gobj, BepInEx.PluginInfo info,  string name, string desc, ItemObject[] acceptables, bool tinkerneerCharExclusive = true, RoomCategory rmcategory = RoomCategory.Null) where T : TinkerneerObject
        {
            TinkerneerObject thing = gobj.AddComponent<T>();
            thing.gameObject.name = name;
            thing.desc = desc;
            thing.requiredItems = acceptables;
            thing.tinkerneerCharExclusive = tinkerneerCharExclusive;
            thing.rm = rmcategory;
            thing.Info = info;
            gobj.ConvertToPrefab(true);
            ITM_TinkerneerWrench.TinkerneerObjectsPre.Add(thing.name, thing);
        }

        /// <summary>
        /// Gets the metadata from the playable character
        /// </summary>
        /// <param name="me">The playable character scriptable object</param>
        /// <returns>The metadata from the playable character</returns>
        public static PlayableCharacterMetaData GetMeta(this PlayableCharacter me) => PlayableCharsPlugin.playablesMetaStorage.Get(me);
        /// <summary>
        /// Grabs the player's playable variables
        /// </summary>
        /// <param name="me">The player themself</param>
        /// <returns>The component that contains playable variables</returns>
        public static PlrPlayableCharacterVars GetPlayable(this PlayerManager me) => PlrPlayableCharacterVars.PlayerPlayables[me.playerNumber];
    }

    /// <summary>
    /// The playable character container
    /// </summary>
    public class PlayableCharacter : ScriptableObject
    {
        public BepInEx.PluginInfo info { get; private set; }
        /// <summary>
        /// The name of the character
        /// </summary>
        public new string name { get; private set; }
        /// <summary>
        /// The description of the character
        /// </summary>
        public string description { get; private set; }
        public Sprite sprselect { get; private set; }
        public PlayableFlags flags { get; private set; }
        //public string[] tags { get; private set; } = new string[0];

        /// <summary>
        /// The speed of the character
        /// </summary>
        public float walkSpeed = 16f;

        /// <summary>
        /// The speed of the character while running
        /// </summary>
        public float runSpeed = 24f;

        /// <summary>
        /// The amount of depletion while running
        /// </summary>
        public float staminaDrop = 10f;

        /// <summary>
        /// The amount of replenishing while resting
        /// </summary>
        public float staminaRise = 20f;

        /// <summary>
        /// The max stamina of the player
        /// </summary>
        public float staminaMax = 100f;

        /// <summary>
        /// Amount of max item slots that the character can contain
        /// </summary>
        public int slots = 5;

        /// <summary>
        /// Is the character unlocked?
        /// </summary>
        public bool unlocked { get; internal set; } = false;

        /// <summary>
        /// The starting items that the character can contain
        /// </summary>
        public ItemObject[] startingItems = new ItemObject[0];

        public PlayerManager prefab { get; internal set; }

        public Type componentType = typeof(PlayableCharacterComponent);

        public PlayableCharacter(BepInEx.PluginInfo Info, string n, string d, Sprite container, PlayableFlags f, bool unlockedfromStart = false)
        {
            info = Info;
            name = n;
            base.name = n;
            description = d;
            sprselect = container;
            flags = f;
            if (unlockedfromStart)
                flags |= PlayableFlags.UnlockedFromStart;
            else
                flags &= ~PlayableFlags.UnlockedFromStart;
            unlocked = unlockedfromStart;
        }

        public PlayableCharacter(BepInEx.PluginInfo Info, string n, string d, Sprite container, PlayableFlags f, Action<PlayerManager, bool> action, bool unlockedfromStart = false)
            : this (Info, n, d, container, f, unlockedfromStart)
        {
            OnInitAction = action;
        }

        //public void SetTags(string[] tagss) => tags = tagss;
        //public void AddTags(string[] tagss) => tags = tags.AddRangeToArray(tagss);

        // Arcade Mode stuff, be on the lookout for BBP new major update before I finish the entire mod!
        internal Action<PlayerManager, bool> OnInitAction { get; private set; }
        /// <summary>
        /// Amount of max item slots that the character can reach with the Item Slot+ upgrade
        /// </summary>
        [Range(0, 9)] public int maxSlots;
        //
    }

    [Flags]
    public enum PlayableFlags
    {
        /// <summary>
        /// This character has no necessary flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// This character is unlocked from the start (CYLN_LOON does not count towards this as its unlock method is from BBCR)
        /// NOTE: This will be added automatically if the boolean `unlocked` is set to `true` during creation
        /// </summary>
        UnlockedFromStart = 1,
        /// <summary>
        /// // This character contains a starting item.
        /// </summary>
        ContainsStartingItem = 2,
        /// <summary>
        /// This character is part of the school faculty (NOT YET IMPLEMENTED)
        /// </summary>
        Guiltiless = 4,
        /// <summary>
        /// This character does not contain one or more abilities
        /// </summary>
        Abilitiless = 8,
    }

    [RequireComponent(typeof(PlayerManager), typeof(ItemManager), typeof(PlayerMovement))]
    public class PlayableCharacterComponent : MonoBehaviour
    {
        protected PlayerManager pm;
        /// <summary>
        /// This function gets called after initialization
        /// </summary>
        protected virtual void Start()
        {
        }
        /// <summary>
        /// This function gets called within the player manager initialization
        /// </summary>
        public virtual void Initialize() { pm = gameObject.GetComponent<PlayerManager>(); }
        /// <summary>
        /// This function gets called after the game has begun
        /// </summary>
        /// <param name="manager"></param>
        public virtual void GameBegin(BaseGameManager manager) { }
        /// <summary>
        /// This function gets called after spoop mode begins
        /// </summary>
        /// <param name="manager"></param>
        public virtual void SpoopBegin(BaseGameManager manager) { }
    }
    internal class PlayableRandomizer : PlayableCharacterComponent
    {
        public static void RandomizePlayable()
        {
            PlayableCharsPlugin.gameStarted = false;
            var chars = PlayableCharacterMetaStorage.Instance.FindAll(chara => chara.value.unlocked && chara.value.componentType != typeof(PlayableRandomizer));
            WeightedSelection<PlayableCharacter>[] weights = new WeightedSelection<PlayableCharacter>[0];
            foreach (var character in chars)
                weights = weights.AddToArray(new() { selection = character.value, weight = 100});
            PlayableCharsGame.Character = WeightedSelection<PlayableCharacter>.RandomSelection(weights);
#if DEBUG
            PlayableCharsPlugin.Log.LogInfo($"Randomly selected {PlayableCharsGame.Character.name} for Player!");
#endif
        }
    }
    // Took this from the API
    public class PlayableCharacterBuilder<T> where T : PlayableCharacterComponent
    {
        private BepInEx.PluginInfo info;
        private string name = "Unknown";
        private string desc = "Who?";
        private Sprite portrait = PlayableCharsPlugin.assetMan["Portrait/Placeholder"] as Sprite;
        private PlayableFlags flags = PlayableFlags.Abilitiless;
        private bool unlockedFromStart = true;
        private PlayerManager prefab = PlayableCharsPlugin.assetMan["PlayerPrefab"] as PlayerManager;
        private bool prefabSeparate = false;
        private string[] tags = new string[0];
        private ItemObject[] startingItems = new ItemObject[0];
        private float walkSpeed = 16f;
        private float runSpeed = 24f;
        private float staminaDrop = 10f;
        private float staminaRise = 20f;
        private float staminaMax = 100f;
        private int slots = 5;
        private Action<PlayerManager, bool> action;
        private int maxSlots = 5;

        public PlayableCharacterBuilder(BepInEx.PluginInfo Info, bool unlocked = true)
        {
            info = Info;
            unlockedFromStart = unlocked;
        }

        public PlayableCharacter Build()
        {
            PlayableCharacter character = new PlayableCharacter(info, name, desc, portrait, flags, action, unlockedFromStart);
            character.walkSpeed = walkSpeed;
            character.runSpeed = runSpeed;
            character.staminaDrop = staminaDrop;
            character.staminaRise = staminaRise;
            character.staminaMax = staminaMax;
            character.slots = slots;
            character.maxSlots = maxSlots;
            character.startingItems = startingItems;
            character.prefab = prefabSeparate ? GameObject.Instantiate(prefab, MTM101BaldiDevAPI.prefabTransform) : prefab;
            if (prefabSeparate)
                character.prefab.gameObject.name = character.prefab.gameObject.name.Replace("(Clone)", $"_{name.Replace(" ", "")}");
            character.componentType = typeof(T);
            PlayableCharacterMetaStorage.Instance.Add(new PlayableCharacterMetaData(info, character, tags));
            return character;
        }

        public PlayableCharacterBuilder<T> SetNameAndDesc(string n, string d)
        {
            name = n;
            desc = d;
            return this;
        }

        public PlayableCharacterBuilder<T> SetPortrait(Sprite p)
        {
            portrait = p;
            return this;
        }

        /// <summary>
        /// Separates the PlayerManager prefab by duplicating the prefab itself. Useful for adjusting player hitboxes.
        /// </summary>
        /// <returns></returns>
        public PlayableCharacterBuilder<T> SeparatePrefab()
        {
            prefabSeparate = true;
            return this;
        }

        public PlayableCharacterBuilder<T> SetStartingItems(params ItemObject[] i)
        {
            if (i.Length <= 0) return this; // Who would do such thing to insert an empty item object array?
            startingItems = i;
            flags |= PlayableFlags.ContainsStartingItem;
            return this;
        }

        public PlayableCharacterBuilder<T> SetFlags(PlayableFlags f)
        {
            flags = f;
            if (startingItems.Length > 0)
                flags |= PlayableFlags.ContainsStartingItem;
            return this;
        }

        public PlayableCharacterBuilder<T> SetTags(params string[] t)
        {
            tags = t;
            return this;
        }

        /// <summary>
        /// Sets the stats for the playable character
        /// </summary>
        /// <param name="w">Walk speed</param>
        /// <param name="r">Run speed</param>
        /// <param name="sd">Amount of stamina depletion</param>
        /// <param name="sr">Amount of stamina regeneration</param>
        /// <param name="sm">Max amounts of stamina</param>
        /// <param name="s">Number of item slots</param>
        /// <returns></returns>
        public PlayableCharacterBuilder<T> SetStats(float w = 16f, float r = 24f, float sd = 10f, float sr = 20f, float sm = 100f, int s = 5)
        {
            walkSpeed = w;
            runSpeed = r;
            staminaDrop = sd;
            staminaRise = sr;
            staminaMax = sm;
            slots = s;
            maxSlots = s;
            return this;
        }

        /// <summary>
        /// Sets the stats for the playable character
        /// </summary>
        /// <param name="w">Walk speed</param>
        /// <param name="r">Run speed</param>
        /// <param name="sd">Amount of stamina depletion</param>
        /// <param name="sr">Amount of stamina regeneration</param>
        /// <param name="sm">Max amounts of stamina</param>
        /// <param name="s">Number of item slots</param>
        /// <param name="maxslots">Maxium amount of item slots that can only be reached with the Item Slot+ upgrade</param>
        /// <returns></returns>
        public PlayableCharacterBuilder<T> SetStats(int maxslots, float w = 16f, float r = 24f, float sd = 10f, float sr = 20f, float sm = 100f, int s = 5)
        {
            SetStats(w, r, sd, sr, sm, s);
            maxSlots = maxslots < 0 ? s : maxslots;
            return this;
        }

        /// <summary>
        /// Sets the action that gets invoked during game initialization
        /// </summary>
        /// <param name="Action">The function that gets invoked during game initialization (<see cref="bool"/> will be set to true if Endless Floors mode is active)</param>
        /// <returns></returns>
        public PlayableCharacterBuilder<T> SetInitializationAction(Action<PlayerManager, bool> Action)
        {
            action = Action;
            return this;
        }
    }
    // Also took this from the API
    public class PlayableCharacterMetaData : IMetadata<PlayableCharacter>
    {
        public PlayableFlags flags => value != null ? value.flags : PlayableFlags.None;

        public string nameLocalizationKey { get; private set; }

        public PlayerManager prefab => value?.prefab;

        public PlayableCharacter value { get; private set; }

        public List<string> tags { get; private set; } = new List<string>();

        public BepInEx.PluginInfo info { get; private set; }

        public PlayableCharacterMetaData(BepInEx.PluginInfo info, PlayableCharacter character)
        {
            this.info = info;

            value = character;
            nameLocalizationKey = character.name;
        }

        public PlayableCharacterMetaData(BepInEx.PluginInfo info, PlayableCharacter character, string[] tags)
            : this(info, character)
        {
            this.tags.AddRange(tags);
        }
    }
    // Also also took this from the API
    public class PlayableCharacterMetaStorage : BasicMetaStorage<PlayableCharacterMetaData, PlayableCharacter>
    {
        public static PlayableCharacterMetaStorage Instance => PlayableCharsPlugin.playablesMetaStorage;
    }

    [Serializable]
    internal class PlayableCharsSave
    {
        public Version modversion;

        [Header("Base Game")]
        public bool cyln;
        public bool partygoer;
        public bool troublemaker;
        public bool thinker;
        public bool backpacker;
        public bool tinkerneer;
        public bool testsubject;
        public bool speedrunner;

        [Header("Modded")]
        public bool protagonist;
        public bool dweller;
        public bool magical;


    }

    [Serializable]
    internal class PlayableCharsExtraSave
    {
        public string selectedChar, charGUID;
    }

    internal class PlayableCharsGame : ModdedSaveGameIOBinary
    {
        public override BepInEx.PluginInfo pluginInfo => PlayableCharsPlugin.Instance.Info;
        public static PlayableCharacter Character;
        public static int prevSlots;
        public static ItemObject[] backpackerBackup = new ItemObject[9];

        public override void Reset()
        {
            Character = PlayableCharsPlugin.Instance.extraSave.Item1;
            prevSlots = 5;
            backpackerBackup = new ItemObject[9];
            var nothing = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
            for (int i = 0; i < backpackerBackup.Length; i++)
                backpackerBackup[i] = nothing;
        }

        public override void Save(BinaryWriter writer)
        {
            var data = new PlayableCharsGameJSON();
            data.character = new(Character);
            data.prevSlots = prevSlots;
            for (int i = 0; i < backpackerBackup.Length; ++i)
                data.backpackerBackup[i] = new(backpackerBackup[i]);
            writer.Write(RijndaelEncryption.Encrypt(JsonUtility.ToJson(data), "PLAYABLECHARSGAME_" + PlayerFileManager.Instance.fileName));
            data.character.Write(writer);
            for (int i = 0; i < backpackerBackup.Length; ++i)
                data.backpackerBackup[i].Write(writer);
        }

        public override void Load(BinaryReader reader)
        {
            var data = JsonUtility.FromJson<PlayableCharsGameJSON>(RijndaelEncryption.Decrypt(reader.ReadString(), "PLAYABLECHARSGAME_" + PlayerFileManager.Instance.fileName));
            Character = PlayableCharacterIdentifier.Read(reader).LocateObject();
            prevSlots = data.prevSlots;
            for (int i = 0; i < data.backpackerBackup.Length; ++i)
                backpackerBackup[i] = ModdedItemIdentifier.Read(reader).LocateObject();

        }

        public override void OnCGMCreated(CoreGameManager instance, bool isFromSavedGame)
        {
            if (!isFromSavedGame)
                CharacterSelector.Instance?.SetValues();
        }

        /*public override string DisplayTags(string[] tags)
        {
            string with = "";
            if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.playablecharacters.modded"))
            {
                with = " w/ ";
                if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                    with += "Baldi's Basics Times" + (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent") ? " & " : "");
                if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                    with += "B. Carnell's Plus Pack";
            }
            return "Standard Mode" + with;
        }*/
    }

    [Serializable]
    internal class PlayableCharsGameJSON
    {
        public PlayableCharacterIdentifier character;
        public int prevSlots;
        public ModdedItemIdentifier[] backpackerBackup = new ModdedItemIdentifier[9];
    }

    [Serializable]
    public struct PlayableCharacterIdentifier // THIS MISTAKE, THIS BIG MISTAKE MADE ME MESS UP VERY BAD!! SO I HAD TO CREATE THIS!
    {
        public byte version;

        public string PluginGUID;

        public string playablecharName;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(PluginGUID);
            writer.Write(playablecharName);
        }

        public PlayableCharacter LocateObject()
        {
            PlayableCharacterIdentifier thiz = this;
            PlayableCharacter[] array = PlayableCharacterMetaStorage.Instance.FindAll(x => x.value.info.Metadata.GUID == thiz.PluginGUID && x.value.name.ToLower().Replace(" ", "") == thiz.playablecharName.ToLower().Replace(" ", "")).ToValues();
            if (array.Length != 0) return array.Last();

            return null;
        }

        public PlayableCharacterIdentifier(PlayableCharacter objct)
        {
            PlayableCharacter chara = PlayableCharacterMetaStorage.Instance.Find(x => x.value == objct).value;
            if (chara == null) throw new NullReferenceException("Playable character: " + objct.name + " does not exist! Can't create PlayableCharacterIdentifier!");
            version = 0;
            PluginGUID = chara.info.Metadata.GUID;
            playablecharName = objct.name;
        }

        public static PlayableCharacterIdentifier Read(BinaryReader reader)
        {
            PlayableCharacterIdentifier result = default(PlayableCharacterIdentifier);
            result.version = reader.ReadByte();
            result.PluginGUID = reader.ReadString();
            result.playablecharName = reader.ReadString();
            return result;
        }
    }
}

[Serializable]
internal class PlayerFileData
{
    public int saveVersion;
    public AchievementData achievementData;
    public int gamesSinceError;
    public int errorCount;
    public bool classicWon;
    public bool partyWon;
    public bool demoWon;
    public bool glitchWon;
    public bool bossSeen;
    public bool[] flags = new bool[6];
    public bool reduceFlashing;
}

