using BBP_Playables.Core;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BBP_Playables.Modded.BCPP;

public class MainProtagonistHUDInit : PlayableCharacterComponent
{
    private static FieldInfo 
        _itemCoverLeftSprite = AccessTools.DeclaredField(typeof(ItemSlotsManager), "itemCoverLeftSprite"),
        _itemCoverCenterSprite = AccessTools.DeclaredField(typeof(ItemSlotsManager), "itemCoverCenterSprite"),
        _itemCoverRightSprite = AccessTools.DeclaredField(typeof(ItemSlotsManager), "itemCoverRightSprite"),
        _itemBackgrounds = AccessTools.DeclaredField(typeof(HudManager), "itemBackgrounds"),
        _staminaNeedle = AccessTools.DeclaredField(typeof(HudManager), "staminaNeedle"),
        _notebookAnimator = AccessTools.DeclaredField(typeof(HudManager), "notebookAnimator");
    public override void Initialize()
    {
        base.Initialize();
        // HUD CHANGES
        if (PlayerFileManager.Instance.authenticMode || CoreGameManager.Instance.authenticScreen.gameObject.activeSelf
            || !Chainloader.PluginInfos.ContainsKey("alexbw145.baldiplus.bcarnellchars"))
            return;
        ItemSlotsManager inventory = CoreGameManager.Instance.GetHud(pm.playerNumber).inventory;
        _itemCoverLeftSprite.SetValue(inventory, PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_left"));
        _itemCoverCenterSprite.SetValue(inventory, PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_center"));
        _itemCoverRightSprite.SetValue(inventory, PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotsBCMAC_right"));
        inventory.SetSize(pm.itm.items.Length);
        var slotTex = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/ItemSlotBar_BCMAC").texture;
        foreach (RawImage slot in (RawImage[])_itemBackgrounds.GetValue(CoreGameManager.Instance.GetHud(pm.playerNumber)))
        {
            slot.texture = slotTex;
            slot.SetNativeSize();
        }
        inventory.transform.Find("Covers").GetComponent<RectTransform>().anchoredPosition += (Vector2.up + Vector2.right) * -1f;
        inventory.transform.parent.Find("Item Text").GetComponent<RectTransform>().anchoredPosition += Vector2.down * 2f;
        RectTransform stamino = (RectTransform)_staminaNeedle.GetValue(CoreGameManager.Instance.GetHud(pm.playerNumber));
        stamino = stamino.parent as RectTransform;
        stamino.anchorMin = stamino.anchorMax = stamino.pivot = Vector2.up;
        stamino.anchoredPosition = new Vector2(-5f, -55f);
        stamino.GetComponentsInChildren<Image>()[0].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaGradientBCMAC");
        stamino.GetComponentsInChildren<Image>()[0].rectTransform.anchoredPosition = new Vector2(10.7f, 13f);
        stamino.GetComponentsInChildren<Image>()[0].rectTransform.offsetMax = new Vector2(160f, 35f);
        stamino.GetComponentsInChildren<Image>()[1].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaPointBCMAC");
        stamino.GetComponentsInChildren<Image>()[2].sprite = PlayableCharsPlugin.assetMan.Get<Sprite>("HUD/StaminaBarBCMAC");
        var noteboAnim = (Animator)_notebookAnimator.GetValue(CoreGameManager.Instance.GetHud(pm.playerNumber));
        RectTransform notebooks = noteboAnim.transform as RectTransform;
        notebooks.anchorMin = notebooks.anchorMax = notebooks.pivot = notebooks.anchoredPosition = Vector2.zero;
        RectTransform nbText = notebooks.parent.Find("Notebook Text") as RectTransform;
        nbText.anchorMin = nbText.anchorMax = nbText.pivot = Vector2.zero;
        nbText.anchoredPosition = new Vector2(50, 16);
        RectTransform tv = notebooks.parent.Find("BaldiTV") as RectTransform;
        tv.anchorMin = tv.anchorMax = tv.pivot = new Vector2(0.5f, 1);
        var skew = tv.Find("TMPSkewParent");
        skew.localPosition = new(4.32f, skew.localPosition.y, skew.localPosition.z);
        tv.anchoredPosition = Vector2.zero;
    }
}
