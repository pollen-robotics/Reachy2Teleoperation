using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationSuspensionManager : Singleton<TeleoperationSuspensionManager>
    {
        // TODO: Put class somewhere else
        private RobotStatus robotStatus;
        private bool isActivatedTeleoperationSuspension;

        private ControllersManager controllers;

        public float indicatorTimer = 0.0f;
        private float minIndicatorTimer = 0.0f;

        private bool rightPrimaryButtonPressed = false;
        private bool rightPrimaryButtonPreviouslyPressed = false;
        private bool allowRightPrimaryButtonUse = true;

        // Start is called before the first frame update
        void Start()
        {
            EventManager.StartListening(EventNames.HeadsetRemoved, CallSuspensionWarning);
            EventManager.StartListening(EventNames.OnEmergencyStop, CallSuspensionWarning);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, ReinitValue);

            controllers = ControllersManager.Instance;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, NoSuspensionWarning);

            NoSuspensionWarning();
        }

        void ReinitValue()
        {
            indicatorTimer = minIndicatorTimer;
        }

        // Update is called once per frame
        void CallSuspensionWarning()
        {
            if (robotStatus.IsRobotTeleoperationActive())
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
                        EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
                        EventManager.TriggerEvent(EventNames.EnterMirrorScene);
                        EventManager.TriggerEvent(EventNames.OnResumeTeleoperation);
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