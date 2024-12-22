using BaldiEndless;
using BBP_Playables.Core.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using MidiPlayerTK;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using TMPro;
using UnityCipher;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace BBP_Playables.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", "5.0.0.0")]
    [BepInDependency("mtm101.rulerp.baldiplus.endlessfloors", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("BALDI.exe")]
    public class PlayableCharsPlugin : BaseUnityPlugin
    {
        public static PlayableCharsPlugin Instance { get; private set; }
        internal static bool gameStarted = false;

        public static AssetManager assetMan = new AssetManager();
        public static List<PlayableCharacter> characters = new List<PlayableCharacter>();
        internal bool unlockedCylnLoon { get; private set; } = false;
        public PlayableCharacter Character => PlayableCharsGame.Character;
        internal PlayableCharsGame gameSave = new PlayableCharsGame();

        // TODO:
        /*
         * CYLN_LOON is done
         * Partygoer isn't done
         * Troublemaker isn't done
         * Thinker is done
         * Backpacker is done
         * Tinkerneer is very not done
         * Test Subject is done
         * Speedrunner is done
         */
        private void Awake()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            Instance = this;
            harmony.PatchAllConditionals();
            LoadingEvents.RegisterOnLoadingScreenStart(Info, BBCRDataLoad());
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), false);
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

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "BackpackerBackpack_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "BackpackerBackpack_Small.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "BackpackerBackpack_SmallOpen.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "TinkerneerWrench_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "TinkerneerWrench_Small.png"),

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

                "Items/BackpackerBackpack_Large",
                "Items/BackpackerBackpack_Small",
                "Items/BackpackerBackpack_SmallOpen",
                "Items/TinkerneerWrench_Large",
                "Items/TinkerneerWrench_Small",

                "Inventions/TapeQuarterPlayerOpen",
                "Inventions/TapeQuarterPlayerClosed",
                "Inventions/StudentcrowFake",
                "Inventions/StudentcrowReal",
                "Inventions/BonusGen",
                "Inventions/BonusGenActive",
            ]);
            assetMan.AddRange<SoundObject>([
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering1.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering2.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "Tinkerneering3.wav"), "Sfx_TinkerneerConstruct", SoundType.Effect, Color.white)
            ],
            [
                "Items/Tinkerneering1",
                "Items/Tinkerneering2",
                "Items/Tinkerneering3",
            ]);
#if DEBUG
            assetMan.AddRange<string>([
                AssetLoader.MidiFromMod("mus_charSel", this, "mus_charSel.mid"),
                AssetLoader.MidiFromMod("mus_charSelINF", this, "mus_charSelINF.mid")
            ],
            [
                "charSel",
                "charSelINF"
            ]);
#endif
        }

        IEnumerator BBCRDataLoad()
        {
            yield return 1;
            yield return "Trying to get BBCR Data";
            string path = Path.Combine(Directory.GetParent(Application.persistentDataPath).ToString(), "Baldi's Basics Classic Remastered", "PlayerFile_!UnassignedFile.sav");
            if (File.Exists(path))
            {
                Debug.Log("BBCR save file found!");
                Debug.Log(path);
                var bbcrdata = JsonUtility.FromJson<BBCRSaveRef>(RijndaelEncryption.Decrypt(File.ReadAllText(path), "!UnassignedFile"));
                if (bbcrdata.glitchWon) {
                    Debug.Log("NULL is defeated! Unlocking new playable character!");
                    unlockedCylnLoon = true;
                }
                else
                    Debug.Log("The player sucks balls.");
            }
        }
        IEnumerator PreLoad()
        {
            yield return 1;            
            yield return "Creating specific character stuff";
            NullObjectThrowableSpawn.prefabs.AddRange([
                Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Table_Test"),
                Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Chair_Test"),
                //Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "Decor_Banana")
                ]);
            assetMan.Add<ItemObject>("BackpackClosed", new ItemBuilder(Info)
                    .SetItemComponent<ITM_BackpackerBackpack>()
                    .SetEnum("BackpackerBackpack")
                    .SetNameAndDescription("Itm_BackpackerBackpack", "Desc_BackpackerBackpack")
                    .SetShopPrice(int.MaxValue)
                    .SetGeneratorCost(int.MaxValue)
                    .SetSprites(assetMan.Get<Sprite>("Items/BackpackerBackpack_Small"), assetMan.Get<Sprite>("Items/BackpackerBackpack_Large"))
                    .SetMeta(ItemFlags.MultipleUse, ["CharacterItemImportant"])
                    .Build());
            assetMan.Add<ItemObject>("BackpackOpen", new ItemBuilder(Info)
                    .SetItemComponent<ITM_BackpackerBackpack>()
                    .SetEnum("BackpackerBackpack")
                    .SetNameAndDescription("Itm_BackpackerBackpack", "Desc_BackpackerBackpack")
                    .SetShopPrice(int.MaxValue)
                    .SetGeneratorCost(int.MaxValue)
                    .SetSprites(assetMan.Get<Sprite>("Items/BackpackerBackpack_SmallOpen"), assetMan.Get<Sprite>("Items/BackpackerBackpack_Large"))
                    .SetMeta(ItemFlags.MultipleUse, ["CharacterItemImportant"])
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
            GameObject fakestudentInvent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fakestudentInvent.GetComponent<SphereCollider>().isTrigger = true;
            fakestudentInvent.GetComponent<SphereCollider>().radius = 2f;
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), fakestudentInvent.transform, worldPositionStays: false);
            fakestudentInvent.CreateTinkerneeringObject<FakeStudentInvention>("Studentcrow", "Can fake Baldi likely from a far hallway,\nbut not that sturdy enough to make Baldi recognize it as a \"fake\".", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value], false);
            fakestudentInvent.gameObject.tag = "Player";
            SpriteRenderer rend = fakestudentInvent.GetComponentInChildren<SpriteRenderer>();
            rend.sprite = assetMan.Get<Sprite>("Inventions/StudentcrowFake");
            rend.transform.localPosition = Vector3.down * 1f;
            GameObject realstudentInvent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            realstudentInvent.GetComponent<SphereCollider>().isTrigger = true;
            realstudentInvent.GetComponent<SphereCollider>().radius = 2f;
            Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TapePlayer").transform.GetComponentInChildren<SpriteRenderer>(), realstudentInvent.transform, worldPositionStays: false);
            realstudentInvent.CreateTinkerneeringObject<RealStudentInvention>("False Student", "More better than the Studentcrow,\ncan fake Baldi from any directions!", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value]);
            realstudentInvent.gameObject.tag = "Player";
            rend = realstudentInvent.GetComponentInChildren<SpriteRenderer>();
            rend.sprite = assetMan.Get<Sprite>("Inventions/StudentcrowReal");
            rend.transform.localPosition = Vector3.down * 1f;
            GameObject realstudentApple = Instantiate(realstudentInvent, null, false);
            Destroy(realstudentApple.GetComponent<RealStudentInvention>());
            realstudentApple.CreateTinkerneeringObject<RealStudentInvention>("A False Student Containing An Apple", "More better than the Studentcrow,\ncan give Baldi an apple after it got attacked!", [ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value, ItemMetaStorage.Instance.FindByEnum(Items.Scissors).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, ItemMetaStorage.Instance.FindByEnum(Items.Apple).value]);
            realstudentApple.gameObject.tag = "Player";
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
            characters.AddRange([
            new PlayableCharacter(Info, "The Default",
                "Desc_Default",
                assetMan.Get<Sprite>("Portrait/Default"), PlayableFlags.Abilitiless, true),
            new PlayableCharacter(Info, "The Predicted Fanon",
                "Desc_Predicted",
                assetMan.Get<Sprite>("Portrait/Fanon"), PlayableFlags.Abilitiless, true)
            {
                slots = 3,
                walkSpeed = 10f,
                runSpeed = 16f,
                staminaDrop = 5f
            },
            new PlayableCharacter(Info, "The Predicted Fanon [Times]",
                "Desc_PredictedBBT",
                assetMan.Get<Sprite>("Portrait/FanonBBT"), PlayableFlags.Abilitiless, Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
            {
                slots = 3
            },
            new PlayableCharacter(Info, "CYLN_LOON",
                "Desc_LOON",
                assetMan.Get<Sprite>("Portrait/Cylnloon"), PlayableFlags.None, unlockedCylnLoon)
            {
                slots = 2,
                walkSpeed = 24f,
                runSpeed = 48f,
                staminaDrop = 25f,
                staminaRise = 15f,
                staminaMax = 200f,
            },
            new PlayableCharacter(Info, "The Partygoer",
                "Desc_Partygoer",
                assetMan.Get<Sprite>("Portrait/Partygoer"), PlayableFlags.ContainsStartingItem | PlayableFlags.Abilitiless) // Party Bash\nBy finding the necessary items to host, you can host a party any room! But can alert Baldi after doing so...
            {
                slots = 6,
                walkSpeed = 19f,
                runSpeed = 26f,
                staminaMax = 90f,
                staminaRise = 15f,
                startingItems = [ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value, ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value]
            },
                new PlayableCharacter(Info, "The Troublemaker",
                "Desc_Troublemaker",
                assetMan.Get<Sprite>("Portrait/Placeholder"), PlayableFlags.ContainsStartingItem | PlayableFlags.Abilitiless) //Schemes of Naught\nIncreases the detention timer, allows the player to get past through Its a Bully with no items at all, and BSODA duration is increased.
            {
                slots = 3,
                walkSpeed = 10f,
                runSpeed = 20f,
                staminaMax = 110f,
                startingItems = [ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value]
            },
            new PlayableCharacter(Info, "The Thinker",
                "Desc_Thinker",
                assetMan.Get<Sprite>("Portrait/Thinker"), PlayableFlags.None)
            {
                slots = 3,
                walkSpeed = 25f,
                runSpeed = 25f,
                staminaMax = 0f,
            },
            new PlayableCharacter(Info, "The Backpacker",
                "Desc_Backpacker",
                assetMan.Get<Sprite>("Portrait/Placeholder"), PlayableFlags.ContainsStartingItem)
            {
                // there are 16 slots in total...
                slots = 9,
                walkSpeed = 19f,
                runSpeed = 31f,
                staminaMax = 50f,
                staminaDrop = 5f,
                staminaRise = 15f,
            },
            new PlayableCharacter(Info, "The Tinkerneer",
                "Desc_Tinkerneer",
                assetMan.Get<Sprite>("Portrait/Placeholder"), PlayableFlags.ContainsStartingItem)
            {
                slots = 9,
                walkSpeed = 18f,
                runSpeed = 28f,
                staminaDrop = 18f,
                staminaRise = 28f,
                startingItems = [assetMan.Get<ItemObject>("TinkerneerWrench")]
            },
            new PlayableCharacter(Info, "The Test Subject",
                "Desc_TestSubject",
                assetMan.Get<Sprite>("Portrait/TestSubject"), PlayableFlags.None)
            {
                walkSpeed = 40f,
                runSpeed = 30f,
                staminaMax = 0f,
            },
            new PlayableCharacter(Info, "The Speedrunner",
                "Desc_Speedrunner",
                assetMan.Get<Sprite>("Portrait/Placeholder"), PlayableFlags.Abilitiless)
            {
                slots = 1,
                walkSpeed = 34f,
                runSpeed = 52f,
                staminaDrop = 30f,
                staminaRise = 10f,
                staminaMax = 200,
            },
            ]);
            yield return "Creating select screen";
            GameObject screen = Instantiate(FindObjectsOfType<GameObject>(true).ToList().Find(x => x.name == "PickChallenge"));
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
            assetMan.Add<GameObject>("CharSelectScreen", screen);

            yield return "Doing the rest of contents";
            Resources.FindObjectsOfTypeAll<PlayerManager>().First().gameObject.AddComponent<PlrPlayableCharacterVars>();
            GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, ld) =>
            {
                ld.shopItems = ld.shopItems.AddToArray(new() { selection = assetMan.Get<ItemObject>("TinkerneerWrench"), weight = 80 });
            });
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors"))
                EndlessFloorsFuncs.ArcadeModeAdd();
            ModdedSaveSystem.AddSaveLoadAction(this, (isSave, path) =>
            {
                PlayableCharsSave filedata;
                if (isSave)
                {
                    filedata = new PlayableCharsSave();
                    if (File.Exists(Path.Combine(path, "unlockedChars.dat"))) // Issue occured with Magical Student, addin' dis!
                        filedata = JsonUtility.FromJson<PlayableCharsSave>(RijndaelEncryption.Decrypt(File.ReadAllText(Path.Combine(path, "unlockedChars.dat")), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
                    filedata.modversion = new Version(PluginInfo.PLUGIN_VERSION);
                    filedata.cyln = unlockedCylnLoon;
                    filedata.partygoer = characters.Find(x => x.name == "The Partygoer").unlocked;
                    filedata.troublemaker = characters.Find(x => x.name == "The Troublemaker").unlocked;
                    filedata.thinker = characters.Find(x => x.name == "The Thinker").unlocked;
                    filedata.backpacker = characters.Find(x => x.name == "The Backpacker").unlocked;
                    filedata.tinkerneer = characters.Find(x => x.name == "The Tinkerneer").unlocked;
                    filedata.testsubject = characters.Find(x => x.name == "The Test Subject").unlocked;
                    filedata.speedrunner = characters.Find(x => x.name == "The Speedrunner").unlocked;
                    if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.playablecharacters.modded"))
                    {
                        if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                            filedata.magical = characters.Find(x => x.name == "Magical Student").unlocked;
                        if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                        {
                            filedata.protagonist = characters.Find(x => x.name == "The Main Protagonist").unlocked;
                            filedata.dweller = characters.Find(x => x.name == "The Dweller").unlocked;
                        }
                    }
                    File.WriteAllText(Path.Combine(path, "unlockedChars.dat"), RijndaelEncryption.Encrypt(JsonUtility.ToJson(filedata), "PLAYABLECHARS_" + PlayerFileManager.Instance.fileName));
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
                        BepInEx.PluginInfo info = Chainloader.PluginInfos.GetValueSafe("alexbw145.baldiplus.playablecharacters.modded");
                        if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
                            SetUnlockedCharacter(info, "Magical Student", filedata.magical);
                        if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                        {
                            SetUnlockedCharacter(info, "The Main Protagonist", filedata.protagonist);
                            SetUnlockedCharacter(info, "The Dweller", filedata.dweller);
                        }
                    }
                }
            });
        }

        public static void UnlockCharacter(BepInEx.PluginInfo info, string charName)
        {
            if (!MTM101BaldiDevAPI.SaveGamesEnabled || (CoreGameManager.Instance != null && !CoreGameManager.Instance.SaveEnabled)
                || characters.Find(x => x.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info) == null
                || characters.Find(x => x.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info).unlocked) return;
            MusicManager.Instance.PlaySoundEffect(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "BAL_Wow"));
            characters.Find(x => x.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info).unlocked = true;
            ModdedSaveSystem.CallSaveLoadAction(Instance, true, ModdedSaveSystem.GetCurrentSaveFolder(Instance));
        }

        /// <summary>
        /// Set the character's unlocked status without calling the save action.
        /// </summary>
        /// <param name="info">The plugin that contains the character</param> <param name="charName">The character's name. (Not case sensitive)</param> <param name="unlocked">Set if the character is unlocked.</param>

        public static void SetUnlockedCharacter(BepInEx.PluginInfo info, string charName, bool unlocked) // I see you cheaters! You can't really unlock CYLN_LOON manually!
        {
            if (characters.Find(x => x.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info) != null)
                characters.Find(x => x.name.ToLower().Replace(" ", "") == charName.ToLower().Replace(" ", "") && x.info == info).unlocked = unlocked;
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
        /// <param name="rmcategory">What category can this room be placed in. (Leave as RoomCategory.Null to make this component craftable anywhere.)</param>
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
    }

    class PluginInfo
    {
        public const string PLUGIN_GUID = "alexbw145.baldiplus.playablecharacters";
        public const string PLUGIN_NAME = "Custom Playable Characters in Baldi's Basics Plus (Core - Base Game)";
        public const string PLUGIN_VERSION = "0.1.1.2"; // UPDATE EVERY TIME!!
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

    internal class PlayableCharsGame : ModdedSaveGameIOBinary
    {
        public override BepInEx.PluginInfo pluginInfo => PlayableCharsPlugin.Instance.Info;
        public static PlayableCharacter Character;
        public static int prevSlots;
        public static ItemObject[] backpackerBackup = new ItemObject[9];

        public override void Reset()
        {
            Character = null;
            prevSlots = 5;
            backpackerBackup = new ItemObject[9];
            for (int i = 0; i < backpackerBackup.Length; i++)
                backpackerBackup[i] = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
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
            PlayableCharacter[] array = PlayableCharsPlugin.characters.FindAll(x => x.info.Metadata.GUID == thiz.PluginGUID).Where(x => x.name.ToLower().Replace(" ", "") == thiz.playablecharName.ToLower().Replace(" ", "")).ToArray();
            if (array.Length != 0) return array.Last();

            return null;
        }

        public PlayableCharacterIdentifier(PlayableCharacter objct)
        {
            PlayableCharacter chara = PlayableCharsPlugin.characters.Find(x => x == objct);
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
internal class BBCRSaveRef
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

