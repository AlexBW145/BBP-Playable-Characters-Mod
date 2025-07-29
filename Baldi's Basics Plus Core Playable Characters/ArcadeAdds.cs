using EndlessFloorsForever;
using EndlessFloorsForever.Components;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore;

namespace BBP_Playables.Core;

public static class ArcadeAdds
{
    internal static Dictionary<string, Sprite> Upgrades => (Dictionary<string, Sprite>)PlayableCharsPlugin.assetMan["INFUpgrades"];
    internal static IEnumerator EndlessLoad()
    {
        yield return 1;
        yield return "Creating Upgrades";
        AssetLoader.LocalizationFromFunction((lang) =>
        {
            return new Dictionary<string, string>()
            {
                { "Upg_PlayableCharsStamina", "Zesty Energizer\nStaminaless playables now have a speed boost when consuming stamina-related items.\nThis does not take up an upgrade slot!" },

                { "Upg_CYLNLOONLastChance", "Buggedout\nGives you another chance to continue the game after getting caught" },
                { "Upg_CYLNLOONThrowableRespawn", "Throw--\nDecreases the random cooldown for throwable objects" },
                { "Upg_CYLNLOONThrowableRespawn2", "Throw--\nThe random cooldown for throwable objects are now set to 30 as always" },

                { "Upg_BackpackerHiker", "Hiker\nReduces the struggles of the Backpack's weight" },
                { "Upg_BackpackerHiker1", "Hiker+\nReduces the struggles of the Backpack's weight" },

                { "Upg_TestSubjectPenalty", "Penalty Destabilizer\nAs time says, this makes the math machine penalty be less punishing..." },
                { "Upg_TestSubjectTimeBender", "Time Bender\nThe Test Subject has learnt how to control time and space\nwhere time flows depend on the amount of people are in the same room as The Test Subject\nand the amount of people that are looking at The Test Subject." },

                { "Upg_SpeedrunnerSpeedy1", "Speedy 1x\nGives The Speedrunner 1.1x more speed" },
                { "Upg_SpeedrunnerSpeedy2", "Speedy 2x\nGives The Speedrunner 1.2x more speed" },
                { "Upg_SpeedrunnerSpeedy3", "Speedy 4x\nGives The Speedrunner 1.4x more speed" },
                { "Upg_SpeedrunnerSpeedy4", "Speedy 8x\nGives The Speedrunner 1.8x more speed" },
                { "Upg_SpeedrunnerSpeedy5", "Speedy 16x\nGives The Speedrunner 2.6x more speed\nPlease stop, you're making it worse..." },
                { "Upg_SpeedrunnerSpeedy6", "Speedy 32x\nGives The Speedrunner 4.2x more speed\nYou can't really think fast..." },
                { "Upg_SpeedrunnerSpeedy7", "Speedy 64x\nGives The Speedrunner 7.4x more speed\nThis feels like you have entered into some engine..." },
                { "Upg_SpeedrunnerSpeedy8", "Speedy 128x\nGives The Speedrunner 13.8x more speed\nYou need more??" },
                { "Upg_SpeedrunnerSpeedy9", "Speedy 256x\nGives The Speedrunner 26.6x more speed\n<color=red>LAST ONE IN STOCK!</color>" },
            };
        });
        PlayableCharsPlugin.assetMan.Add<SoundObject[]>("LoseLastChanceSnds", [
            ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(PlayableCharsPlugin.Instance, "AudioClip", "CHANCE_no.wav"), "Sfx_Lose_Buzz", SoundType.Effect, Color.green),
            ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(PlayableCharsPlugin.Instance, "AudioClip", "CHANCE_steamexplode.wav"), "Sfx_Lose_Buzz", SoundType.Effect, Color.green),
            ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(PlayableCharsPlugin.Instance, "AudioClip", "CHANCE_what.wav"), "Sfx_Lose_Buzz", SoundType.Effect, Color.green)
        ]);
        PlayableCharsPlugin.assetMan.Add("INFUpgrades", new Dictionary<string, Sprite>());
        string iconPath = Path.Combine(AssetLoader.GetModPath(PlayableCharsPlugin.Instance), "Texture2D", "UpgradeIcons");
        foreach (string p in Directory.GetFiles(iconPath))
        {
            Texture2D tex = AssetLoader.TextureFromFile(p);
            Sprite spr = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, 25f);
            Upgrades.Add(Path.GetFileNameWithoutExtension(p), spr);
        }
        new StaminalessUpgrade("playablechars_staminaless", 50)
        {
            behavior = UpgradePurchaseBehavior.IncrementCounter,
            levels = [new UpgradeLevel()
            {
                icon = "PlayableCharsStamina",
                cost = 5000,
                descLoca = "Upg_PlayableCharsStamina"
            }]
        }.Register();
        // CYLN_LOON
        var glitched = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "CYLN_LOON").value;
        new PlayableCharUpgrade("cylnloon_onechance", 25, glitched)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot,
            levels = [new UpgradeLevel()
            {
                icon = "CylnloonOneChance",
                cost = 7500,
                descLoca = "Upg_CYLNLOONLastChance"
            }]
        }.Register();
        new PlayableCharUpgrade("cylnloon_thorwablesrespawn", 59, glitched)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot,
            levels = [new UpgradeLevel()
            {
                icon = "CylnloonThrowableRespawn",
                cost = 3500,
                descLoca = "Upg_CYLNLOONThrowableRespawn"
            },
            new UpgradeLevel()
            {
                icon = "CylnloonThrowableRespawn2",
                cost = 5000,
                descLoca = "Upg_CYLNLOONThrowableRespawn2"
            }]
        }.Register();
        // Partygoer
        var partyman = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Partygoer").value;
        // Troublemaker
        var bullyjr = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Troublemaker").value;
        // Thinker
        var thinker = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Thinker").value;
        // Backpacker
        var backpack = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Backpacker").value;
        new PlayableCharUpgrade("backpacker_penaltyremoval", 50, backpack)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot,
            levels = [new UpgradeLevel()
            {
                icon = "BackpackerHiker",
                cost = 3000,
                descLoca = "Upg_BackpackerHiker"
            },
            new UpgradeLevel()
            {
                icon = "BackpackerHiker",
                cost = 5500,
                descLoca = "Upg_BackpackerHiker1"
            }]
        }.Register();
        // Tinkerneer
        var engi = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Tinkerneer").value;
        // Test Subject
        var testjr = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Test Subject").value;
        new PlayableCharUpgrade("testsubject_penaltyremoval", 99, testjr)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot,
            levels = [
                new UpgradeLevel()
                {
                    icon = "TestSubjectPenalty",
                    cost = DateTime.Now.Year*((int)DateTime.Now.DayOfWeek + 1),
                    descLoca = "Upg_TestSubjectPenalty"
                }
            ]
        }.Register();
        new TimeBenderUpgrade("testsubject_timebender", 99, testjr)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot,
            levels = [
                new UpgradeLevel()
                {
                    icon = "TestSubjectTimeBender",
                    cost = 9999,
                    descLoca = "Upg_TestSubjectTimeBender"
                }
            ]
        }.Register();
        // Speedrunner
        var speedrunner = PlayableCharacterMetaStorage.Instance.Find(playable => playable.nameLocalizationKey == "The Speedrunner").value;
        new SpeedrunnerSpeedyUpgrade("speedrunner_speedy", 55, speedrunner)
        {
            behavior = UpgradePurchaseBehavior.FillUpgradeSlot, // The more expensive, the more fast they are.
            levels = [
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 999,
                    descLoca = "Upg_SpeedrunnerSpeedy1"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 1998,
                    descLoca = "Upg_SpeedrunnerSpeedy2"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 3996,
                    descLoca = "Upg_SpeedrunnerSpeedy3"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 7992,
                    descLoca = "Upg_SpeedrunnerSpeedy4"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 15984,
                    descLoca = "Upg_SpeedrunnerSpeedy5"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 31968,
                    descLoca = "Upg_SpeedrunnerSpeedy6"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 63936,
                    descLoca = "Upg_SpeedrunnerSpeedy7"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 127872,
                    descLoca = "Upg_SpeedrunnerSpeedy8"
                },
                new UpgradeLevel()
                {
                    icon = "SpeedrunnerSpeedy",
                    cost = 255744,
                    descLoca = "Upg_SpeedrunnerSpeedy9"
                },
            ]
        }.Register();
    }
}

class StaminalessUpgrade : StandardUpgrade
{
    public StaminalessUpgrade(string id, int weight) : base(id, weight) { }

    public override Sprite GetIcon(int level) => ArcadeAdds.Upgrades[GetIconKey(level)];

    public override bool ShouldAppear(int currentLevel) => base.ShouldAppear(currentLevel) && PlayableCharsPlugin.Instance.Character.staminaMax <= 0;
}

public class PlayableCharUpgrade : StandardUpgrade
{
    private PlayableCharacter character;
    public PlayableCharUpgrade(string id, int weight, PlayableCharacter man) : base (id, weight)
    {
        character = man;
    }

    public override Sprite GetIcon(int level) => ArcadeAdds.Upgrades[GetIconKey(level)];

    public override bool ShouldAppear(int currentLevel) => base.ShouldAppear(currentLevel) && PlayableCharsPlugin.Instance.Character == character;
}

#region CYLN_LOON Upgrades
#endregion

#region Partygoer Upgrades
#endregion

#region Troublemaker Upgrades
#endregion

#region Thinker Upgrades
#endregion

#region Backpacker Upgrades
#endregion

#region Tinkerneer Upgrades
#endregion

#region Test Subject Upgrades
class TimeBenderUpgrade : PlayableCharUpgrade
{
    public TimeBenderUpgrade(string id, int weight, PlayableCharacter man) : base(id, weight, man) { }

    public override int GetCost(int level) => UnityEngine.Random.RandomRangeInt(1994, 499990);

    public override int CalculateSellPrice(int level) => UnityEngine.Random.RandomRangeInt(1994, 5000);
}
#endregion

#region Speedrunner Upgrades
class SpeedrunnerSpeedyUpgrade : PlayableCharUpgrade
{
    public SpeedrunnerSpeedyUpgrade(string id, int weight, PlayableCharacter man) : base(id, weight, man) { }

    public ValueModifier speedyStat = new ValueModifier();
    private float[] speedyValues = [1f, 1.1f, 1.2f, 1.4f, 1.8f, 2.6f, 4.2f, 7.4f, 13.8f, 26.6f];

    public override void OnPurchase()
    {
        SetValue();
        base.OnPurchase();
    }

    internal void SetValue() => speedyStat.multiplier = speedyValues[Mathf.Min(EndlessForeverPlugin.Instance.GetUpgradeCount(id), speedyValues.Length - 1)];
}
#endregion