using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace BBP_Playables.Core.Patches
{
    [HarmonyPatch(typeof(MainMenu), "Start")]
    class PartygoerThinkerUnlock
    {
        static void Postfix(ref GameObject ___seedInput)
        {
            PlayableCharsPlugin.gameStarted = false;
            if (!PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Partygoer").value.unlocked && PlayerFileManager.Instance.clearedLevels[2])
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Partygoer");
            if (!PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Thinker").value.unlocked && PlayableCharacterMetaStorage.Instance.FindAll(x => x.value.unlocked && x.info == PlayableCharsPlugin.Instance.Info && (!x.flags.HasFlag(PlayableFlags.UnlockedFromStart) || x.value.name.ToLower().Replace(" ", "") == "cyln_loon")).ToValues().Count() >= 4)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Thinker");
            if (!PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Backpacker").value.unlocked // FIND ALL except those item points and the map...
                //&& PlayerFileManager.Instance.foundItems.ToList().FindAll(x => x == true).Count >= 17
                && PlayableCharacterMetaStorage.Instance.Find(x => x.value.name == "The Thinker").value.unlocked)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Backpacker");
            if (!ModdedFileManager.Instance.saveData.saveAvailable)
            {
                PlayableCharsGame.Character = null;
                PlayableCharsGame.prevSlots = 5;
                PlayableCharsGame.backpackerBackup = new ItemObject[9];
                for (int i = 0; i < PlayableCharsGame.backpackerBackup.Length; i++)
                    PlayableCharsGame.backpackerBackup[i] = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
            }
            //Debug.Log(PlayerFileManager.Instance.foundItems.ToList().FindAll(x => x == true).Count + "\n" + PlayerFileManager.Instance.foundChars.ToList().FindAll(x => x == true).Count + "\n" + PlayerFileManager.Instance.foundEvnts.ToList().FindAll(x => x == true).Count);
            /*if (___seedInput != null && ___seedInput?.transform.parent.Find("MainNew") != null) {
                ___seedInput.transform.parent.Find("MainNew").GetComponent<StandardMenuButton>().transitionOnPress = true;
                ___seedInput.transform.parent.Find("MainNew").GetComponent<StandardMenuButton>().transitionType = UiTransition.SwipeRight;
                ___seedInput.transform.parent.Find("MainNew").GetComponent<StandardMenuButton>().transitionTime = 1.0666667f;
            }*/

            var thething = new GameObject("Portrait", typeof(Image), typeof(CharacterSelector)).GetComponent<Image>();
            CharacterSelector.Instance.portrait = thething;
            var text = GameObject.Instantiate(___seedInput.transform.parent.Find("PlayStyle").GetComponentInChildren<TextMeshProUGUI>(), thething.transform, false);
            text.rectTransform.anchoredPosition = Vector2.up * 80f;
            text.font = BaldiFonts.ComicSans18.FontAsset();
            text.fontSize = BaldiFonts.ComicSans18.FontSize();
            text.autoSizeTextContainer = true;
            GameObject.Destroy(text.GetComponent<TextLocalizer>());
            CharacterSelector.Instance.text = text;
            thething.sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("Portrait/Fanon");
            thething.transform.SetParent(___seedInput.transform.parent, false);
            thething.rectTransform.anchorMin = new Vector2(1, 0);
            thething.rectTransform.anchorMax = new Vector2(1, 0);
            thething.rectTransform.pivot = new Vector2(1, 0);
            thething.rectTransform.anchoredPosition = new Vector2(-110f, 10f);
            var leftbutton = GameObject.Instantiate(___seedInput.transform.parent.Find("PlayStyle").GetChild(0), thething.transform, false);
            leftbutton.transform.localScale = Vector3.one;
            leftbutton.GetComponent<RectTransform>().anchoredPosition = Vector2.left * 50;
            var but = leftbutton.GetComponent<StandardMenuButton>();
            but.OnPress = new UnityEngine.Events.UnityEvent();
            but.OnPress.AddListener(() => CharacterSelector.Instance.ButtonPress(false));
            but.OnHighlight = new UnityEngine.Events.UnityEvent();
            var rightbutton = GameObject.Instantiate(___seedInput.transform.parent.Find("PlayStyle").GetChild(1), thething.transform, false);
            rightbutton.transform.localScale = Vector3.one;
            rightbutton.GetComponent<RectTransform>().anchoredPosition = Vector2.right * 50;
            but = rightbutton.GetComponent<StandardMenuButton>();
            but.OnPress = new UnityEngine.Events.UnityEvent();
            but.OnPress.AddListener(() => CharacterSelector.Instance.ButtonPress(true));
            but.OnHighlight = new UnityEngine.Events.UnityEvent();
            thething.transform.SetSiblingIndex(thething.transform.GetSiblingIndex() - 1);
            var tooltip = GameObject.Instantiate(___seedInput.transform.parent.Find("TooltipHotspots").GetChild(1), /*___seedInput.transform.parent.Find("TooltipHotspots")*/ thething.transform, false).GetComponent<StandardMenuButton>();
            var recttrans = tooltip.GetComponent<RectTransform>();
            recttrans.anchorMin = Vector2.one/2f;
            recttrans.anchorMax = Vector2.one/2f;
            recttrans.pivot = Vector2.one/2f;
            recttrans.anchoredPosition = text.rectTransform.anchoredPosition;
            recttrans.sizeDelta = new Vector2(130f, 32f);
            var controllert = ___seedInput.transform.parent.GetComponent<TooltipController>();
            tooltip.OnHighlight = new UnityEngine.Events.UnityEvent();
            tooltip.OnHighlight.AddListener(() => controllert.UpdateTooltip(CharacterSelector.Instance.GetDesc()));
            tooltip.OffHighlight = new UnityEngine.Events.UnityEvent();
            tooltip.OffHighlight.AddListener(controllert.CloseTooltip);
        }
    }

    [HarmonyPatch(typeof(BaldiDance), nameof(BaldiDance.CrashSound), [])]
    class WinGameBugfix
    {
        static void Prefix()
        {
            PlayableCharsGame.Character = null;
            PlayableCharsGame.prevSlots = 5;
            PlayableCharsGame.backpackerBackup = new ItemObject[9];
            for (int i = 0; i < PlayableCharsGame.backpackerBackup.Length; i++)
                PlayableCharsGame.backpackerBackup[i] = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
            PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Partygoer");
        }
    }

    /*[HarmonyPatch(typeof(StandardMenuButton))]
    class MenuButtonPatches
    {
        [HarmonyPatch(nameof(StandardMenuButton.Press)), HarmonyPrefix]
        static void Transit(StandardMenuButton __instance)
        {
            if (__instance.OnPress.m_PersistentCalls.GetListeners().ToList().Exists(x => x.methodName == nameof(GameLoader.Initialize)))
            {
                if (__instance.OnPress.m_PersistentCalls.GetListeners().ToList().Exists(x => x.target.name.ToLower() == "pickfieldtrip"))
                {
                    PlayableCharsGame.Character = PlayableCharacterMetaStorage.Instance.All().ToValues().First();
                    SceneManager.LoadSceneAsync("Game");
                    return;
                }
                __instance.transitionOnPress = true;
                __instance.transitionType = UiTransition.SwipeRight;
                __instance.transitionTime = 1.0666667f;
            }
        }
    }*/

    [HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel")]
    class CertainUnlocks
    {
        internal static bool Testy = false;
        static void Prefix(BaseGameManager __instance)
        {
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thebackpacker" && CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>() != null)
                PlayableCharsGame.backpackerBackup = CoreGameManager.Instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>().items;
            if (CoreGameManager.Instance.currentMode == Mode.Free || __instance.levelObject == null) return;
            if (Testy && __instance is MainGameManager)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Test Subject");
            if (CoreGameManager.Instance.GetPlayer(0).itm.items.ToList().FindAll(x => x.itemType == Items.ZestyBar).Count + CoreGameManager.Instance.currentLockerItems.ToList().FindAll(x => x.itemType == Items.ZestyBar).Count >= 3
                && __instance.levelObject.finalLevel)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Troublemaker");
            List<ItemObject[]> backup = (List<ItemObject[]>)CoreGameManager.Instance.ReflectionGetVariable("backupItems");
            float time = (49f + __instance.NotebookTotal + (Mathf.Max(__instance.Ec.levelSize.x, __instance.Ec.levelSize.z) - Mathf.Min(__instance.Ec.levelSize.x, __instance.Ec.levelSize.z)));
#if DEBUG
            string text = Mathf.Floor(time % 60f).ToString();
            if (Mathf.Floor(time % 60f) < 10f)
            {
                text = "0" + Mathf.Floor(time % 60f);
            }

            PlayableCharsPlugin.Log.LogInfo("Required time: " + Mathf.Floor(time / 60f) + ":" + text);
#endif
            if (CoreGameManager.Instance.sceneObject.name == "MainLevel_1" && (float)__instance.ReflectionGetVariable("time") < time && CoreGameManager.Instance.Lives == 2
                && PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") == "thedefault" && PlayableCharsPlugin.Instance.Character.info == PlayableCharsPlugin.Instance.Info
                && backup[0].ToList().TrueForAll(x => x == ItemMetaStorage.Instance.FindByEnum(Items.None).value)
                && __instance.FoundNotebooks == __instance.NotebookTotal)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Speedrunner");
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.BeginPlay))]
    class TestSubjectUnlockBugfix
    {
        static void Postfix(BaseGameManager __instance)
        {
            CertainUnlocks.Testy = __instance.Ec.npcsToSpawn.Exists(x => x.Character == Character.LookAt);
            //Debug.Log("Test Subject unlock method is " + CertainUnlocks.Testy);
        }
    }

    [HarmonyPatch(typeof(Pickup), nameof(Pickup.Clicked))]
    class TinkerneerUnlock
    {
        static void Prefix(int player, Pickup __instance)
        {
            if (CoreGameManager.Instance.currentMode == Mode.Main
                && CoreGameManager.Instance.GetPoints(player) >= __instance.price && !__instance.free && __instance.item == PlayableCharsPlugin.assetMan.Get<ItemObject>("TinkerneerWrench") && __instance.showDescription)
                PlayableCharsPlugin.UnlockCharacter(PlayableCharsPlugin.Instance.Info, "The Tinkerneer");
        }
    }

    /*[HarmonyPatch]
    class CharSelectScreenAdds
    {
        [HarmonyPatch(typeof(ElevatorScreen), "Initialize"), HarmonyPostfix]
        static void AddSelectScreen(ElevatorScreen __instance)
        {
            if (((PlayableCharsPlugin.Instance.Character == null && !ModdedFileManager.Instance.saveData.saveAvailable)
                || (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") && !CoreGameManager.Instance.SaveEnabled && CoreGameManager.Instance.GetPlayer(0) == null))
                && UnityEngine.Object.FindObjectOfType<CharacterSelectScreen>(false) == null)
            {
                __instance.gameObject.SetActive(false);
                UnityEngine.Object.Instantiate(PlayableCharsPlugin.assetMan.Get<GameObject>("CharSelectScreen")).SetActive(true);
            }
        }
        [HarmonyPatch(typeof(GameLoader), "LoadReady"), HarmonyPrefix, HarmonyPriority(Priority.First)]
        static bool DisableSceneChange()
        {
            return !((PlayableCharsPlugin.Instance.Character == null && !ModdedFileManager.Instance.saveData.saveAvailable)
                || (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") && !CoreGameManager.Instance.SaveEnabled && CoreGameManager.Instance.GetPlayer(0) == null));
        }
    }*/

    [HarmonyPatch]
    class PlayerPatches
    {
        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.SpawnPlayers)), HarmonyPrefix, HarmonyPriority(Priority.High)]
        static void ReplacePrefabs(CoreGameManager __instance, ref HudManager ___hudPref, ref HudManager[] ___huds)
        {
            CustomLevelGenerationParameters lvl = BaseGameManager.Instance.levelObject as CustomLevelGenerationParameters;
            if (PlayableCharsPlugin.Instance.Character.componentType == typeof(PlayableRandomizer)
                || ((bool?)lvl?.GetCustomModValue(PlayableCharsPlugin.Instance.Info, "randomizeralways") == true && PlayableCharsPlugin.IsRandom))
                PlayableRandomizer.RandomizePlayable();
            /*if (CoreGameManager.Instance.GetPlayer(0) != null) // Was using this for reiniting the HUD on every next level, but whatever...
                CoreGameManager.Instance.ReflectionInvoke("DestroyPlayers", []);*/
            // Nothing good to do, this will possibly have multiplayer issues.
            __instance.playerPref = PlayableCharsPlugin.Instance.Character.prefab; // If you are looking at this, I will move this to a transpiler if a Multiplayer mod exists. Most playables here uses the default prefab.
            for (int i = 0; i < __instance.setPlayers; i++)
                if (__instance.GetPlayer(i) != null)
                {
                    HudManager oldhud = CoreGameManager.Instance.GetHud(i);
                    ___huds[i] = GameObject.Instantiate(___hudPref);
                    ___huds[i].hudNum = i;
                    if (!PlayerFileManager.Instance.authenticMode)
                        ___huds[i].Canvas().worldCamera = CoreGameManager.Instance.GetCamera(i).canvasCam;
                    GameObject.Destroy(oldhud.gameObject);
                }
        }
        [HarmonyPatch(typeof(CoreGameManager),nameof(CoreGameManager.SpawnPlayers)), HarmonyPostfix]
        static void InitVars(CoreGameManager __instance) // Bugfix with Partygoer's Wrapping Bundles
        {
            for (int i = 0; i < __instance.setPlayers; i++)
            {
                var player = __instance.GetPlayer(i);
                player.gameObject.GetOrAddComponent<PlrPlayableCharacterVars>().Init(player);
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "Start"), HarmonyPrefix, HarmonyPriority(Priority.High)]
        static void PatchEmStats(PlayerManager __instance)
        {
            if (__instance.GetMovementStatModifier() != null)
            {
                var statmodifier = __instance.GetMovementStatModifier();
                statmodifier.ChangeBaseStat("walkSpeed", PlayableCharsPlugin.Instance.Character.walkSpeed);
                statmodifier.ChangeBaseStat("runSpeed", PlayableCharsPlugin.Instance.Character.runSpeed);
                statmodifier.ChangeBaseStat("staminaDrop", PlayableCharsPlugin.Instance.Character.staminaDrop);
                statmodifier.ChangeBaseStat("staminaRise", PlayableCharsPlugin.Instance.Character.staminaRise);
                statmodifier.ChangeBaseStat("staminaMax", PlayableCharsPlugin.Instance.Character.staminaMax);
            }
            __instance.plm.walkSpeed = PlayableCharsPlugin.Instance.Character.walkSpeed;
            __instance.plm.runSpeed = PlayableCharsPlugin.Instance.Character.runSpeed;
            __instance.plm.staminaDrop = PlayableCharsPlugin.Instance.Character.staminaDrop;
            __instance.plm.staminaRise = PlayableCharsPlugin.Instance.Character.staminaRise;
            __instance.plm.staminaMax = PlayableCharsPlugin.Instance.Character.staminaMax;
            CoreGameManager.Instance.GetHud(__instance.playerNumber).transform.Find("Staminometer").gameObject.SetActive(PlayableCharsPlugin.Instance.Character.staminaMax > 0f);
            __instance.plm.stamina = PlayableCharsPlugin.Instance.Character.staminaMax;
            __instance.gameObject.AddComponent(PlayableCharsPlugin.Instance.Character.componentType);
            bool endless = false;
#if !DEMO
            if (Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.arcadeendlessforever"))
                endless = Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.arcadeendlessforever") && EndlessFloorsFuncs.Is99();
#endif
            if (!endless)
            {
                if (!PlayableCharsPlugin.gameStarted)
                    __instance.itm.maxItem = PlayableCharsPlugin.Instance.Character.slots - 1;
                else
                    __instance.itm.maxItem = PlayableCharsGame.prevSlots - 1;
                PlayableCharsGame.prevSlots = __instance.itm.maxItem + 1;
                if (!PlayableCharsPlugin.gameStarted)
                    for (int i = 0; i < PlayableCharsPlugin.Instance.Character.startingItems.Length; i++)
                        __instance.itm.SetItem(PlayableCharsPlugin.Instance.Character.startingItems[i], i);
                CoreGameManager.Instance.GetHud(__instance.playerNumber).UpdateInventorySize(PlayableCharsPlugin.Instance.Character.slots);
            }
            __instance.gameObject.GetComponent<PlayableCharacterComponent>()?.Initialize();
            switch (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", ""))
            {
                default:
                    if (!endless)
                        PlayableCharsPlugin.Instance.Character.OnInitAction?.Invoke(__instance, false);
                    break;
                case "magicalstudent":
                    __instance.itm.LockSlot(0, true);
                    break;
            }
            if (!PlayableCharsPlugin.gameStarted)
                __instance.itm.selectedItem = 0;
            PlayableCharsPlugin.gameStarted = true;
            __instance.itm.UpdateItems();
            if (PlayableCharsPlugin.Instance.Character.name.ToLower().Replace(" ", "") != "thebackpacker" && PlayableCharsGame.backpackerBackup.Any(x => x.itemType != Items.None))
                for (int i = 0; i < PlayableCharsGame.backpackerBackup.Length; i++)
                    PlayableCharsGame.backpackerBackup[i] = ItemMetaStorage.Instance.FindByEnum(Items.None).value;
        }

        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.SaveAndQuit)), HarmonyPrefix]
        static void SaveBackupBackpack(CoreGameManager __instance)
        {
            if (__instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>() != null)
                PlayableCharsGame.backpackerBackup = __instance.GetPlayer(0).gameObject.GetComponent<BackpackerBackpack>().items;
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.RemoveItem)), HarmonyPriority(Priority.HigherThanNormal), HarmonyPrefix] // I don't like you.
        static bool PreventStealing(int val, ItemManager __instance, ref bool[] ___slotLocked)
        {
            if (__instance.items[val].GetMeta().tags.Contains("CharacterItemImportant")
                && __instance.items.Count(x => !x.GetMeta().tags.Contains("CharacterItemImportant")) > 0
                && ___slotLocked[val])
                __instance.RemoveRandomItem();
            return !(__instance.items[val].GetMeta().tags.Contains("CharacterItemImportant") && ___slotLocked[val]);
        }

        [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.AddItem), [typeof(ItemObject)]), HarmonyPriority(Priority.High), HarmonyPrefix] // I hate you.
        static bool PreventSwapWithLocked(ItemObject item, ItemManager __instance, ref bool[] ___slotLocked)
        {
            int num = 0;
            bool flag = false;
            for (int i = 0; i <= __instance.maxItem; i++)
            {
                if (__instance.items[i].itemType == Items.None)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag && __instance.items[__instance.selectedItem].GetMeta().tags.Contains("CharacterItemImportant") && ___slotLocked[__instance.selectedItem])
            {
                while (___slotLocked[num] && num <= __instance.maxItem)
                    num = __instance.items.ToList().FindIndex(num, x => !x.GetMeta().tags.Contains("CharacterItemImportant"));
                __instance.items[num] = item;
                CoreGameManager.Instance.GetHud(__instance.pm.playerNumber).UpdateItemIcon(num, __instance.items[num].itemSpriteSmall);
                CoreGameManager.Instance.GetHud(__instance.pm.playerNumber).inventory.CollectItem(num, item);
                __instance.UpdateSelect();
                return false;
            }
            return true;
        }

        // I removed a backpacker patch because of the override variable that I didn't see.

        [HarmonyPatch(typeof(Bully), nameof(Bully.Initialize)), HarmonyPostfix]
        static void ItemNoLongerBeStolen(ref List<Items> ___itemsToReject)
        {
            foreach (var item in ItemMetaStorage.Instance.FindAllWithTags(false, "CharacterItemImportant").SelectMany(x => x.itemObjects))
                if (!___itemsToReject.Contains(item.itemType)) ___itemsToReject.Add(item.itemType);
        }

        // Useless, but it'll stay because of the Hide and Seek save system...
        [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.RestorePlayers)), HarmonyPrefix]
        static bool ManagerCrashBugfix(CoreGameManager __instance, ref PlayerManager[] ___players, ref List<ItemObject[]> ___backupItems, ref ItemObject[] ___backupLockerItems)
        {
            PlayableCharsPlugin.gameStarted = true;
            return true;
        }

        [HarmonyPatch(typeof(Pickup), nameof(Pickup.Clicked)), HarmonyPrefix, HarmonyPriority(Priority.HigherThanNormal)]
        static bool DoNotPutImportantToLockers(int player, Pickup __instance)
        {
            // If the item is important to the character? (Given at the start of the game and the slot of it is locked...)
            bool[] slotlocked = (bool[])AccessTools.DeclaredField(typeof(ItemManager), "slotLocked").GetValue(CoreGameManager.Instance.GetPlayer(player).itm);
            bool isStorageLocker = (__instance.transform.GetComponentInParent<StorageLocker>() != null
                && CoreGameManager.Instance.GetPlayer(player).itm.items[CoreGameManager.Instance.GetPlayer(player).itm.selectedItem].GetMeta().tags.Contains("CharacterItemImportant")
                && slotlocked[CoreGameManager.Instance.GetPlayer(player).itm.selectedItem] == true);
            if (isStorageLocker)
                CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "ErrorMaybe"));
            return !isStorageLocker;
        }
    }
}
