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

        public GameObject stream; // to hide it when robot is virtual

        [SerializeField]
        private Transform resetPositionButton;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller simulatedReachy;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller realReachy;

        [SerializeField]
        private Transform realRobotLabel;

        [SerializeField]
        private Button leaveMirrorSceneButton;

        [SerializeField]
        private Button leaveMirrorSceneButtonRobotLocked;

        private bool realRobotDisplayed;
        private bool needUpdateRealRobotDisplay;

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

        public UnityEvent event_OnStartTeleopInitialization;
        public UnityEvent event_OnAbortTeleopInitialization;

        private bool rightPrimaryButtonPreviouslyPressed;

        // Start is called before the first frame update
        void Start()
        {
            needUpdateRealRobotDisplay = false;

            userOrigin = UserTrackerManager.Instance.transform;

            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnRobotReady.AddListener(RobotReadyForTeleop);
            connectionStatus.event_OnRobotUnready.AddListener(AbortTeleopInitialization);

            resetPositionButton.gameObject.SetActive(false);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(ModifyRobotsDisplayed);

            robotStatus = RobotDataManager.Instance.RobotStatus;

            readyButton.onClick.AddListener(ValidateUserOrigin);

            controllers = ActiveControllerManager.Instance.ControllersManager;

            leaveMirrorSceneButton.onClick.AddListener(CheckBeforeQuittingScene);

            if (Robot.IsCurrentRobotVirtual())
            {
                readyButton.gameObject.SetActive(false);
                stream.SetActive(false);
            }
            if (robotConfig.HasHead())
            {
                robotStatus.SetEmotionsActive(true);
            }

            DisplayRealRobot(false);
            FixUserOrigin();
            MakeMirrorFaceUserOrigin();
            initializationState = InitializationState.WaitingForRobotReady;

            if (connectionStatus.IsRobotReady()) RobotReadyForTeleop();
        }

        private void CheckBeforeQuittingScene()
        {
            if (!robotStatus.IsRobotPositionLocked) BackToConnectionScene();
            else menuWarningLockPosition.ActivateChildren(true);
        }

        private void ResetPosition()
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

        private void ModifyRobotsDisplayed()
        {
            DisplayRealRobot(realRobotDisplayed);
        }

        private void DisplayRealRobot(bool enabled)
        {
            realRobotDisplayed = enabled;
            needUpdateRealRobotDisplay = true;
        }

        void Update()
        {
            if(needUpdateRealRobotDisplay)
            {
                needUpdateRealRobotDisplay = false;
                realReachy.transform.switchRenderer(realRobotDisplayed);
                realRobotLabel.gameObject.SetActive(realRobotDisplayed);
                if (robotConfig.GotReachyConfig())
                {
                    realReachy.Head.transform.switchRenderer(robotConfig.HasHead() && realRobotDisplayed);
                    realReachy.LeftArm.transform.switchRenderer(robotConfig.HasLeftArm() && realRobotDisplayed);
                    realReachy.RightArm.transform.switchRenderer(robotConfig.HasRightArm() && realRobotDisplayed);
                    realReachy.MobileBase.transform.switchRenderer(robotConfig.HasMobileBase() && realRobotDisplayed);
                }
            }

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
        }

        protected void AbortTeleopInitialization()
        {
            initializationState = InitializationState.WaitingForRobotReady;
            DisplayRealRobot(false);
            event_OnAbortTeleopInitialization.Invoke();
        }

        protected void RobotReadyForTeleop()
        {
            initializationState = InitializationState.WaitingForUserOriginValidation;
            event_OnStartTeleopInitialization.Invoke();
        }

        protected void BackToConnectionScene()
        {
            EventManager.TriggerEvent(EventNames.EnterConnectionScene);
        }
    }
}

