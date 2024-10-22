using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class UserEmergencyStopInput : MonoBehaviour
    {
        private ControllersManager controllers;

        private RobotStatus robotStatus;

        bool rightGripPressed;
        bool rightTriggerPressed;
        bool rightPrimaryButtonPressed;
        bool leftGripPressed;
        bool leftTriggerPressed;
        bool leftPrimaryButtonPressed;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);
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
            leftTriggerPressed = false;
            leftPrimaryButtonPressed = false;
        }

        void Update()
        {
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out rightGripPressed);
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out rightTriggerPressed);
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out leftGripPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out leftTriggerPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed);

            // if (robotStatus != null && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            // {
                if ((leftGripPressed && leftTriggerPressed && leftPrimaryButtonPressed) || (rightGripPressed && rightTriggerPressed && rightPrimaryButtonPressed))
                {
                    EventManager.TriggerEvent(EventNames.OnSuspendTeleoperation);
                    EventManager.TriggerEvent(EventNames.OnEmergencyStop);
                }
            // }
        }
    }
}