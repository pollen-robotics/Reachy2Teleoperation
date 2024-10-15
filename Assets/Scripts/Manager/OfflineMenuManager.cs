using UnityEngine;
using UnityEngine.Events;

namespace TeleopReachy
{
    public class OfflineMenuManager : Singleton<OfflineMenuManager>
    {
        public enum OfflineMenuItem
        {
            LockAndHome, Cancel, Home
        }

        private ControllersManager controllers;

        private RobotStatus robotStatus;
        //private RobotConfig robotConfig;

        public OfflineMenuItem selectedItem;

        private bool isOfflineMenuActive;

        private bool rightPrimaryButtonPreviouslyPressed;
        private bool leftPrimaryButtonPreviouslyPressed;

        private UserEmergencyStopInput userEmergencyStop;

        public float indicatorTimer = 0.0f;
        private const float minIndicatorTimer = 0.0f;

        public UnityEvent event_OnAskForOfflineMenu;
        public UnityEvent event_OnLeaveOfflineMenu;

        // Start is called before the first frame update
        void Start()
        {
            //robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(DeactivateOfflineMenu);

            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init_EmergencyStop);

            controllers = ControllersManager.Instance;

            selectedItem = OfflineMenuItem.Cancel;

            isOfflineMenuActive = false;
        }

        void Init_EmergencyStop()
        {
            userEmergencyStop = UserInputManager.Instance.UserEmergencyStopInput;
            userEmergencyStop.event_OnEmergencyStopCalled.AddListener(EmergencyStopCalled);
        }

        void EmergencyStopCalled()
        {
            ExitOffLineMenu();
        }

        // Update is called once per frame
        void Update()
        {
            bool rightPrimaryButtonPressed = false;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            bool leftPrimaryButtonPressed = false;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed);

            Vector2 leftJoystickValue;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftJoystickValue);

            if (robotStatus.IsRobotArmTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
                {
                    selectedItem = OfflineMenuItem.Home;
                    if (!isOfflineMenuActive)
                    {
                        event_OnAskForOfflineMenu.Invoke();
                        isOfflineMenuActive = true;
                    }
                }

                if (isOfflineMenuActive)
                {
                    if (rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                    {
                        float r = Mathf.Sqrt(Mathf.Pow(leftJoystickValue[0], 2) + Mathf.Pow(leftJoystickValue[1], 2));

                        if (r != 0)
                        {
                            indicatorTimer += Time.deltaTime * 2 * r;
                        }
                        else
                        {
                            indicatorTimer += Time.deltaTime / 2;
                        }

                        if (indicatorTimer >= 1.0f)
                        {
                            ExitOffLineMenu();
                            if (selectedItem == OfflineMenuItem.LockAndHome)
                                robotStatus.LockRobotPosition();
                            EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
                            EventManager.TriggerEvent(EventNames.EnterMirrorScene);
                        }

                        if (leftPrimaryButtonPressed && !leftPrimaryButtonPreviouslyPressed)
                        {
                            selectedItem = OfflineMenuItem.LockAndHome;
                        }
                        else if (!leftPrimaryButtonPressed && leftPrimaryButtonPreviouslyPressed)
                        {
                            selectedItem = OfflineMenuItem.Home;
                        }

                    }
                    else if (!rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                    {
                        ExitOffLineMenu();
                    }
                }
            }

            rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
            leftPrimaryButtonPreviouslyPressed = leftPrimaryButtonPressed;
        }

        void ExitOffLineMenu()
        {
            indicatorTimer = minIndicatorTimer;
            event_OnLeaveOfflineMenu.Invoke();
            DeactivateOfflineMenu();
        }

        void DeactivateOfflineMenu()
        {
            isOfflineMenuActive = false;
        }
    }
}
