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

        // Start is called before the first frame update
        void Start()
        {
            EventManager.StartListening(EventNames.HeadsetRemoved, CallSuspensionWarning);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);

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

        // Update is called once per frame
        void CallSuspensionWarning()
        {
            if(robotStatus.IsRobotTeleoperationActive())
            {
                isActivatedTeleoperationSuspension = true;
            }
        }

        void NoSuspensionWarning()
        {
            isActivatedTeleoperationSuspension = false;
        }

        void Update()
        {
            if (isActivatedTeleoperationSuspension)
            {
                bool rightPrimaryButtonPressed = false;
                controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

                if (rightPrimaryButtonPressed)
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
                }
            }
        }
    }
}