﻿using BBP_Playables.Core.Patches;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BBP_Playables.Core
{
    public class CharacterSelectScreen : MonoBehaviour
    {
        private int curChar = 0;
        private bool selected = false;
        public bool Loading => selected;
        public TextMeshProUGUI nametext;
        public TextMeshProUGUI desctext;
        public Image portrait;
        private List<PlayableCharacter> characters = new List<PlayableCharacter>();
        private AnalogInputData analogData;
        private Vector2 _absoluteVector;
        private Vector2 _deltaVector;
        private bool inputDown = false;


        void OnEnable()
        {
            analogData = Resources.FindObjectsOfTypeAll<PlayerMovement>().First().movementAnalogData;
            characters = PlayableCharsPlugin.characters.FindAll(x => x.unlocked);
            UpdateSelection();
            InputManager.Instance.ActivateActionSet("InGame");
#if DEBUG
            MusicManager.Instance.PlayMidi(
                Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") ? PlayableCharsPlugin.assetMan.Get<string>("charSelINF") :
                PlayableCharsPlugin.assetMan.Get<string>("charSel"), true);
#endif
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") && GameObject.Find("PickMode") != null)
            {
                GlobalCam.Instance.Transition(UiTransition.SwipeRight, 1.0666667f);
                GameObject.Find("PickMode").SetActive(false);
            }
        }

        void Update()
        {
            if (selected && characters.Count <= 0)
                return;
            if (analogData != null) InputManager.Instance.GetAnalogInput(analogData, out _absoluteVector, out _deltaVector, 0.1f);
            if (!inputDown)
            {
                if (_absoluteVector.x > 0.25f)
                    curChar++;
                else if (_absoluteVector.x < -0.25f)
                    curChar--;
                if (_absoluteVector.x > 0.25f || _absoluteVector.x < -0.25f)
                {
                    if (curChar > characters.Count - 1 || curChar < 0)
                        curChar = curChar < 0 ? characters.Count - 1 : 0;
                    UpdateSelection();
                }
            }
            inputDown = _absoluteVector.x != 0f;
            if ((InputManager.Instance.GetDigitalInput("UseItem", true) || InputManager.Instance.GetDigitalInput("Interact", true))
                && !GlobalCam.Instance.TransitionActive)
            {
                selected = true;
                MusicManager.Instance.StopMidi();
                PlayableCharsGame.Character = characters[curChar];
                ElevatorScreen.Instance.gameObject.SetActive(true);
                InputManager.Instance.ActivateActionSet("Interface");
                SceneManager.LoadSceneAsync("Game");
            }
        }

        private void UpdateSelection()
        {
            nametext.text = LocalizationManager.Instance.GetLocalizedText(characters[curChar].name);
            desctext.text = LocalizationManager.Instance.GetLocalizedText(characters[curChar].description);
            portrait.sprite = characters[curChar].sprselect;
        }
    }
}