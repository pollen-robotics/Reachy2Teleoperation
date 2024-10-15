using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class TeleoperationSceneManager : Singleton<TeleoperationSceneManager>
    {
        public enum TeleoperationMenuItem
        {
            LockAndHome, Cancel, Home
        }

        private ControllersManager controllers;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private bool needUpdateRobotDisplay;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachy;
        private bool robotDisplayed;

        public TeleoperationMenuItem selectedItem;
        private bool isTeleoperationMenuActive;

        public UnityEvent event_OnAskForTeleoperationMenu;
        public UnityEvent event_OnLeaveTeleoperationMenu;

        private bool rightPrimaryButtonPreviouslyPressed;
        private bool leftPrimaryButtonPreviouslyPressed;

        public float indicatorTimer = 0.0f;
        private const float minIndicatorTimer = 0.0f;

        void Start()
        {
            EventManager.StartListening(EventNames.OnSuspendTeleoperation, ExitTeleoperationMenu);
            controllers = ControllersManager.Instance;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(DisplayReachy);

            if (robotConfig.HasMobileBase())
            {
                robotStatus.SetMobilityActive(true);
            }
            if (robotStatus.IsRobotPositionLocked || robotStatus.IsGraspingLockActivated())
            {
                if (robotStatus.IsLeftGripperClosed())
                {
                    UserInputManager.Instance.UserMovementsInput.ForceLeftGripperStatus(true);
                    robotStatus.SetGraspingLockActivated(true);
                }
                if (robotStatus.IsRightGripperClosed())
                {
                    UserInputManager.Instance.UserMovementsInput.ForceRightGripperStatus(true);
                    robotStatus.SetGraspingLockActivated(true);
                }
            }

            // For menu
            selectedItem = TeleoperationMenuItem.Cancel;
            isTeleoperationMenuActive = false;
        }

        void StopTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: StopTeleoperation");
            //robotStatus.SetEmotionsActive(false);
            robotStatus.SetMobilityActive(false);
        }

        void Update()
        {
            if(needUpdateRobotDisplay)
            {
                needUpdateRobotDisplay = false;
                reachy.transform.switchRenderer(robotDisplayed);
                if (robotConfig.GotReachyConfig())
                {
                    reachy.head.transform.switchRenderer(robotConfig.HasHead() && robotDisplayed);
                    reachy.l_arm.transform.switchRenderer(robotConfig.HasLeftArm() && robotDisplayed);
                    reachy.r_arm.transform.switchRenderer(robotConfig.HasRightArm() && robotDisplayed);
                    reachy.mobile_base.transform.switchRenderer(robotConfig.HasMobileBase() && robotDisplayed);
                }
            }


            // Update to leave teleoperation
            bool rightPrimaryButtonPressed = false;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            bool leftPrimaryButtonPressed = false;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed);

            Vector2 leftJoystickValue;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftJoystickValue);

            if (robotStatus.IsRobotArmTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                CheckTeleoperationMenuState(rightPrimaryButtonPressed, leftPrimaryButtonPressed, leftJoystickValue);
            }

            rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
            leftPrimaryButtonPreviouslyPressed = leftPrimaryButtonPressed;
        }

        private void DisplayReachy()
        {
            DisplayReachy(robotDisplayed);
        }

        private void DisplayReachy(bool enabled)
        {
            robotDisplayed = enabled;
            needUpdateRobotDisplay = true;
        }

        void CheckTeleoperationMenuState(bool rightPrimaryButtonPressed, bool leftPrimaryButtonPressed, Vector2 leftJoystickValue)
        {
            if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
            {
                selectedItem = TeleoperationMenuItem.Home;
                if (!isTeleoperationMenuActive)
                {
                    event_OnAskForTeleoperationMenu.Invoke();
                    isTeleoperationMenuActive = true;
                }
            }

            if (isTeleoperationMenuActive)
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
                        ExitTeleoperationMenu();
                        if (selectedItem == TeleoperationMenuItem.LockAndHome)
                            robotStatus.LockRobotPosition();
                        EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
                    }

                    if (leftPrimaryButtonPressed && !leftPrimaryButtonPreviouslyPressed)
                    {
                        selectedItem = TeleoperationMenuItem.LockAndHome;
                    }
                    else if (!leftPrimaryButtonPressed && leftPrimaryButtonPreviouslyPressed)
                    {
                        selectedItem = TeleoperationMenuItem.Home;
                    }

                }
                else if (!rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                {
                    ExitTeleoperationMenu();
                }
            }
        }

        void ExitTeleoperationMenu()
        {
            indicatorTimer = minIndicatorTimer;
            event_OnLeaveTeleoperationMenu.Invoke();
            isTeleoperationMenuActive = false;
        }
    }
}