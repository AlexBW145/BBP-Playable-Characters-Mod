using BBP_Playables.Core;
using MonoMod.Utils;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MTM101BaldAPI.Reflection;
using PlusStudioLevelLoader;
using BepInEx.Bootstrap;

namespace BBP_Playables.Modded.LevelEditor;

internal static class LoaderAdds
{
    internal static Sprite restrictIcon { get; private set; }
    internal static Sprite noPlayableIcon { get; private set; }

    internal static RestrictAllowPlayableSuperdoor blacklist { get; private set; }
    internal static RestrictAllowPlayableSuperdoor whitelist { get; private set; }

    internal static IEnumerator LoaderLoad()
    {
        yield return "Doing Level Loader Adds...";
        LevelLoaderPlugin.Instance.itemObjects.AddRange(new Dictionary<string, ItemObject>()
        {
            { "playablecharacters_unwrappedpresent", PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentUnwrapped") },
            { "playablecharacters_tinkerneerwrench", PlayableCharsPlugin.assetMan.Get<ItemObject>("TinkerneerWrench") },
        });
        if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
            LevelLoaderPlugin.Instance.itemObjects.Add("playablecharacters_firewallblaster", PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster"));
        yield return "Doing Premade Exclusive Creations...";
        PlayableCharsPlugin.assetMan.AddRange(
            [AssetLoader.SpriteFromMod(PlayableCharsPlugin.Instance, Vector2.one/2f, 1f, "Texture2D", "EditorUI", "structure_playablecharacters_restricterdoor.png"),
        AssetLoader.SpriteFromMod(PlayableCharsPlugin.Instance, Vector2.one/2f, 1f, "Texture2D", "RestrictedPlayable.png")],
            ["EditorUI/RestrictedSuperdoor",
            "Structure/RestrictedIcon"]);
        noPlayableIcon = PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Random");
        var superdoor = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LockdownDoor>().Last(x => x.name == "LockdownDoor"), MTM101BaldiDevAPI.prefabTransform, true);
        var restricteddoor = superdoor.gameObject.AddComponent<RestrictAllowPlayableSuperdoor>();
        restricteddoor.gameObject.name = "PlayableChars_RestrictPlayableSuperdoor_Blacklist";
        restricteddoor.door = superdoor.ReflectionGetVariable("door") as MeshRenderer;
        restricteddoor.collider = superdoor.ReflectionGetVariable("collider") as BoxCollider;
        restricteddoor.mapUnlockedSprite = superdoor.ReflectionGetVariable("mapUnlockedSprite") as Sprite;
        restricteddoor.mapLockedSprite = superdoor.ReflectionGetVariable("mapLockedSprite") as Sprite;
        restricteddoor.lockBlocks = true;
        restricteddoor.closeBlocks = true;
        restricteddoor.makesNoise = false;
        GameObject.DestroyImmediate(superdoor);
        var nobillboard = Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "SpriteStandard_NoBillboard");
        var model = restricteddoor.transform.Find("LockdownDoor_Model");
        var restricticon = PlayableCharsPlugin.assetMan.Get<Sprite>("Structure/RestrictedIcon");
        restrictIcon = restricticon;
        var spriterender = new GameObject("SideForwardSprite", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        spriterender.material = nobillboard;
        spriterender.transform.SetParent(model, false);
        spriterender.transform.localPosition = new Vector3(0f, 5f, 1f);
        spriterender.transform.localScale = Vector3.one * 0.05f;
        spriterender.sprite = restricticon;
        var spriterender2 = new GameObject("SideBackwardsSprite", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        spriterender2.material = nobillboard;
        spriterender2.transform.SetParent(model, false);
        spriterender2.transform.localPosition = new Vector3(0f, 5f, -1f);
        spriterender2.transform.localScale = Vector3.one * 0.05f;
        spriterender2.sprite = restricticon;
        restricteddoor.renders = [spriterender, spriterender2];
        var placehold = PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Placeholder");
        var spriteplayable = GameObject.Instantiate(spriterender, model, true);
        spriteplayable.gameObject.name = "SideForwardPlayable";
        spriteplayable.sprite = placehold;
        spriteplayable.transform.localScale = Vector3.one * 0.035f;
        spriteplayable.sortingOrder--;
        var spriteplayable2 = GameObject.Instantiate(spriterender2, model, true);
        spriteplayable2.gameObject.name = "SideBackwardsPlayable";
        spriteplayable2.sprite = placehold;
        spriteplayable2.transform.localScale = Vector3.one * 0.035f;
        spriteplayable2.sortingOrder--;
        restricteddoor.playables = [spriteplayable, spriteplayable2];
        restricteddoor.DisallowMode = true;
        var openrestricteddoor = GameObject.Instantiate(restricteddoor, MTM101BaldiDevAPI.prefabTransform, true);
        openrestricteddoor.DisallowMode = false;
        openrestricteddoor.gameObject.name = "PlayableChars_RestrictPlayableSuperdoor_Whitelist";
        foreach (var render in openrestricteddoor.renders)
            render.sprite = null;
        foreach (var render in restricteddoor.renders)
            render.sprite = restricticon;
        var structurebuilder = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<Structure_HallDoor>().Last(x => x.name == "LockdownDoorConstructor"), MTM101BaldiDevAPI.prefabTransform, false);
        var playablestructurebuilder = structurebuilder.gameObject.AddComponent<Structure_RestrictAllowPlayableSuperdoor>();
        playablestructurebuilder.defaultDoorPrefab = openrestricteddoor;
        playablestructurebuilder.defaultIfTrapped = (bool)structurebuilder.ReflectionGetVariable("defaultIfTrapped");
        playablestructurebuilder.gameObject.name = "RestrictedSuperdoorBuilder";
        GameObject.DestroyImmediate(structurebuilder);
        LevelLoaderPlugin.Instance.structureAliases.Add("playablecharacters_restricterdoor", new LoaderStructureData(playablestructurebuilder, new Dictionary<string, GameObject>()
        {
            { "playablecharacters_restricterdoor", openrestricteddoor.gameObject },
            { "playablecharacters_restricterdoor_shut", restricteddoor.gameObject }
        }));
        blacklist = restricteddoor;
        whitelist = openrestricteddoor;
        foreach (var playable in PlayableCharacterMetaStorage.Instance.All())
        {
            if (playable.info == PlayableCharsPlugin.Instance.Info || playable.info.Metadata.GUID == "alexbw145.baldiplus.playablecharacters.modded" || playable.info.Metadata.GUID == "alexbw145.baldiplus.playablecharacters.foxo")
            {
                switch (playable.nameLocalizationKey.ToLower().Replace(" ", ""))
                {
                    case "randomplayable":
                        break;
                    case "thedefault":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Default);
                        break;
                    case "thepredictedfanon":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Fanon);
                        break;
                    case "thepredictedfanon[times]":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.FanonT);
                        break;
                    case "cyln_loon":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.CYLNLOON);
                        break;
                    case "thepartygoer":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Partygoer);
                        break;
                    case "thetroublemaker":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Troublemaker);
                        break;
                    case "thethinker":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Thinker);
                        break;
                    case "thebackpacker":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Backpacker);
                        break;
                    case "thetinkerneer":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Tinkerneer);
                        break;
                    case "thetestsubject":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.TestSubject);
                        break;
                    case "thespeedrunner":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Speedrunner);
                        break;
                    case "magicalstudent":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.MgS);
                        break;
                    case "themainprotagonist":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Protagonist);
                        break;
                    case "thedweller":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Dweller);
                        break;
                    case "thetraumatized":
                        PlayableIDs.IDs.Add(playable.value, PlayableIDs.PlayableIDList.Traumatized);
                        break;
                }
            }
        }
    }
}

public static class PlayableIDs
{
    [Flags]
    public enum PlayableIDList
    {
        None = 0x0,
        Default = 0x1,
        Fanon = 0x2,
        FanonT = 0x4,
        CYLNLOON = 0x8,
        Partygoer = 0x10,
        Troublemaker = 0x20,
        Thinker = 0x40,
        Backpacker = 0x80,
        Tinkerneer = 0x100,
        TestSubject = 0x200,
        Speedrunner = 0x400,
        MgS = 0x800,
        Dweller = 0x1000,
        Protagonist = 0x2000,
        Traumatized = 0x4000
    }

    internal readonly static Dictionary<PlayableCharacter, PlayableIDList> IDs = new Dictionary<PlayableCharacter, PlayableIDList>();

    public static PlayableIDList GetID(this PlayableCharacter me)
    {
        if (!IDs.ContainsKey(me))
            return PlayableIDList.None;
        return IDs[me];
    }

    public static PlayableIDList GetIDFromString(this string theirname)
    {
        PlayableCharacter me = PlayableCharacterMetaStorage.Instance.Find(x => x.nameLocalizationKey == theirname).value;
        if (!IDs.ContainsKey(me))
            return PlayableIDList.None;
        //IDs.Add(me, (PlayableIDList)Convert.ToInt32(me.name.Replace(" ", "").Replace("The", "")));
        return IDs[me];
    }

    public static PlayableCharacter[] GetPlayables(this PlayableIDList me) => IDs.Where(x => x.Value != PlayableIDList.None && me.HasFlag(x.Value))
        .Select(x => x.Key).ToArray();
}