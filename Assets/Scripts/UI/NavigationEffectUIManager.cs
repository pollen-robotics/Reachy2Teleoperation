using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class NavigationEffectUIManager : LazyFollow
    {
        [SerializeField]
        private Transform navigationEffectInfoPanel;

        private RobotStatus robotStatus;
        private MotionSicknessManager motionSicknessManager;

        private Coroutine navigationEffectPanelDisplay;

        private bool needNavigationEffectUpdate;

        private string navigationEffectText;
        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, -0.27f, 0.8f);
            }
            else {
                targetOffset = new Vector3(0, -0.27f, 0.7f);
            }
            maxDistanceAllowed = 0;

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnRequestNavigationEffect.AddListener(ShowInfoMessage);
            
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(HideInfoMessage);

            HideInfoMessage();
        }
        
        void Update()
        {
            if(needNavigationEffectUpdate)
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
                if(motionSicknessManager.IsTunnellingOn) 
                {
                    if(motionSicknessManager.RequestNavigationEffect)
                    {
                        navigationEffectText = "Activate tunnelling";
                    }
                    else 
                    {
                        navigationEffectText = "Deactivate tunnelling";
                    }
                }
                else if(motionSicknessManager.IsReducedScreenOn) 
                {
                    if(motionSicknessManager.RequestNavigationEffect)
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
