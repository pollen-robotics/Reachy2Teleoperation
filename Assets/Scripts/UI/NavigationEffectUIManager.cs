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

        [SerializeField]
        private Transform navigationEffectIconPanel;

        [SerializeField]
        private Texture tunnellingIcon;

        [SerializeField]
        private Texture reducedScreenIcon;

        [SerializeField]
        private Texture requestedIcon;

        [SerializeField]
        private Texture notRequestedIcon;

        private RobotStatus robotStatus;
        private MotionSicknessManager motionSicknessManager;

        private Coroutine navigationEffectPanelDisplay;

        private bool needNavigationEffectUpdate;
        private bool needIconRequestedUpdate;

        private Texture displayedNavigationEffect;
        private Texture displayedNavigationRequest;

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
            robotStatus.event_OnStartTeleoperation.AddListener(ShowIcon);
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

            if(needIconRequestedUpdate)
            {
                navigationEffectIconPanel.GetChild(1).GetComponent<RawImage>().texture = displayedNavigationEffect;
                navigationEffectIconPanel.GetChild(2).GetComponent<RawImage>().texture = displayedNavigationRequest;
                if(motionSicknessManager.RequestNavigationEffect)
                {
                    navigationEffectIconPanel.ActivateChildren(true);
                }
                needIconRequestedUpdate = false;
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
                        displayedNavigationRequest = requestedIcon;
                    }
                    else 
                    {
                        navigationEffectText = "Deactivate tunnelling";
                        displayedNavigationRequest = notRequestedIcon;
                    }
                }
                else if(motionSicknessManager.IsReducedScreenOn) 
                {
                    if(motionSicknessManager.RequestNavigationEffect)
                    {
                        navigationEffectText = "Activate reduced screen";
                        displayedNavigationRequest = requestedIcon;
                    }
                    else
                    {
                        navigationEffectText = "Deactivate reduced screen";
                        displayedNavigationRequest = notRequestedIcon;
                    }
                }
                needNavigationEffectUpdate = true;
                needIconRequestedUpdate = true;
            }
        }

        void ShowIcon()
        {
            if(motionSicknessManager.IsTunnellingOn)
            {
                displayedNavigationEffect = tunnellingIcon;
            }
            else if (motionSicknessManager.IsReducedScreenOn)
            {
                displayedNavigationEffect = reducedScreenIcon;
            }

            if(motionSicknessManager.RequestNavigationEffect)
            {
                displayedNavigationRequest = requestedIcon;
            }
            else 
            {
                displayedNavigationRequest = notRequestedIcon;
            }

            needIconRequestedUpdate = true;
        }

        void HideInfoMessage()
        {
            if (navigationEffectPanelDisplay != null) StopCoroutine(navigationEffectPanelDisplay);
            navigationEffectInfoPanel.ActivateChildren(false);
            navigationEffectIconPanel.ActivateChildren(false);
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }
    }
}
