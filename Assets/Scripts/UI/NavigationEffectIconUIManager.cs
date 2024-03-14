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
    public class NavigationEffectIconUIManager : MonoBehaviour
    {
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

        private bool needNavigationEffectUpdate;
        private bool needIconRequestedUpdate;

        private Texture displayedNavigationEffect;
        private Texture displayedNavigationRequest;

        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                transform.localPosition = new Vector3(-100, -140, -500);
            }
            else {
                transform.localPosition = new Vector3(-100, -140, -500);
            }

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnRequestNavigationEffect.AddListener(ShowInfoMessage);
            
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(ShowIcon);
            robotStatus.event_OnStopTeleoperation.AddListener(HideIcon);

            HideIcon();
        }
        
        void Update()
        {
            if(needIconRequestedUpdate)
            {
                navigationEffectIconPanel.GetChild(1).GetComponent<RawImage>().texture = displayedNavigationEffect;
                navigationEffectIconPanel.GetChild(2).GetComponent<RawImage>().texture = displayedNavigationRequest;
                if(motionSicknessManager.IsNavigationEffectOnDemand)
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
                        displayedNavigationRequest = requestedIcon;
                    }
                    else 
                    {
                        displayedNavigationRequest = notRequestedIcon;
                    }
                }
                else if(motionSicknessManager.IsReducedScreenOn) 
                {
                    if(motionSicknessManager.RequestNavigationEffect)
                    {
                        displayedNavigationRequest = requestedIcon;
                    }
                    else
                    {
                        displayedNavigationRequest = notRequestedIcon;
                    }
                }
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

        void HideIcon()
        {
            navigationEffectIconPanel.ActivateChildren(false);
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }
    }
}
