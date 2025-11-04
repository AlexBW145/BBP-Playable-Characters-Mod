using MTM101BaldAPI.Components;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BBP_Playables.Core
{
    public class PlayableUnlockedUI : MonoBehaviour
    {
        [SerializeField] internal Image portraitMan;
        [SerializeField] internal TMP_Text textMan;
        [SerializeField] internal CustomImageAnimator animatorMan;
        [SerializeField] internal Sprite[] tvsprites;

        internal void DoThang()
        {
            textMan.text = LocalizationManager.Instance.GetLocalizedText("Hud_UnlockedPlayableCharacter");
            animatorMan.animations.Add("SwingIn", new CustomAnimation<Sprite>(tvsprites.Reverse().ToArray(), 1f));
            animatorMan.animations.Add("SwingOut", new CustomAnimation<Sprite>(tvsprites, 1f));
            animatorMan.animations.Add("SwingIdle", new CustomAnimation<Sprite>([tvsprites.First()], 1f));

            portraitMan.gameObject.SetActive(false);
            textMan.gameObject.SetActive(false);
            animatorMan.SetDefaultAnimation("SwingIdle", 1f);
            StartCoroutine(AnimationThing());
        }

        private IEnumerator AnimationThing()
        {
            animatorMan.Play("SwingIn", 0.5f);
            while (animatorMan.currentAnimationName == "SwingIn")
                yield return null;
            portraitMan.gameObject.SetActive(true);
            textMan.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(7f);
            portraitMan.gameObject.SetActive(false);
            textMan.gameObject.SetActive(false);
            animatorMan.Play("SwingOut", 0.5f);
            while (animatorMan.currentAnimationName == "SwingOut")
                yield return null;
            Destroy(gameObject);
            yield break;
        }
    }
}
