using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class NavigationEffectUIManager : InformationalPanel
    {
        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.27f, 0.8f));

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnRequestNavigationEffect.AddListener(ChooseMessageAndDisplay);

            HideInfoMessage();
        }

        void ChooseMessageAndDisplay(bool activate)
        {
            if (motionSicknessManager.IsTunnellingOnClickOn)
            {
                if (motionSicknessManager.RequestNavigationEffect)
                {
                    textToDisplay = "Activate tunnelling";
                }
                else
                {
                    textToDisplay = "Deactivate tunnelling";
                }
            }
            else if (motionSicknessManager.IsReducedScreenOnClickOn)
            {
                if (motionSicknessManager.RequestNavigationEffect)
                {
                    textToDisplay = "Activate reduced screen";
                }
                else
                {
                    textToDisplay = "Deactivate reduced screen";
                }
            }
            ShowInfoMessage();
        }
    }
}
