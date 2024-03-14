using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra;

using System;

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
        private Transform reachyGhost;

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

        public UnityEvent event_OnReadyForTeleop;
        public UnityEvent event_OnAbortTeleop;
        public UnityEvent event_OnWaitingForPosition;
        public UnityEvent event_OnExitTransitionRoomRequested;


        // calibration variables 
        public double meanArmSize { get; set; }
        public Vector3 midShoulderPoint { get; set; }
        private CaptureHandPoints captureHandPoints;


        // Start is called before the first frame update
        void Start()
        {
            headset = GameObject.Find("Main Camera").transform;
            userTracker = UserTrackerManager.Instance.transform;
            State = TransitionState.WaitingForTracker;

            connectionStatus = WebRTCManager.Instance.ConnectionStatus;
            connectionStatus.event_OnRobotReady.AddListener(ReadyForTeleop);
            connectionStatus.event_OnRobotUnready.AddListener(AbortTeleop);

            resetPositionButton.gameObject.SetActive(false);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus; 
        
            HideReachy();
            FixUserTrackerPosition();
            MakeMirrorFaceUser();
            Debug.Log("1");

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

        // //calibration
        // private void CalibrationShoulders()
        // {
        //     Debug.Log("Debut de la fonction de calibration du TransitionManager");
        //     Transform trackedLeftHand = GameObject.Find("TrackedLeftHand").transform;
        //     Transform trackedRightHand = GameObject.Find("TrackedRightHand").transform;
        //     if (trackedLeftHand == null || trackedRightHand == null) {
        //         Debug.Log("Manettes non trouvées."); 
        //         return;
        //     } else {
        //         captureHandPoints = new CaptureHandPoints();
        //         captureHandPoints.CaptureAndCalibrate();
        //         Debug.Log("Fin de la fonction de calibration du TransitionManager");
        //     }
            
        // }

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
            if (robotConfig.GotReachyConfig())
            {
                reachyGhost.switchRenderer(true);
                // ghostReachyIndicator.gameObject.SetActive(true);
                if (!robotConfig.HasHead())
                {
                    reachyGhost.GetChild(0).switchRenderer(false);
                }
                if (!robotConfig.HasLeftArm())
                {
                    reachyGhost.GetChild(1).switchRenderer(false);
                }
                if (!robotConfig.HasRightArm())
                {
                    reachyGhost.GetChild(3).switchRenderer(false);
                }
            }
        }

        private void HideReachy()
        {
            reachyGhost.switchRenderer(false);
            ghostReachyIndicator.gameObject.SetActive(false);
        }

        void FixUserTrackerPosition()
        // Fix the position and orientation of Reachy's coordinate system of the user based on the headset position and orientation
        {
            Debug.Log("Debut du FixUser");
            Quaternion rotation = headset.localRotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            // Only the rotation around the y axis is kept, z and x axis are considered parallel to the floor
            Quaternion systemRotation = Quaternion.Euler(0, eulerAngles.y, 0);

            userTracker.rotation = systemRotation;
            // Origin of the coordinate system is placed 15cm under the headset y position
            Vector3 headPosition = headset.position - headset.forward * 0.1f;
            userTracker.position = new Vector3(headPosition.x, headPosition.y - UserSize.Instance.UserShoulderHeadDistance, headPosition.z);
            Debug.Log("ancienne :" + userTracker.position);


        }

        //calibration
        public void FixNewPosition()
        {

            userTracker.position = midShoulderPoint;
            Debug.Log("nouvelle :" + userTracker.position);

            // Debug.Log("Debut du NewFixUser");

            // //modification pour calibration 
            // Matrix4x4 midShoulderTransform = Matrix4x4.TRS(midShoulderPoint, userTracker.rotation, Vector3.one); 
            // Matrix4x4 userCenterMatrix = midShoulderTransform * headset.transform.worldToLocalMatrix;
            // Debug.Log("user transform initiale :" + userTracker.transform.position + userTracker.transform.rotation );
            // Debug.Log("user transform avec calib :" + userCenterMatrix);

        }

        public void ValidateTracker()
        {
           // FixUserTrackerPosition();
            userTrackerOk = true;
            if (connectionStatus.IsRobotReady()) ReadyForTeleop();
            else AbortTeleop();
        }

        public void WaitingForPosition()
        {
            readyButton.gameObject.SetActive(false);
            DisplayReachy();
            resetPositionButton.gameObject.SetActive(true);
            State = TransitionState.WaitingForPosition;
            event_OnWaitingForPosition.Invoke();
        }

        public void AbortTeleop()
        {
            if (userTrackerOk)
            {
                State = TransitionState.WaitingForRobot;
                DisplayReachy();
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
                DisplayReachy();
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

