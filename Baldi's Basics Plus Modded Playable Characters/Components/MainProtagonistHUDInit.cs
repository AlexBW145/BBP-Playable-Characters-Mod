using BBP_Playables.Core;
using BepInEx.Bootstrap;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BBP_Playables.Modded.BCPP
{
    public class MainProtagonistHUDInit : PlayableCharacterComponent
    {
        public override void Initialize(BaseGameManager manager)
        {
            // HUD CHANGES
            if (PlayerFileManager.Instance.authenticMode || CoreGameManager.Instance.authenticScreen.gameObject.activeSelf
                || !Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
                return;
            ItemSlotsManager inventory = CoreGameManager.Instance.GetHud(pm.playerNumber).inventory;
            inventory.ReflectionSetVariable("itemCoverLeftSprite", PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_left"));
            inventory.ReflectionSetVariable("itemCoverCenterSprite", PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_center"));
            inventory.ReflectionSetVariable("itemCoverRightSprite", PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_right"));
            inventory.SetSize(pm.itm.items.Length);
            foreach (RawImage slot in (RawImage[])CoreGameManager.Instance.GetHud(pm.playerNumber).ReflectionGetVariable("itemBackgrounds"))
            {
                slot.texture = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotBar_BCMAC").texture;
                slot.SetNativeSize();
            }
            RectTransform stamino = (RectTransform)CoreGameManager.Instance.GetHud(pm.playerNumber).ReflectionGetVariable("staminaNeedle");
            stamino = stamino.parent as RectTransform;
            stamino.anchorMin = Vector2.up;
            stamino.anchorMax = Vector2.up;
            stamino.pivot = Vector2.up;
            stamino.anchoredPosition = new Vector2(-5f, -55f);
            stamino.GetComponentsInChildren<Image>()[0].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaGradientBCMAC");
            stamino.GetComponentsInChildren<Image>()[0].rectTransform.anchoredPosition = new Vector2(10.7f, 13f);
            stamino.GetComponentsInChildren<Image>()[0].rectTransform.offsetMax = new Vector2(160f, 35f);
            stamino.GetComponentsInChildren<Image>()[1].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaPointBCMAC");
            stamino.GetComponentsInChildren<Image>()[2].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaBarBCMAC");
            var noteboAnim = (Animator)CoreGameManager.Instance.GetHud(pm.playerNumber).ReflectionGetVariable("notebookAnimator");
            RectTransform notebooks = noteboAnim.transform as RectTransform;
            notebooks.anchorMin = Vector2.zero;
            notebooks.anchorMax = Vector2.zero;
            notebooks.pivot = Vector2.zero;
            notebooks.anchoredPosition = Vector2.zero;
            RectTransform nbText = notebooks.parent.Find("Notebook Text") as RectTransform;
            nbText.anchorMin = Vector2.zero;
            nbText.anchorMax = Vector2.zero;
            nbText.pivot = Vector2.zero;
            nbText.anchoredPosition = new Vector2(50, 16);
            RectTransform tv = notebooks.parent.Find("BaldiTV") as RectTransform;
            tv.anchorMin = new Vector2(0.5f, 1);
            tv.anchorMax = new Vector2(0.5f, 1);
            tv.pivot = new Vector2(0.5f, 1);
            var skew = tv.Find("TMPSkewParent");
            skew.localPosition = new(4.32f, skew.localPosition.y, skew.localPosition.z);
            tv.anchoredPosition = Vector2.zero;
        }
    }
}
