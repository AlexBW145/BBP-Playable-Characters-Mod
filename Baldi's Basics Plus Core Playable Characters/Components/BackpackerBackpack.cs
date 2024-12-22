using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BackpackerBackpack : MonoBehaviour
    {
        private PlayerManager pm;
        private ValueModifier modifier = new ValueModifier(1f);
        void Start()
        {
            pm = gameObject.GetComponent<PlayerManager>();
            if (pm != null)
            {
                for (int i = 0; i < items.Length; i++)
                    if (items[i] == null)
                        items[i] = pm.itm.nothing;
                items[pm.itm.maxItem] = PlayableCharsPlugin.assetMan.Get<ItemObject>("BackpackOpen");
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("walkSpeed", modifier);
                pm.GetComponent<PlayerMovementStatModifier>().AddModifier("runSpeed", modifier);
            }
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
                modifier.multiplier = Mathf.Max(0.2f,
                    1.5f -
                    Mathf.Abs((items.Count(x => x.itemType != Items.None && x.itemType != EnumExtensions.GetFromExtendedName<Items>("BackpackerBackpack"))
                    + pm.itm.items.Count(x => x.itemType != Items.None && x.itemType != EnumExtensions.GetFromExtendedName<Items>("BackpackerBackpack")))
                    / 16f));
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
