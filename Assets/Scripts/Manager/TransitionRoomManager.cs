using System.Collections;
using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public enum TransitionState
    {
        WaitingForTracker, WaitingForPosition, ReadyForTeleop, WaitingForRobot
    }

    public class TransitionRoomManager : Singleton<TransitionRoomManager>
    {
        public TransitionState State { get; private set; }

        [SerializeField]
        private Transform readyButton;

        public GameObject stream;

        [SerializeField]
        private Transform resetPositionButton;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachyGhost;
        private bool robotGhostDisplayed;

        [SerializeField]
        private Transform ghostReachyIndicator;

        private Transform userTracker;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private ConnectionStatus connectionStatus;

        private Transform headset;

        private bool userTrackerOk;

        public Transform mirror = null;

        const float distanceToMirror = 2.5f;
        const float mirrorHeight = -0.0f;

        private bool needUpdateReachyGhostDisplay;

        public UnityEvent event_OnReadyForTeleop;
        public UnityEvent event_OnAbortTeleop;
        public UnityEvent event_OnWaitingForPosition;
        public UnityEvent event_OnExitTransitionRoomRequested;

        // Start is called before the first frame update
        void Start()
        {
            needUpdateReachyGhostDisplay = false;

            headset = GameObject.Find("Main Camera").transform;
            userTracker = UserTrackerManager.Instance.transform;
            State = TransitionState.WaitingForTracker;

            connectionStatus = WebRTCManager.Instance.ConnectionStatus;
            connectionStatus.event_OnRobotReady.AddListener(ReadyForTeleop);
            connectionStatus.event_OnRobotUnready.AddListener(AbortTeleop);

            resetPositionButton.gameObject.SetActive(false);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(DisplayReachy);

            robotStatus = RobotDataManager.Instance.RobotStatus;

            DisplayReachy(false);
            FixUserTrackerPosition();
            MakeMirrorFaceUser();
            if (Robot.IsCurrentRobotVirtual())
            {
                readyButton.gameObject.SetActive(false);
                stream.SetActive(false);
            }
            if (robotConfig.HasHead())
            {
                robotStatus.SetEmotionsActive(true);
            }
        }

        public void ResetPosition()
        {
            FixUserTrackerPosition();
            MakeMirrorFaceUser();
        }

        private void MakeMirrorFaceUser()
        {
            mirror.position = userTracker.TransformPoint(Vector3.forward * distanceToMirror);
            mirror.position = new Vector3(mirror.position.x, mirror.position.y + mirrorHeight, mirror.position.z);

            Quaternion rotation = headset.localRotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            // Only the rotation around the y axis is kept, z and x axis are considered parallel to the floor
            Quaternion systemRotation = Quaternion.Euler(0, eulerAngles.y, 0);
            mirror.rotation = systemRotation;
        }

        private void DisplayReachy()
        {
            DisplayReachy(robotGhostDisplayed);
        }

        private void DisplayReachy(bool enabled)
        {
            robotGhostDisplayed = enabled;
            needUpdateReachyGhostDisplay = true;
        }

        void Update()
        {
            if(needUpdateReachyGhostDisplay)
            {
                needUpdateReachyGhostDisplay = false;
                reachyGhost.transform.switchRenderer(robotGhostDisplayed);
                ghostReachyIndicator.gameObject.SetActive(robotGhostDisplayed);
                if (robotConfig.GotReachyConfig())
                {
                    reachyGhost.head.transform.switchRenderer(robotConfig.HasHead() && robotGhostDisplayed);
                    reachyGhost.l_arm.transform.switchRenderer(robotConfig.HasLeftArm() && robotGhostDisplayed);
                    reachyGhost.r_arm.transform.switchRenderer(robotConfig.HasRightArm() && robotGhostDisplayed);
                    reachyGhost.mobile_base.transform.switchRenderer(robotConfig.HasMobileBase() && robotGhostDisplayed);
                }
            }
        }

        void FixUserTrackerPosition()
        // Fix the position and orientation of Reachy's coordinate system of the user based on the headset position and orientation
        {
            Quaternion rotation = headset.localRotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            // Only the rotation around the y axis is kept, z and x axis are considered parallel to the floor
            Quaternion systemRotation = Quaternion.Euler(0, eulerAngles.y, 0);

            userTracker.rotation = systemRotation;
            // Origin of the coordinate system is placed 15cm under the headset y position
            Vector3 headPosition = headset.position - headset.forward * 0.1f;
            userTracker.position = new Vector3(headPosition.x, headPosition.y - UserSize.Instance.UserShoulderHeadDistance, headPosition.z);
        }

        public void ValidateTracker()
        {
            FixUserTrackerPosition();
            userTrackerOk = true;
            if (connectionStatus.IsRobotReady()) ReadyForTeleop();
            else AbortTeleop();
        }

        public void WaitingForPosition()
        {
            readyButton.gameObject.SetActive(false);
            DisplayReachy(true);
            resetPositionButton.gameObject.SetActive(true);
            State = TransitionState.WaitingForPosition;
            event_OnWaitingForPosition.Invoke();
        }

        public void AbortTeleop()
        {
            if (userTrackerOk)
            {
                State = TransitionState.WaitingForRobot;
                DisplayReachy(true);
                readyButton.gameObject.SetActive(false);
                event_OnAbortTeleop.Invoke();
                // WaitingForPosition();
            }
        }

        public void ReadyForTeleop()
        {
            robotStatus.InitializeRobotState();
            if (userTrackerOk)
            {
                State = TransitionState.ReadyForTeleop;
                DisplayReachy(true);
                readyButton.gameObject.SetActive(false);
                resetPositionButton.gameObject.SetActive(true);
                event_OnReadyForTeleop.Invoke();
            }
        }

        public void ExitTransitionRoomRequested()
        {
            event_OnExitTransitionRoomRequested.Invoke();
            robotStatus.SetEmotionsActive(true);
            EventManager.TriggerEvent(EventNames.QuitMirrorScene);
        }

        public void BackToConnectionScene()
        {
            StartCoroutine(GoBackToConnectionScene());
        }

        IEnumerator GoBackToConnectionScene()
        {
            RobotJointCommands robotJointsCommands = RobotDataManager.Instance.RobotJointCommands;
            if (robotJointsCommands.setSmoothCompliance != null) yield return robotJointsCommands.setSmoothCompliance;
            else yield return null;
            EventManager.TriggerEvent(EventNames.LoadConnectionScene);
        }
    }
}

