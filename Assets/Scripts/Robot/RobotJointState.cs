using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Grpc.Core;
using Reachy;

namespace TeleopReachy
{
    public class RobotJointState : MonoBehaviour
    {
        private gRPCDataController dataController;

        [Tooltip("Robot that will be updated")]
        public ReachyController.ReachyController reachy;

        //private bool inTransitionRoom;

        void Start()
        {
            dataController = gRPCManager.Instance.gRPCDataController;
            dataController.event_OnStateUpdatePresentPositions.AddListener(UpdateJointsState);

            EventManager.StartListening(EventNames.QuitMirrorScene, UpdateRobot);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, UpdateModelRobot);

            //inTransitionRoom = true;
        }

        void Update()
        {
            dataController.GetJointsState();
        }

        void UpdateRobot()
        {
            reachy = GameObject.Find("Reachy").GetComponent<ReachyController.ReachyController>();
            // reachy = null;
        }

        void UpdateModelRobot()
        {
            reachy = GameObject.Find("ReachyGhost").GetComponent<ReachyController.ReachyController>();
        }

        protected void UpdateJointsState(Dictionary<string, float> PresentPositions)
        {
            if (reachy != null)
            {
                reachy.HandleCommand(PresentPositions);
            }
        }
    }
}