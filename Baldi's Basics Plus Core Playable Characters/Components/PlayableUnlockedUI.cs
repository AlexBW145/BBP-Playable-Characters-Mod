using MTM101BaldAPI.Components.Animation;
using System.Collections;
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

        internal void DoThang()
        {
            textMan.text = LocalizationManager.Instance.GetLocalizedText("Hud_UnlockedPlayableCharacter");

            portraitMan.gameObject.SetActive(false);
            textMan.gameObject.SetActive(false);
            animatorMan.SetDefaultAnimation("SwingIdle", 1f);
            StartCoroutine(AnimationThing());
        }

        private IEnumerator AnimationThing()
        {
            animatorMan.Play("SwingIn", 1.25f);
            while (animatorMan.AnimationId == "SwingIn")
                yield return null;
            portraitMan.gameObject.SetActive(true);
            textMan.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(5f);
            portraitMan.gameObject.SetActive(false);
            textMan.gameObject.SetActive(false);
            animatorMan.Play("SwingOut", 1.25f);
            while (animatorMan.AnimationId == "SwingOut")
                yield return null;
            Destroy(gameObject);
            yield break;
        }
    }
}
