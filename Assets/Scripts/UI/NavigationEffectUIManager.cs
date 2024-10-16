using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class NavigationEffectUIManager : CustomLazyFollowUI
    {
        [SerializeField]
        private Transform navigationEffectInfoPanel;

        private RobotStatus robotStatus;
        private MotionSicknessManager motionSicknessManager;

        private Coroutine navigationEffectPanelDisplay;

        private bool needNavigationEffectUpdate;

        private string navigationEffectText;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.27f, 0.8f));

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnRequestNavigationEffect.AddListener(ShowInfoMessage);

            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideInfoMessage);


            HideInfoMessage();
        }

        void Update()
        {
            if (needNavigationEffectUpdate)
            {
                if (navigationEffectPanelDisplay != null) StopCoroutine(navigationEffectPanelDisplay);
                navigationEffectInfoPanel.ActivateChildren(true);
                navigationEffectInfoPanel.GetChild(1).GetComponent<Text>().text = navigationEffectText;
                navigationEffectPanelDisplay = StartCoroutine(HidePanelAfterSeconds(3, navigationEffectInfoPanel));

                needNavigationEffectUpdate = false;
            }
        }

        void ShowInfoMessage(bool activate)
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                if (motionSicknessManager.IsTunnellingOnClickOn)
                {
                    if (motionSicknessManager.RequestNavigationEffect)
                    {
                        navigationEffectText = "Activate tunnelling";
                    }
                    else
                    {
                        navigationEffectText = "Deactivate tunnelling";
                    }
                }
                else if (motionSicknessManager.IsReducedScreenOnClickOn)
                {
                    if (motionSicknessManager.RequestNavigationEffect)
                    {
                        navigationEffectText = "Activate reduced screen";
                    }
                    else
                    {
                        navigationEffectText = "Deactivate reduced screen";
                    }
                }
                needNavigationEffectUpdate = true;
            }
        }

        void HideInfoMessage()
        {
            if (navigationEffectPanelDisplay != null) StopCoroutine(navigationEffectPanelDisplay);
            navigationEffectInfoPanel.ActivateChildren(false);
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }
    }
}
