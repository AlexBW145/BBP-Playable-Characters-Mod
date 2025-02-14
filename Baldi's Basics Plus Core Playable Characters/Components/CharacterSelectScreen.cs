using BBP_Playables.Core.Patches;
using BepInEx.Bootstrap;
using MTM101BaldAPI.Registers;
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
            characters = PlayableCharacterMetaStorage.Instance.FindAll(x => x.value.unlocked).ToValues().ToList();
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
    [RequireComponent(typeof(Image))]
    public class CharacterSelector : MonoBehaviour
    {
        private int curChar = 0;
        public Image portrait;
        public TextMeshProUGUI text;
        private List<PlayableCharacter> characters = new List<PlayableCharacter>();
        public static CharacterSelector Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            characters = PlayableCharacterMetaStorage.Instance.FindAll(x => x.value.unlocked).ToValues().ToList();
            curChar = characters.IndexOf(PlayableCharsPlugin.Instance.extraSave.Item1);
            if (curChar < 0) curChar = 0;
            UpdateSelection();
        }

        public string GetDesc() => characters[curChar].description;

        internal void ButtonPress(bool forward = true)
        {
            if (forward)
                curChar++;
            else
                curChar--;
            if (curChar > characters.Count - 1 || curChar < 0)
                curChar = curChar < 0 ? characters.Count - 1 : 0;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            PlayableCharsPlugin.Instance.extraSave = new(characters[curChar]);
            portrait.sprite = characters[curChar].sprselect;
            text.text = characters[curChar].name;
        }

        internal void SetValues()
        {
            PlayableCharsGame.Character = characters[curChar];
            PlayableCharsPlugin.Instance.extraSave = new(characters[curChar]);
        }
    }
}
