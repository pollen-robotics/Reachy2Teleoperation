using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace TeleopReachy
{
    public enum InitializationState
    {
        WaitingForRobotReady, WaitingForUserOriginValidation, ReadyForTeleop,
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

            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnRobotReady.AddListener(RobotReadyForTeleop);
            connectionStatus.event_OnRobotUnready.AddListener(AbortTeleopInitialization);

            resetPositionButton.gameObject.SetActive(false);

            robotStatus = RobotDataManager.Instance.RobotStatus;

            readyButton.onClick.AddListener(ValidateUserOrigin);

            controllers = ActiveControllerManager.Instance.ControllersManager;

            leaveMirrorSceneButton.onClick.AddListener(CheckIfLockedBeforeQuittingScene);
            leaveMirrorSceneButtonRobotLocked.onClick.AddListener(SetRobotCompliantBeforeQuittingScene);

            if (robotConfig.HasHead())
            {
                robotStatus.SetEmotionsActive(true);
            }

            ResetPosition();
            initializationState = InitializationState.WaitingForRobotReady;

            if (connectionStatus.IsRobotReady()) RobotReadyForTeleop();
        }

        private void CheckIfLockedBeforeQuittingScene()
        {
            if (!robotStatus.IsRobotPositionLocked) BackToConnectionScene();
            else menuWarningLockPosition.ActivateChildren(true);
        }

        private void SetRobotCompliantBeforeQuittingScene()
        {
            TeleoperationManager.Instance.AskForRobotSmoothlyCompliant();
            RobotDataManager.Instance.RobotStatus.event_OnRobotFullyCompliant.AddListener(BackToConnectionScene);
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
                bool rightPrimaryButtonPressed;
                controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);

                if (rightPrimaryButtonPressed && rightPrimaryButtonPreviouslyPressed)
                {
                    indicatorTimer += Time.deltaTime;
                    if (indicatorTimer >= 1.0f)
                    {
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

        protected void ValidateUserOrigin()
        {
            ResetPosition();
            initializationState = InitializationState.ReadyForTeleop;
            event_OnTeleopInitializationStepChanged.Invoke();
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
            EventManager.TriggerEvent(EventNames.EnterConnectionScene);
        }
    }
}
