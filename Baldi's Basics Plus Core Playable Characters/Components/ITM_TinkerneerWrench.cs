using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
#if !DEMO
namespace BBP_Playables.Core
{
    public class ITM_TinkerneerWrench : Item
    {
        public static SoundObject[] tinkerneeringSnds;
        public static Dictionary<string, TinkerneerObject> TinkerneerObjectsPre = new Dictionary<string, TinkerneerObject>();
        public Canvas hudPre;
        private List<TinkerneerObject> availableTinkers = new List<TinkerneerObject>();
        private Vector3 cellPos;
        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            var cell = pm.ec.CellFromPosition(pm.transform.position + CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward * 5f);
            hudPre.GetComponentInChildren<TextMeshProUGUI>().text = "Inventions:";
            if (cell != null && Physics.Raycast(pm.transform.position, cell.CenterWorldPosition - pm.transform.position, pm.ec.MaxRaycast, LayerMask.GetMask("Default", "Block Raycast", "Windows")))
            {
                cellPos = cell.CenterWorldPosition;
                if (!Physics.OverlapBox(cellPos, Vector3.one * 5f).ToList().Exists(s => s.gameObject.GetComponent<TinkerneerObject>() || s.gameObject.GetComponent<EnvironmentObject>() || s.gameObject.GetComponent<NavMeshObstacle>()))
                {
                    foreach (var tinkerobject in TinkerneerObjectsPre)
                    {
                        List<ItemObject> requiredItems = tinkerobject.Value.requiredItems.ToList();
                        if (!HasRequiredItems(requiredItems) || !TinkerneerExclusive(tinkerobject.Value) || !tinkerobject.Value.AppropriateLocation(cell) || availableTinkers.Count > 6 ||
                            (tinkerobject.Value.rm != RoomCategory.Null && tinkerobject.Value.rm != cell.room.category))
                            continue;
                        availableTinkers.Add(tinkerobject.Value);
                        hudPre.GetComponentInChildren<TextMeshProUGUI>().text += ("\n({0}) " + tinkerobject.Value.name).Replace("{0}", availableTinkers.Count <= 6 ? InputManager.Instance.GetInputButtonName("Item" + availableTinkers.Count, "InGame", false) : "OUT OF REACH!");
                    }
                    if (availableTinkers.Count > 0)
                    {
                        hudPre.worldCamera = CoreGameManager.Instance.GetCamera(pm.playerNumber).canvasCam;
                        hudPre.gameObject.SetActive(true);
                        CoreGameManager.Instance.Pause(false);
                        return false;
                    }
                }
            }
            CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "ErrorMaybe"));
            Destroy(gameObject);
            return false;
        }

        private bool TinkerneerExclusive(TinkerneerObject tinkerobject) => (PlrPlayableCharacterVars.GetLocalPlayable()?.GetCurrentPlayable().name.ToLower().Replace(" ", "") == "thetinkerneer" && tinkerobject.tinkerneerCharExclusive) || !tinkerobject.tinkerneerCharExclusive;

        private bool HasRequiredItems(List<ItemObject> items)
        {
            foreach (var item in pm.itm.items)
                if (items.Contains(item))
                    items.Remove(item);
            return items.Count <= 0;
        }

        public void Tinker(TinkerneerObject tinkerobject, Vector3 pos)
        {
            CoreGameManager.Instance.audMan.PlayRandomAudio(tinkerneeringSnds);
            Instantiate(tinkerobject.gameObject, pos, tinkerobject.transform.rotation, pm.ec.transform).GetComponent<TinkerneerObject>().Create(pm.itm);
            List<ItemObject> itemsToTakeAway = tinkerobject.requiredItems.ToList(); // So that multiple items of the same won't be taken away.
            foreach (var item in pm.itm.items)
                if (itemsToTakeAway.Contains(item))
                {
                    pm.itm.RemoveItem(pm.itm.items.ToList().FindIndex(x => x == item));
                    itemsToTakeAway.Remove(item);
                }
            StopThinking(false);
        }

        private void StopThinking(bool error = true)
        {
            Time.timeScale = 1f;
            CoreGameManager.Instance.Pause(false);
            Destroy(gameObject);
            if (error) CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "ErrorMaybe"));
        }

        void Update()
        {
            if (availableTinkers.Count > 0)
            {
                if (CoreGameManager.Instance.GetCamera(pm.playerNumber).camCom.cullingMask == 0
                    || CoreGameManager.Instance.GetCamera(pm.playerNumber).billboardCam.cullingMask == 0
                    || CoreGameManager.Instance.GetCamera(pm.playerNumber).canvasCam.cullingMask == 0)
                    CoreGameManager.Instance.GetCamera(pm.playerNumber).StopRendering(false);
                else if (!CoreGameManager.Instance.Paused && gameObject.activeSelf)
                    StopThinking();
                for (int i = 0; i < availableTinkers.Count; i++)
                {
                    if (InputManager.Instance.GetDigitalInput("Item" + (i + 1), true))
                        Tinker(availableTinkers[i], cellPos);
                }
            }
        }
    }

    public class TinkerneerObject : MonoBehaviour
    {
        public ItemObject[] requiredItems = [];
        public string desc = "";
        public bool tinkerneerCharExclusive = true;
        public RoomCategory rm = RoomCategory.Null;
        public BepInEx.PluginInfo Info;

        public virtual void Create(ItemManager itm)
        {
        }

        public virtual bool AppropriateLocation(Cell cell) => !cell.Null;
    }
}
#endif