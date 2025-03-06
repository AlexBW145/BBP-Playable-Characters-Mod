using BBP_Playables.Core;
#if !DEMO
using BBP_Playables.Modded.BCPP;
#endif
using BBP_Playables.Modded.BBTimes;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using System.Collections;
using System.Linq;
using UnityEngine;
using MTM101BaldAPI.SaveSystem;
using BBP_Playables.Modded.Patches;
using MTM101BaldAPI.Reflection;

namespace BBP_Playables.Modded
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("alexbw145.baldiplus.playablecharacters", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("alexbw145.baldiplus.bcarnellchars", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.bbextracontent", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("txv.bbplus.testvariants", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("BALDI.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static BepInEx.PluginInfo info { get; private set; }
        private void Awake()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            info = Info;
            harmony.PatchAllConditionals();
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), false);
            ModdedSaveGame.AddSaveHandler(Info);
            AssetLoader.LocalizationFromMod(this);

            PlayableCharsPlugin.assetMan.AddRange<Sprite>([
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Magical.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MenuSelect", "Dweller.png"),

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "MagicWand_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "MagicWand_Small.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 50f, "Texture2D", "FirewallBlaster_Large.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "FirewallBlaster_Small.png"),

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 25f, "Texture2D", "BlasterPellets.png"),

                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "HUD", "ItemSlot_BCMAC.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "HUD", "ItemSlotBar_BCMAC.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "HUD", "StaminaBar_BCMAC.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "HUD", "StaminaGradient_BCMAC.png"),
                AssetLoader.SpriteFromMod(this, Vector2.one/2f, 1f, "Texture2D", "HUD", "StaminaPoint_BCMAC.png"),
            ],
            [
                "Portrait/MagicalStudent",
                "Portrait/Dweller", // By https://gamebanana.com/members/3165945

                "Items/MagicalStudentWand_Large",
                "Items/MagicalStudentWand_Small",
                "Items/FirewallBlaster_Large",
                "Items/FirewallBlaster_Small",

                "FirewallBlaster_Pellets",

                "HUD/ItemSlotsBCMAC",
                "HUD/ItemSlotBar_BCMAC",
                "HUD/StaminaBarBCMAC",
                "HUD/StaminaGradientBCMAC",
                "HUD/StaminaPointBCMAC",
            ]);
            PlayableCharsPlugin.assetMan.AddRange<Sprite[]>([
                AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Default.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Fanon.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "CYLN_LOON.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Partygoer.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Troublemaker.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Thinker.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Backpacker.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Tinkerneer.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "TestSubject.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Speedrunner.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "MgS.png")),
            AssetLoader.SpritesFromSpritesheet(2,1,29.8f,Vector2.one/2f,AssetLoader.TextureFromMod(this, "Texture2D", "PlayerVisuals", "Dweller.png")),],
                [
                    "Visual/Default",
                    "Visual/Fanon",
                    "Visual/CYLN_LOON",
                    "Visual/Partygoer",
                    "Visual/Troublemaker",
                    "Visual/Thinker",
                    "Visual/Backpacker",
                    "Visual/Tinkerneer",
                    "Visual/TestSubject",
                    "Visual/Speedrunner",
                    "Visual/MagicalStudent",
                    "Visual/Dweller"
                ]);
            // Seems like rect was used for some reason...
            Sprite sprite = Sprite.Create(PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC").texture, new Rect(0f, 0f, 40f, 36f), Vector2.one / 2f, 1f, 0u, SpriteMeshType.FullRect);
            sprite.name = "SprItemSlotsBCMAC_left";
            PlayableCharsPlugin.assetMan.Add<Sprite>("HUD/ItemSlotsBCMAC_left", sprite);
            sprite = Sprite.Create(PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC").texture, new Rect(PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC").rect.x - 80f, 0f, 40f, 36f), Vector2.one / 2f, 1f, 0u, SpriteMeshType.FullRect);
            sprite.name = "SprItemSlotsBCMAC_center";
            PlayableCharsPlugin.assetMan.Add<Sprite>("HUD/ItemSlotsBCMAC_center", sprite);
            sprite = Sprite.Create(PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC").texture, new Rect(PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC").rect.x - 40f, 0f, 40f, 36f), Vector2.one / 2f, 1f, 0u, SpriteMeshType.FullRect);
            sprite.name = "SprItemSlotsBCMAC_right";
            PlayableCharsPlugin.assetMan.Add<Sprite>("HUD/ItemSlotsBCMAC_right", sprite);
            PlayableCharsPlugin.assetMan.AddRange<SoundObject>([
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "FirewallPellets.ogg"), "Sfx_FirewallPellets", SoundType.Effect, Color.white),
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "AudioClip", "FirewallEffect.wav"), "Sfx_FirewallEffect", SoundType.Effect, Color.white),
            ],
            [
                "Items/BlasterShoot",
                "Items/BlasterEffected"
            ]);
        }

        // THE ENTIRE THING ACTS LIKE THAT FEATURE IN DON'T STARVE IF YOU PLAN TO CREATE A WORLD WITH DLCS OR NOT!!
        IEnumerator PreLoad()
        {
            yield return 1;
            yield return "Adding other new characters";
            if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
            {
                GameObject shooter = Instantiate(ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item.gameObject);
                shooter.name = "ITM_FirewallShooter";
                Destroy(shooter.GetComponent<ITM_BSODA>());
                shooter.AddComponent<ITM_FirewallBlaster>().entity = shooter.GetComponent<Entity>();
                shooter.ConvertToPrefab(true);
                shooter.layer = LayerMask.NameToLayer("StandardEntities");
                Destroy(shooter.transform.Find("RendereBase").Find("Particles").gameObject);
                shooter.transform.Find("RendereBase").GetComponentInChildren<SpriteRenderer>().sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("FirewallBlaster_Pellets");
                shooter.GetComponent<ITM_FirewallBlaster>().shootSnd = PlayableCharsPlugin.assetMan.Get<SoundObject>("Items/BlasterShoot");
                ITM_FirewallBlaster.effectSnd = PlayableCharsPlugin.assetMan.Get<SoundObject>("Items/BlasterEffected");
                PlayableCharsPlugin.assetMan.Add<ItemObject>("FirewallBlaster", new ItemBuilder(Info)
                    .SetItemComponent(shooter.GetComponent<ITM_FirewallBlaster>())
                    .SetEnum("FirewallBlaster")
                    .SetNameAndDescription("Itm_FirewallBlaster", "Desc_FirewallBlaster")
                    .SetShopPrice(0)
                    .SetGeneratorCost(99)
                    .SetSprites(PlayableCharsPlugin.assetMan.Get<Sprite>("Items/FirewallBlaster_Small"), PlayableCharsPlugin.assetMan.Get<Sprite>("Items/FirewallBlaster_Large"))
                    .SetMeta(ItemFlags.MultipleUse | ItemFlags.CreatesEntity | ItemFlags.Persists, ["BCPP", "CharacterItemImportant"])
                    .Build());
                /*Material wholeWhiteMat = new Material(Shader.Find("GUI/Text Shader"));
                wholeWhiteMat.name = "MapWhiteMat";
                PlayableCharsPlugin.assetMan.Add<Material>("DwellerMapMat", wholeWhiteMat);*/
                new PlayableCharacterBuilder<MainProtagonistHUDInit>(Info, false)
                .SetNameAndDesc("The Main Protagonist", "Desc_Protagonist")
                .SetPortrait(PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Placeholder"))
                .SetStats(s: 5, w: 18, r: 28) // Original Stats: 14, 18
                .SetTags("BCPP")
                .Build();
                new PlayableCharacterBuilder<DwellerComponent>(Info, false)
                .SetNameAndDesc("The Dweller", "Desc_Dweller")
                .SetPortrait(PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Dweller"))
                .SetStats(s: 6, w: 26, r: 26, sm: 0f) // Original Stats: 14, 18
                .SetFlags(PlayableFlags.None)
                .Build();
            }
            if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.bbextracontent"))
            {
                PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Partygoer").value.startingItems = [ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Present")).value];
                GameObject magic = Instantiate(ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item.gameObject);
                magic.name = "MagicObjectPlayer";
                Destroy(magic.GetComponent<ITM_BSODA>());
                magic.AddComponent<ITM_MagicWandMagical>().entity = magic.GetComponent<Entity>();
                magic.GetComponent<ITM_MagicWandMagical>().spriteRenderer = magic.transform.Find("RendereBase").GetComponentInChildren<SpriteRenderer>();
                magic.GetComponent<ITM_MagicWandMagical>().spriteRenderer.sprite = Resources.FindObjectsOfTypeAll<Sprite>().ToList().Find(x => x.name == "SprBBTimesAsset_MGS_Magic");
                magic.ConvertToPrefab(true);
                magic.layer = LayerMask.NameToLayer("StandardEntities");
                Destroy(magic.transform.Find("RendereBase").Find("Particles").gameObject);
                PlayableCharsPlugin.assetMan.Add<ItemObject>("MagicalWandTimesCharacter", new ItemBuilder(Info)
                    .SetItemComponent(magic.GetComponent<ITM_MagicWandMagical>())
                    .SetEnum("MagicalWandTimesCharacter")
                    .SetNameAndDescription("Itm_MagicWand", "Desc_MagicWand")
                    .SetShopPrice(int.MaxValue)
                    .SetGeneratorCost(int.MaxValue)
                    .SetSprites(PlayableCharsPlugin.assetMan.Get<Sprite>("Items/MagicalStudentWand_Small"), PlayableCharsPlugin.assetMan.Get<Sprite>("Items/MagicalStudentWand_Large"))
                    .SetMeta(ItemFlags.MultipleUse | ItemFlags.CreatesEntity | ItemFlags.Persists, ["CharacterItemImportant"])
                    .Build());
                new PlayableCharacterBuilder<PlayableCharacterComponent>(Info, false)
                .SetNameAndDesc("Magical Student", "Desc_Magical")
                .SetPortrait(PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/MagicalStudent"))
                .SetStats(s: 4, w: 16, r: 16, sm: 0f)
                .SetStartingItems(PlayableCharsPlugin.assetMan.Get<ItemObject>("MagicalWandTimesCharacter"))
                .Build();

                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Default").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Default"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Predicted Fanon").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Fanon"));
                //PlayerVisualPatch.playableEmotions.Add(default, [Resources.FindObjectsOfTypeAll<Sprite>().ToList().First(x => x.name == "BBTimesAsset_player0_0"), Resources.FindObjectsOfTypeAll<Sprite>().ToList().First(x => x.name == "BBTimesAsset_player1_0")]);
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "CYLN_LOON").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/CYLN_LOON"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Partygoer").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Partygoer"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Troublemaker").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Troublemaker"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Thinker").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Thinker"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Backpacker").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Backpacker"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Tinkerneer").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Tinkerneer"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Test Subject").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/TestSubject"));
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Speedrunner").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Speedrunner"));
                if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                    PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "The Dweller").value, [PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Dweller")[0], PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/Dweller")[0]]);
                PlayerVisualPatch.playableEmotions.Add(PlayableCharacterMetaStorage.Instance.Find(p => p.nameLocalizationKey == "Magical Student").value, PlayableCharsPlugin.assetMan.Get<Sprite[]>("Visual/MagicalStudent"));

                GameObject basketballshooter = new GameObject("BasketballCannon_Tinkerneer", typeof(BoxCollider));
                basketballshooter.GetComponent<BoxCollider>().isTrigger = true;
                basketballshooter.GetComponent<BoxCollider>().size = Vector3.one;
                var shooterbod = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shooterbod.transform.SetParent(basketballshooter.transform);
                shooterbod.transform.localScale = new(4f, 5f, 4f);
                shooterbod.transform.localPosition = Vector3.up * 3.2f;
                var mat = Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "Locker_Red"));
                mat.SetMainTexture(Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(x => x.name == "machineTexture"));
                shooterbod.GetComponent<Renderer>().SetMaterial(mat);
                Destroy(shooterbod.GetComponent<Collider>());
                var shootercann = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shootercann.transform.SetParent(basketballshooter.transform);
                shootercann.transform.localScale = new(1f, 1f, 3f);
                shootercann.transform.localPosition = new(-0.5f, 4.13f, 0.55f);
                mat = Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "Locker_Red"));
                mat.SetMainTexture(Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(x => x.name == "cannonTexture"));
                shootercann.GetComponent<Renderer>().SetMaterial(mat);
                // Copy and pasted from BBT because I can't use that function to this script.
                Mesh mesh = shootercann.GetComponent<MeshFilter>().mesh;
                mesh.Clear();
                mesh.vertices = [
                new (0, 1f, 0),
            new (0, 0, 0),
            new (1.004f, 1f, 0),
            new (1f, 0, 0),

            new (0, 0, 1f),
            new (1f, 0, 1f),
            new (0, 1f, 1f),
            new (1f, 1f, 1f),

            new (0, 1f, 0),
            new (1f, 1f, 0),

            new (0, 1f, 0),
            new(0, 1f, 1f),

            new(1f, 1f, 0),
            new(1f, 1f, 1f),
        ];
                mesh.triangles = [
                0, 2, 1, // front
			1, 2, 3,
            4, 5, 6, // back
			5, 7, 6,
            6, 7, 8, //top
			7, 9 ,8,
            1, 3, 4, //bottom
			3, 5, 4,
            1, 11,10,// left
			1, 4, 11,
            3, 12, 5,//right
			5, 12, 13


            ];
                mesh.uv = [
                new(0, 0.66f),
            new(0.25f, 0.66f),
            new(0, 0.33f),
            new(0.25f, 0.34f),

            new(0.5f, 0.66f),
            new(0.5f, 0.34f),
            new(0.75f, 0.65f),
            new(0.75f, 0.34f),

            new(1, 0.66f),
            new(1, 0.34f),

            new(0.25f, 1),
            new(0.5f, 1),

            new(0.25f, 0),
            new(0.5f, 0),
        ];
                mesh.Optimize();
                mesh.RecalculateNormals();
                Destroy(shootercann.GetComponent<Collider>());
                var shooterbase = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shooterbase.transform.SetParent(basketballshooter.transform);
                shooterbase.transform.localScale = new(5f, 4f, 5f);
                shooterbase.transform.localPosition = Vector3.zero;
                shooterbase.GetComponent<Renderer>().SetMaterial(Resources.FindObjectsOfTypeAll<Material>().ToList().Last(x => x.name == "BlackUnlit"));
                Destroy(shooterbase.GetComponent<Collider>());
                basketballshooter.CreateTinkerneeringObject<BasketBallThrower>(Info, "Basketball Shooter", "An iffy version of the school's basketball shooter, it does not hit you, but can push back others at a slow pace.", [ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Basketball")).value, ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value, ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Coal")).value], true, RoomCategory.Hall);
                basketballshooter.GetComponent<BasketBallThrower>().audMan = basketballshooter.AddComponent<PropagatedAudioManager>();
                basketballshooter.GetComponent<BasketBallThrower>().audMan.audioDevice = basketballshooter.AddComponent<AudioSource>();
                basketballshooter.GetComponent<BasketBallThrower>().audBoom = Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.soundClip.name == "shootBoom");
                var annoying = basketballshooter.GetComponent<BasketBallThrower>().audMan.audioDevice;
                annoying.spatialBlend = 1;
                annoying.rolloffMode = AudioRolloffMode.Custom;
                annoying.maxDistance = 150;
                annoying.dopplerLevel = 0;
                annoying.spread = 0;
            }

            GeneratorManagement.Register(this, GenerationModType.Addend, (name, num, ld) =>
            {
                if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                    ld.levelObject.potentialItems = ld.levelObject.potentialItems.AddRangeToArray([
                        new ()
                        {
                            selection = PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"),
                            weight = 9
                        }
                    ]);
            });
        }
    }

    class PluginInfo
    {
        public const string PLUGIN_GUID = "alexbw145.baldiplus.playablecharacters.modded";
        public const string PLUGIN_NAME = "Custom Playable Characters in Baldi's Basics Plus (Core - Modded)";
        public const string PLUGIN_VERSION = "0.17"; // UPDATE EVERY TIME!!
    }
}
