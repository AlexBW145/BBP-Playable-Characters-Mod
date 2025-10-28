using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using PlusLevelStudio.Editor;
using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using BBP_Playables.Core;
using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using PlusStudioLevelFormat;
using UnityEngine.UI;
using BepInEx.Bootstrap;
using PlusStudioLevelLoader;

namespace BBP_Playables.Modded.LevelEditor;

internal static class EditorAdds
{
    internal static IEnumerator EditorLoad()
    {
        yield return "Doing Editor Rewrite Adds...";
        AssetLoader.LocalizationFromFunction((lang) => new Dictionary<string, string>()
                {
                    { "Ed_Tool_item_playablecharacters_unwrappedpresent_Desc", "An leftover wrapping bundle, right click on a item to wrap it up for a specific character." },
                    { "Ed_Tool_item_playablecharacters_tinkerneerwrench_Desc", "A tinkerneering wrench for a brainiac." },
                    { "Ed_Tool_item_playablecharacters_firewallblaster_Desc", "Only one can set it from \"safe mode\" to \"fire mode\" as holding the \"use item\" input fires many bullets as you like while facing against Principal of the Thing's punishment." },

                    { "Ed_Tool_structure_playablecharacters_restricterdoor_Title", "<size=65%>Restricting Playable Character Superdoor</size>" },
                    { "Ed_Tool_structure_playablecharacters_restricterdoor_Desc", "Forbids or allows specific playable characters to enter through that pathway, this will affect the pathfindings of non-player characters." },
                });
        GameObject gameObject36 = EditorInterface.AddStructureGenericVisual("playablecharacters_restricterdoor", LoaderAdds.whitelist.gameObject);
        gameObject36.transform.Find("LockdownDoor_Model").transform.localPosition += Vector3.down * 10f;
        gameObject36.AddComponent<SettingsComponent>().offset = new Vector3(0f, 25f, 0f);
        LevelStudioPlugin.Instance.structureTypes.Add("playablecharacters_restricterdoor", typeof(PlayableCharacterBlacklistStructureLocation));
        GameObject gameObject37 = EditorInterface.AddStructureGenericVisual("playablecharacters_restricterdoor_shut", LoaderAdds.blacklist.gameObject);
        gameObject37.GetComponent<BoxCollider>().center += Vector3.up * 10f;
        gameObject37.AddComponent<SettingsComponent>().offset = new Vector3(0f, 25f, 0f);
        EditorInterfaceModes.AddModeCallback((mode, vanillaComplaint) =>
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "items", [
                new ItemTool("playablecharacters_unwrappedpresent", PlayableCharsPlugin.assetMan.Get<ItemObject>("PresentUnwrapped").itemSpriteSmall),
                new ItemTool("playablecharacters_tinkerneerwrench", PlayableCharsPlugin.assetMan.Get<ItemObject>("TinkerneerWrench").itemSpriteSmall)
            ]);
            if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.playablecharacters.modded") && Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("playablecharacters_firewallblaster", PlayableCharsPlugin.assetMan.Get<ItemObject>("FirewallBlaster").itemSpriteSmall));
            EditorInterfaceModes.AddToolToCategory(mode, "structures", new RestrictedDoorTool("playablecharacters_restricterdoor", PlayableCharsPlugin.assetMan.Get<Sprite>("EditorUI/RestrictedSuperdoor")));
        });
    }
}

internal class RestrictedDoorTool : HallDoorStructureTool
{
    public RestrictedDoorTool(string type, string doorType, Sprite sprite) : base(type, doorType, sprite) { }
    public RestrictedDoorTool(string type, Sprite sprite) : base(type, type, sprite) { }

    protected override bool TryPlace(IntVector2 position, Direction dir)
    {
        PlusStudioLevelFormat.Cell cellSafe = EditorController.Instance.levelData.GetCellSafe(position);
        if (cellSafe == null)
            return false;

        if (cellSafe.type == 16)
            return false;

        EditorController.Instance.AddUndo();
        PlayableCharacterBlacklistStructureLocation hallDoorStructureLocation = (PlayableCharacterBlacklistStructureLocation)EditorController.Instance.AddOrGetStructureToData(type, true);
        SimpleLocation simpleLocation = hallDoorStructureLocation.CreateNewChild();
        simpleLocation.position = position;
        simpleLocation.direction = dir;
        simpleLocation.prefab = doorType;
        hallDoorStructureLocation.superdoors.Add((PlayableCharacterBlacklistLocation)simpleLocation);
        EditorController.Instance.UpdateVisual(hallDoorStructureLocation);
        return true;
    }
}

public class PlayableCharacterBlacklistStructureLocation : HallDoorStructureLocation
{
    public override SimpleLocation CreateNewChild() => new PlayableCharacterBlacklistLocation
    {
        prefab = type,
        deleteAction = (data, local) => OnSubDelete(data, local, true),
        whiteblacklist = new List<string>()
    };

    public override bool OnSubDelete(EditorLevelData data, SimpleLocation local, bool deleteSelf)
    {
        superdoors.Remove((PlayableCharacterBlacklistLocation)local);
        EditorController.Instance.RemoveVisual(local);
        if (superdoors.Count == 0 && deleteSelf)
            OnDelete(data);

        return true;
    }

    public override void CleanupVisual(GameObject visualObject)
    {
        for (int i = 0; i < superdoors.Count; i++)
            EditorController.Instance.RemoveVisual(superdoors[i]);
    }

    public override void InitializeVisual(GameObject visualObject)
    {
        for (int i = 0; i < superdoors.Count; i++)
        {
            if (!(EditorController.Instance.GetVisual(superdoors[i]) != null))
                EditorController.Instance.AddVisual(superdoors[i]);
        }
    }

    public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
    {
        for (int i = 0; i < superdoors.Count; i++)
            superdoors[i].position -= cellOffset;
    }

    public override void UpdateVisual(GameObject visualObject)
    {
        for (int i = 0; i < superdoors.Count; i++)
        {
            if (EditorController.Instance.GetVisual(superdoors[i]) == null)
                EditorController.Instance.AddVisual(superdoors[i]);
            else
                EditorController.Instance.UpdateVisual(superdoors[i]);
        }
    }

    public override bool ValidatePosition(EditorLevelData data)
    {
        for (int num = superdoors.Count - 1; num >= 0; num--)
        {
            if (!superdoors[num].ValidatePosition(data, ignoreSelf: true))
                OnSubDelete(data, superdoors[num], deleteSelf: false);
        }

        return superdoors.Count > 0;
    }

    public override void AddStringsToCompressor(StringCompressor compressor)
    {
        compressor.AddStrings(superdoors.Select(x => x.prefab));
        foreach (var playable in superdoors.Select(x => x.whiteblacklist))
            compressor.AddStrings(playable);
    }

    public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
    {
        writer.Write((byte)1);
        writer.Write(superdoors.Count);
        for (int i = 0; i < superdoors.Count; i++)
        {
            compressor.WriteStoredString(writer, superdoors[i].prefab);
            writer.Write(superdoors[i].whiteblacklist.Count);
            for (int j = 0; j < superdoors[i].whiteblacklist.Count; j++)
                compressor.WriteStoredString(writer, superdoors[i].whiteblacklist[j]);
            writer.Write(EditorExtensions.ToByte(superdoors[i].position));
            writer.Write((byte)superdoors[i].direction);
        }
    }

    public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
    {
        byte b = reader.ReadByte();
        int num = reader.ReadInt32();
        for (int i = 0; i < num; i++)
        {
            PlayableCharacterBlacklistLocation simpleLocation = (PlayableCharacterBlacklistLocation)CreateNewChild();
            if (b > 0)
            {
                simpleLocation.prefab = compressor.ReadStoredString(reader);
                simpleLocation.whiteblacklist = new List<string>();
                int listCount = reader.ReadInt32();
                for (int j = 0; j < listCount; j++)
                    simpleLocation.whiteblacklist.Add(compressor.ReadStoredString(reader));
            }

            simpleLocation.position = EditorExtensions.ToInt(reader.ReadByteVector2());
            simpleLocation.direction = (Direction)reader.ReadByte();
            superdoors.Add(simpleLocation);
        }
    }

    public override GameObject GetVisualPrefab() => null;
    public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
    {
        StructureInfo structureInfo = new StructureInfo(type);
        for (int i = 0; i < superdoors.Count; i++)
        {
            PlayableIDs.PlayableIDList ids = PlayableIDs.PlayableIDList.None;
            foreach (var strings in superdoors[i].whiteblacklist)
                ids |= strings.GetIDFromString();
            structureInfo.data.Add(new StructureDataInfo
            {
                position = superdoors[i].position.ToData(),
                direction = (PlusDirection)superdoors[i].direction,
                prefab = superdoors[i].prefab,
                data = (int)ids
            });
        }
        return structureInfo;
    }

    public List<PlayableCharacterBlacklistLocation> superdoors = new List<PlayableCharacterBlacklistLocation>();
}

public class PlayableCharacterBlacklistLocation : SimpleLocation, IEditorSettingsable
{
    public SpriteRenderer[] renders;

    public override void InitializeVisual(GameObject visualObject)
    {
        visualObject.GetComponent<SettingsComponent>().activateSettingsOn = this;
        renders = visualObject.transform.Find("LockdownDoor_Model").GetComponentsInChildren<SpriteRenderer>().Where(x => x.gameObject.name.EndsWith("Playable")).ToArray();
        base.InitializeVisual(visualObject);
    }

    public void SettingsClicked()
    {
        PlayableCharacterBlacklistSettingsExchangeHandler lockdownDoorSettingsExchangeHandler = EditorController.Instance.CreateUI<PlayableCharacterBlacklistSettingsExchangeHandler>("RestirctPlayableSuperdoorConfig", Path.Combine(AssetLoader.GetModPath(PlayableCharsPlugin.Instance), "EditorJson", "PlayableCharacters_RestrictedSuperDoorConfig.json"));
        lockdownDoorSettingsExchangeHandler.myDoor = this;
        lockdownDoorSettingsExchangeHandler.myLocal = (PlayableCharacterBlacklistStructureLocation)EditorController.Instance.levelData.structures.Where(x => x is PlayableCharacterBlacklistStructureLocation).First(x => ((PlayableCharacterBlacklistStructureLocation)x).superdoors.Contains(this));
        lockdownDoorSettingsExchangeHandler.Assigned();
    }

    public List<string> whiteblacklist;
    public bool isBlacklist = false;

    public override void UpdateVisual(GameObject visualObject)
    {
        base.UpdateVisual(visualObject);
        List<PlayableCharacter> listed = PlayableCharacterMetaStorage.Instance.FindAll(x => whiteblacklist.Contains(x.nameLocalizationKey)).Select(x => x.value).ToList();
        foreach (var render in renders)
        {
            if (whiteblacklist.Count == 0)
                render.sprite = LoaderAdds.noPlayableIcon;
            else
                render.sprite = listed[Mathf.RoundToInt(UnityEngine.Random.Range(0f, listed.Count - 1))].sprselect;
        }
    }
}

public class PlayableCharacterBlacklistSettingsExchangeHandler : EditorOverlayUIExchangeHandler
{
    public PlayableCharacterBlacklistLocation myDoor;

    public PlayableCharacterBlacklistStructureLocation myLocal;

    public TextMeshProUGUI shutText;
    public Image[] listButtons = new Image[3];
    public static int page = 0;
    private PlayableCharacter[] containers = PlayableCharacterMetaStorage.Instance.All().Select(x => x.value).Where(x => !x.componentType.Equals(typeof(PlayableRandomizer)) && PlayableIDs.IDs.ContainsKey(x)).ToArray();

    private bool somethingChanged;

    public override bool OnExit()
    {
        if (somethingChanged)
            EditorController.Instance.AddHeldUndo();
        else
            EditorController.Instance.CancelHeldUndo();

        return base.OnExit();
    }

    public void Assigned()
    {
        shutText.text = (myDoor.prefab == myLocal.type) ? "Set as Blacklist" : "Set as Whitelist";
        // Blacklist - Opens the superdoor, but closes instantly if playing as that playable.
        // Whitelist - Closes the superdoor, but opens instantly if playing as that playable.
        UpdateImages();
    }

    public override void OnElementsCreated()
    {
        base.OnElementsCreated();
        EditorController.Instance.HoldUndo();
        shutText = transform.Find("IsWhitelist").GetComponent<TextMeshProUGUI>();
        listButtons[0] = transform.Find("PlayableList1").GetComponent<Image>();
        listButtons[1] = transform.Find("PlayableList2").GetComponent<Image>();
        listButtons[2] = transform.Find("PlayableList3").GetComponent<Image>();
    }

    private static Color isDeselected = new Color(1f, 1f, 1f, 0.65f);

    private void UpdateImages()
    {
        for (int i = 0; i < listButtons.Length; i++)
        {
            if (containers[page + i] != null)
            {
                listButtons[i].gameObject.SetActive(true);
                listButtons[i].sprite = containers[page + i].sprselect;
                listButtons[i].color = myDoor.whiteblacklist.Contains(containers[page + i].name) ? (myDoor.prefab == myLocal.type ? Color.white : isDeselected) : (myDoor.prefab == myLocal.type ? isDeselected : Color.white);
            }
            else
                listButtons[i].gameObject.SetActive(false);
        }
    }

    public override void SendInteractionMessage(string message, object data)
    {
        if (message == "toggleShut")
        {
            if (myDoor.prefab == myLocal.type)
                myDoor.prefab = "playablecharacters_restricterdoor_shut";
            else
                myDoor.prefab = myLocal.type;
            shutText.text = (myDoor.prefab == myLocal.type) ? "Set as Blacklist" : "Set as Whitelist";

            somethingChanged = true;
            UpdateImages();
            EditorController.Instance.RemoveVisual(myDoor);
            EditorController.Instance.AddVisual(myDoor);
            EditorController.Instance.UpdateVisual(myLocal);
        }
        else if (message.StartsWith("enablelist"))
        {
            int selection = 0;
            switch (message)
            {
                case "enablelist1":
                    selection = 0;
                    break;
                case "enablelist2":
                    selection = 1;
                    break;
                case "enablelist3":
                    selection = 2;
                    break;
            }
            if (containers[page + selection] == null) return;

            if (myDoor.whiteblacklist.Contains(containers[page + selection].name))
                myDoor.whiteblacklist.Remove(containers[page + selection].name);
            else
                myDoor.whiteblacklist.Add(containers[page + selection].name);

            somethingChanged = true;
            UpdateImages();
            EditorController.Instance.UpdateVisual(myLocal);
        }
        else if (message == "nextPage" || message == "prevPage")
        {
            bool forward = message == "nextPage";
            page += forward ? 1 : -1;
            if (page > containers.Length - listButtons.Length)
                page = 0;
            else if (page < 0)
                page = containers.Length - listButtons.Length;

            UpdateImages();
        }

        base.SendInteractionMessage(message, data);
    }
}