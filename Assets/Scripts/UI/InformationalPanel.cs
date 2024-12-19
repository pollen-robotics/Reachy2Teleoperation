using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class InformationalPanel : CustomLazyFollowUI
    {
        [SerializeField]
        protected Text infoText;

        [SerializeField]
        protected Image infoBackground;

        protected Coroutine infoPanelDisplay;

        protected bool needInfoPanelUpdate;

        protected string textToDisplay;

        protected int displayDuration = 3;

        protected Color32 backgroundColor = ColorsManager.error_black;

        protected virtual void SetMinimumTimeDisplayed(int seconds)
        {
            displayDuration = seconds;
        }

        protected virtual void Update()
        {
            if (needInfoPanelUpdate)
            {
                if (infoPanelDisplay != null) StopCoroutine(infoPanelDisplay);
                transform.ActivateChildren(true);
                infoText.text = textToDisplay;
                if (infoBackground != null) infoBackground.color = backgroundColor;
                infoPanelDisplay = StartCoroutine(HidePanelAfterSeconds(displayDuration, transform));
                
                needInfoPanelUpdate = false;
            }
        }

        protected virtual void ShowInfoMessage()
        {
            needInfoPanelUpdate = true;
        }

        protected virtual void HideInfoMessage()
        {
            if (infoPanelDisplay != null) StopCoroutine(infoPanelDisplay);
            transform.ActivateChildren(false);
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }
    }
}
