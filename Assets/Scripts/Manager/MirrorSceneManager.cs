using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace TeleopReachy
{
    public enum InitializationState
    {
        WaitingForRobotReady, WaitingForUserOriginValidation, ReadyForTeleop, NoInitializationRequired,
    }

    public class MirrorSceneManager : Singleton<MirrorSceneManager>
    {
        public InitializationState initializationState { get; private set; }

        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private Transform resetPositionButton;

        [SerializeField]
        private Button leaveMirrorSceneButton;

        [SerializeField]
        private Button leaveMirrorSceneButtonRobotLocked;

        private Transform userOrigin;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private ConnectionStatus connectionStatus;

        private ControllersManager controllers;

        private RealHandTracking handTracking;

        public float indicatorTimer { get; private set; }
        private const float minIndicatorTimer = 0.0f;

        [SerializeField]
        private Transform mirror;

        [SerializeField]
        private Transform menuWarningLockPosition;

        const float distanceToMirror = 2.5f;
        const float mirrorHeight = -0.0f;

        public UnityEvent event_OnTeleopInitializationStepChanged;

        private bool rightPrimaryButtonPreviouslyPressed;

        void Start()
        {
            userOrigin = UserTrackerManager.Instance.transform;

            resetPositionButton.gameObject.SetActive(false);

            readyButton.onClick.AddListener(ValidateUserOrigin);

            controllers = ActiveControllerManager.Instance.ControllersManager;
            handTracking = RealHandTracking.Instance;

            leaveMirrorSceneButton.onClick.AddListener(CheckIfLockedBeforeQuittingScene);
            leaveMirrorSceneButtonRobotLocked.onClick.AddListener(SetRobotCompliantBeforeQuittingScene);

            ResetPosition();
            if (Robot.IsCurrentRobotVirtual())  initializationState = InitializationState.NoInitializationRequired;
            else initializationState = InitializationState.WaitingForRobotReady;

            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnRobotReady.AddListener(RobotReadyForTeleop);
            connectionStatus.event_OnRobotUnready.AddListener(AbortTeleopInitialization);

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            // if (robotConfig.HasHead())
            // {
            //     robotStatus.SetEmotionsActive(true);
            // }

            if (connectionStatus.IsRobotReady()) RobotReadyForTeleop();
        }

        private void CheckIfLockedBeforeQuittingScene()
        {
            if (!robotStatus.IsRobotPositionLocked) SetRobotCompliantBeforeQuittingScene();
            else menuWarningLockPosition.ActivateChildren(true);
        }

        private void SetRobotCompliantBeforeQuittingScene()
        {
            if (!robotStatus.IsRobotCompliant())
            {
                menuWarningLockPosition.ActivateChildren(true);
                menuWarningLockPosition.GetComponent<ExitOnLockedPositionUIManager>().QuitTransitionRoom();
                TeleoperationManager.Instance.AskForRobotSmoothlyCompliant();
                RobotDataManager.Instance.RobotStatus.event_OnRobotFullyCompliant.AddListener(BackToConnectionScene);
            }
            else
            {
                BackToConnectionScene();
            }
        }

        public void ResetPosition()
        {
            FixUserOrigin();
            MakeMirrorFaceUserOrigin();
        }

        void FixUserOrigin()
        {
            EventManager.TriggerEvent(EventNames.OnFixUserOrigin);
        }

        private void MakeMirrorFaceUserOrigin()
        {
            mirror.position = userOrigin.TransformPoint(Vector3.forward * distanceToMirror);
            mirror.position = new Vector3(mirror.position.x, mirror.position.y + mirrorHeight, mirror.position.z);
            mirror.rotation = userOrigin.localRotation;
        }

        void Update()
        {
            if (initializationState == InitializationState.ReadyForTeleop)
            {
                if (handTracking.IsHandTrackingUsed())
                {
                    if (handTracking.IsOkPosePerformed)
                    {
                        indicatorTimer += Time.deltaTime;
                        if (indicatorTimer >= 1.0f)
                        {
                            EventManager.TriggerEvent(EventNames.OnInitializeRobotStateRequested);
                            EventManager.TriggerEvent(EventNames.EnterTeleoperationScene);
                        }
                    }
                    else
                    {
                        indicatorTimer = minIndicatorTimer;
                    }
                }

                // else we use the controllers as usual
                else
                {
                    bool rightPrimaryButtonPressed;
                    controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

                    if (rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                    {
                        indicatorTimer += Time.deltaTime;
                        if (indicatorTimer >= 1.0f)
                        {
                            handTracking.PoseStopped();
                            EventManager.TriggerEvent(EventNames.OnInitializeRobotStateRequested);
                            EventManager.TriggerEvent(EventNames.EnterTeleoperationScene);
                        }
                    }
                    else
                    {
                        indicatorTimer = minIndicatorTimer;
                    }
                    rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
                }               
            }

            if (initializationState == InitializationState.WaitingForUserOriginValidation && handTracking.IsHandTrackingUsed())
            {
                if (handTracking.IsOkPosePerformed)
                {
                    ValidateUserOrigin();
                    handTracking.PoseStopped();
                }
            }
        }

        protected void ValidateUserOrigin()
        {
            ResetPosition();
            initializationState = InitializationState.ReadyForTeleop;
            event_OnTeleopInitializationStepChanged.Invoke();
            resetPositionButton.gameObject.SetActive(true);
        }

        protected void AbortTeleopInitialization()
        {
            initializationState = InitializationState.WaitingForRobotReady;
            event_OnTeleopInitializationStepChanged.Invoke();
        }

        protected void RobotReadyForTeleop()
        {
            initializationState = InitializationState.WaitingForUserOriginValidation;
            event_OnTeleopInitializationStepChanged.Invoke();
        }

        protected void BackToConnectionScene()
        {
            EventManager.TriggerEvent(EventNames.OnReinitializeLimitsRequested);
        }
    }
}
