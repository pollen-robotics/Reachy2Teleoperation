using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class UserEmergencyStopInput : MonoBehaviour
    {
        private ControllersManager controllers;

        private RobotStatus robotStatus;

        bool rightGripPressed;
        bool leftGripPressed;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        private void OnDisable()
        {
            EventManager.StopListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        private void Init()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void Awake()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
        }

        void Start()
        {
            rightGripPressed = false;
            leftGripPressed = false;
        }

        void Update()
        {
            // For joystick commands
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out rightGripPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out leftGripPressed);

            if (robotStatus != null && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                if (rightGripPressed && leftGripPressed)
                {
                    robotStatus.SuspendRobotTeleoperation();
                }
            }
        }
    }
}