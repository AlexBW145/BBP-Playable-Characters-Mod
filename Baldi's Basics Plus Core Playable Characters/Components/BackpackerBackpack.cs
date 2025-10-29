using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BBP_Playables.Core
{
    public class ITM_BackpackerBackpack : Item
    {
        public BackpackerBackpack backpack;
        public override bool Use(PlayerManager pm)
        {
            backpack = pm.gameObject.GetComponent<BackpackerBackpack>();
            if (backpack != null)
                backpack.Open();
            Destroy(gameObject);
            return false;
        }
    }

    [RequireComponent(typeof(PlayerManager), typeof(ItemManager), typeof(PlayerMovement))]
    public class BackpackerBackpack : PlayableCharacterComponent
    {
        private ValueModifier modifier = new ValueModifier(1f);
        private Items backpackEnum;
        protected override void Start()
        {
            base.Start();
            if (pm != null)
            {
                pm.itm.LockSlot(pm.itm.maxItem, true); // Is it locking??
                items[pm.itm.maxItem] = PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackOpen");
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("walkSpeed", modifier);
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("runSpeed", modifier);
            }
        }
        private static FieldInfo restoreItmsOnSpawn = AccessTools.DeclaredField(typeof(CoreGameManager), "restoreItemsOnSpawn");
        public override void Initialize()
        {
            base.Initialize();
            backpackEnum = EnumExtensions.GetFromExtendedName<Items>("BackpackerBackpack");
            for (int i = 0; i < items.Length; i++)
                if (items[i] == null)
                    items[i] = pm.itm.nothing;
            pm.itm.SetItem(PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackClosed"), pm.itm.maxItem);
            pm.itm.LockSlot(pm.itm.maxItem, true);
            if ((bool)restoreItmsOnSpawn.GetValue(CoreGameManager.Instance) || PlayableCharsPlugin.gameStarted)
                gameObject.GetComponent<BackpackerBackpack>().items = PlayableCharsGame.backpackerBackup;
        }

        private bool open = false;
        public ItemObject[] items = new ItemObject[9];
        public void Open()
        {
            if (pm == null) return;
            open = !open;
            ItemObject[] prevItems = pm.itm.items;
            pm.itm.items = items;
            items = prevItems;
            pm.itm.UpdateItems();
            if (open)
            {
                CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "GrappleClang"));
            }
            else
            {
                CoreGameManager.Instance.audMan.PlaySingle(Resources.FindObjectsOfTypeAll<SoundObject>().ToList().Find(x => x.name == "GrappleLaunch"));
            }
        }

        void Update()
        {
            if (pm != null)
            {
                if (pm.itm.maxItem == 0)
                {
                    modifier.multiplier = 1f;
                    return;
                }
                modifier.multiplier = Mathf.Max(0.2f,
                    1.5f -
                    Mathf.Abs((items.Count(x => x.itemType != Items.None && x.itemType != backpackEnum)
                    + pm.itm.items.Count(x => x.itemType != Items.None && x.itemType != backpackEnum))
                    / (float)((pm.itm.maxItem - 1f) * 2f))); // There are 16 available slots...
            }
        }

        void OnDisable()
        {
            if (pm != null && pm.GetComponent<PlayerMovementStatModifier>().modifiers.Values.ToList().Exists(x => x.Contains(modifier)))
                pm.GetComponent<PlayerMovementStatModifier>().RemoveModifier(modifier);
        }

        void OnEnable()
        {
            if (pm != null && !pm.GetComponent<PlayerMovementStatModifier>().modifiers.Values.ToList().Exists(x => x.Contains(modifier))) {
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("walkSpeed", modifier);
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("runSpeed", modifier);
            }
        }
    }
}
