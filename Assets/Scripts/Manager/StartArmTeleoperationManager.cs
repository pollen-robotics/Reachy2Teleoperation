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
    public class StartArmTeleoperationManager : LazyFollow
    {
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private ControllersManager controllers;

        private bool needUpdateInfoMessage;
        private bool wantInfoMessageDisplayed;

        [SerializeField]
        private Button backToTransitionRoomButton;

        bool rightPrimaryButtonPressed = false;
        bool rightPrimaryButtonPreviouslyPressed = false;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            backToTransitionRoomButton.onClick.AddListener(BackToTransitionRoom);
        }

        void Update()
        {
            bool rightPrimaryButtonPressed = false;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            if(robotStatus.IsRobotTeleoperationActive() && !robotStatus.IsRobotArmTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                if ((!robotConfig.HasLeftArm() || !robotStatus.IsLeftArmOn()) && (!robotConfig.HasRightArm() || !robotStatus.IsRightArmOn()))
                {
                    robotStatus.StartArmTeleoperation();
                }
                else if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
                {
                    robotStatus.StartArmTeleoperation();
                }
                
            }

            rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
        }

        void BackToTransitionRoom()
        {
            EventManager.TriggerEvent(EventNames.BackToMirrorScene);
        }
    }
}