using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Mobile.Base.Lidar;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class StartArmTeleoperationUIManager : LazyFollow
    {
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private ControllersManager controllers;
        private UserEmergencyStopInput userEmergencyStop;

        private bool needUpdateInfoMessage;
        private bool wantInfoMessageDisplayed;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, -0.1f, 0.8f);
            }
            else {
                targetOffset = new Vector3(0, -0.1f, 0.7f);
            }
            maxDistanceAllowed = 0;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            robotStatus.event_OnStartTeleoperation.AddListener(ShowInfoMessage);
            robotStatus.event_OnStartArmTeleoperation.AddListener(HideInfoMessage);
            robotStatus.event_OnStopTeleoperation.AddListener(HideInfoMessage);

            needUpdateInfoMessage = false;
            wantInfoMessageDisplayed = false;

            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);

            transform.ActivateChildren(false);
        }

        void Init()
        {
            userEmergencyStop = UserInputManager.Instance.UserEmergencyStopInput;
            userEmergencyStop.event_OnEmergencyStopCalled.AddListener(HideInfoMessage);
        }

        void ShowInfoMessage()
        {
            wantInfoMessageDisplayed = true;
            needUpdateInfoMessage = true;
        }

        void Update()
        {
            if(needUpdateInfoMessage)
            {
                transform.ActivateChildren(wantInfoMessageDisplayed);
                needUpdateInfoMessage = false;
            }
        }

        void HideInfoMessage()
        {
            wantInfoMessageDisplayed = false;
            needUpdateInfoMessage = true;
        }
    }
}