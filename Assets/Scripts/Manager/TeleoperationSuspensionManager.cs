using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationSuspensionManager : Singleton<TeleoperationSuspensionManager>
    {
        private RobotStatus robotStatus;
        private bool isActivatedTeleoperationSuspension;

        private ControllersManager controllers;
        private UserEmergencyStopInput userEmergencyStop;

        public float indicatorTimer = 0.0f;
        private float minIndicatorTimer = 0.0f;

        private bool rightPrimaryButtonPressed = false;
        private bool rightPrimaryButtonPreviouslyPressed = false;
        private bool allowRightPrimaryButtonUse = true;

        // Start is called before the first frame update
        void Start()
        {
            EventManager.StartListening(EventNames.HeadsetRemoved, CallSuspensionWarning);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            EventManager.StartListening(EventNames.BackToMirrorScene, ReinitValue);

            controllers = ControllersManager.Instance;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(NoSuspensionWarning);

            NoSuspensionWarning();
        }

        void Init()
        {
            userEmergencyStop = UserInputManager.Instance.UserEmergencyStopInput;
            userEmergencyStop.event_OnEmergencyStopCalled.AddListener(CallSuspensionWarning);
        }

        void ReinitValue()
        {
            indicatorTimer = minIndicatorTimer;
        }

        // Update is called once per frame
        void CallSuspensionWarning()
        {
            if(robotStatus.IsRobotTeleoperationActive())
            {
                if (rightPrimaryButtonPressed) allowRightPrimaryButtonUse = false;
                else allowRightPrimaryButtonUse = true;
                isActivatedTeleoperationSuspension = true;
            }
        }

        void NoSuspensionWarning()
        {
            isActivatedTeleoperationSuspension = false;
        }

        void Update()
        {
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            if (isActivatedTeleoperationSuspension)
            {
                if (rightPrimaryButtonPressed && allowRightPrimaryButtonUse)
                {
                    indicatorTimer += Time.deltaTime;

                    if (indicatorTimer >= 1.0f)
                    {
                        EventManager.TriggerEvent(EventNames.BackToMirrorScene);
                        robotStatus.ResumeRobotTeleoperation();
                    }
                }
                else
                {
                    indicatorTimer = minIndicatorTimer;
                    if (!rightPrimaryButtonPreviouslyPressed && rightPrimaryButtonPressed) allowRightPrimaryButtonUse = true;
                }
            }
            rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
        }
    }
}