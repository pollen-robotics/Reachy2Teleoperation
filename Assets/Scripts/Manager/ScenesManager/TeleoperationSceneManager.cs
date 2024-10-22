using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class TeleoperationSceneManager : Singleton<TeleoperationSceneManager>
    {
        public enum TeleoperationExitMenuItem
        {
            LockAndHome, Cancel, Home
        }

        [SerializeField]
        private Button backToMirrorSceneButton;

        private ControllersManager controllers;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        public TeleoperationExitMenuItem teleoperationExitSelectedOption { get; private set; }
        public bool IsTeleoperationExitMenuActive { get; private set; }

        public UnityEvent event_OnAskForTeleoperationMenu;
        public UnityEvent event_OnLeaveTeleoperationMenu;

        private bool rightPrimaryButtonPreviouslyPressed;
        private bool leftPrimaryButtonPreviouslyPressed;

        public float indicatorTimer { get; private set; }
        private const float minIndicatorTimer = 0.0f;

        private bool suspensionMenuEntry = true;

        void Start()
        {
            EventManager.StartListening(EventNames.OnSuspendTeleoperation, CloseTeleoperationExitMenu);
            controllers = ControllersManager.Instance;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

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

            // For exit menu
            teleoperationExitSelectedOption = TeleoperationExitMenuItem.Cancel;
            IsTeleoperationExitMenuActive = false;
            indicatorTimer = 0.0f;

            // For start arm teleop input
            if(backToMirrorSceneButton != null) backToMirrorSceneButton.onClick.AddListener(BackToMirrorScene);
        }

        void StopTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: StopTeleoperation");
            //robotStatus.SetEmotionsActive(false);
            robotStatus.SetMobilityActive(false);
        }

        void Update()
        {
            bool rightPrimaryButtonPressed = false;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

            bool leftPrimaryButtonPressed = false;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed);

            Vector2 leftJoystickValue;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftJoystickValue);


            // Check teleoperation and controllers status for exit menu
            if (robotStatus.IsRobotArmTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                CheckTeleoperationExitMenuState(rightPrimaryButtonPressed, leftPrimaryButtonPressed, leftJoystickValue);
            }

            // Check teleoperation and controllers status to start arm teleoperation
            if (!robotStatus.IsRobotArmTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                CheckStartArmTeleoperationState(rightPrimaryButtonPressed);
            }

            // Check teleoperation and controllers status for suspension menu
            if (robotStatus.IsRobotTeleoperationActive() && robotStatus.AreRobotMovementsSuspended())
            {
                CheckTeleoperationSuspensionMenuState(rightPrimaryButtonPressed);
            }

            rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
            leftPrimaryButtonPreviouslyPressed = leftPrimaryButtonPressed;
        }

        void CheckTeleoperationExitMenuState(bool rightPrimaryButtonPressed, bool leftPrimaryButtonPressed, Vector2 leftJoystickValue)
        {
            if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
            {
                teleoperationExitSelectedOption = TeleoperationExitMenuItem.Home;
                if (!IsTeleoperationExitMenuActive)
                {
                    event_OnAskForTeleoperationMenu.Invoke();
                    IsTeleoperationExitMenuActive = true;
                }
            }

            if (IsTeleoperationExitMenuActive)
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
                        CloseTeleoperationExitMenu();
                        if (teleoperationExitSelectedOption == TeleoperationExitMenuItem.LockAndHome)
                            robotStatus.LockRobotPosition();
                        EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
                    }

                    if (leftPrimaryButtonPressed && !leftPrimaryButtonPreviouslyPressed)
                    {
                        teleoperationExitSelectedOption = TeleoperationExitMenuItem.LockAndHome;
                    }
                    else if (!leftPrimaryButtonPressed && leftPrimaryButtonPreviouslyPressed)
                    {
                        teleoperationExitSelectedOption = TeleoperationExitMenuItem.Home;
                    }

                }
                else if (!rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                {
                    CloseTeleoperationExitMenu();
                }
            }
        }

        void CheckStartArmTeleoperationState(bool rightPrimaryButtonPressed)
        {
            if ((!robotConfig.HasLeftArm() || !robotStatus.IsLeftArmOn()) && (!robotConfig.HasRightArm() || !robotStatus.IsRightArmOn()))
            {
                TeleoperationManager.Instance.AskForStartingArmTeleoperation();
            }
            else if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
            {
                TeleoperationManager.Instance.AskForStartingArmTeleoperation();
            }
        }

        void CheckTeleoperationSuspensionMenuState(bool rightPrimaryButtonPressed)
        {
            if (suspensionMenuEntry)
            {
                if (rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed) suspensionMenuEntry = false;
            }
            else
            {
                if (rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                {
                    indicatorTimer += Time.deltaTime;

                    if (indicatorTimer >= 1.0f)
                    {
                        EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
                    }
                }
                else
                {
                    indicatorTimer = minIndicatorTimer;
                }
            }
        }

        void CloseTeleoperationExitMenu()
        {
            indicatorTimer = minIndicatorTimer;
            event_OnLeaveTeleoperationMenu.Invoke();
            IsTeleoperationExitMenuActive = false;
        }

        void BackToMirrorScene()
        {
            EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
        }
    }
}