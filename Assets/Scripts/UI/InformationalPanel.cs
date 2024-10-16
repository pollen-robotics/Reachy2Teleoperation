using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class InformationalPanel : CustomLazyFollowUI
    {
        [SerializeField]
        private Text infoText;

        private Coroutine infoPanelDisplay;

        private bool needInfoPanelUpdate;

        protected string textToDisplay;

        private int displayDuration = 3;

        protected void SetMinimumTimeDisplayed(int seconds)
        {
            displayDuration = seconds;
        }

        void Update()
        {
            if (needInfoPanelUpdate)
            {
                if (infoPanelDisplay != null) StopCoroutine(infoPanelDisplay);
                transform.ActivateChildren(true);
                infoText.text = textToDisplay;
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
