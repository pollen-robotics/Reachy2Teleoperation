using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class Reachy2JointStateUpdater : MonoBehaviour
    {
        private RobotJointState robotJointState;

        [Tooltip("Robot that will be updated")]
        public Reachy2Controller.Reachy2Controller reachy;

        void Start()
        {
            robotJointState = RobotDataManager.Instance.RobotJointState;
            robotJointState.event_OnPresentPositionsChanged.AddListener(UpdateJointsState);

            EventManager.StartListening(EventNames.QuitMirrorScene, UpdateRobot);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, UpdateModelRobot);
        }

        void UpdateRobot()
        {
            reachy = GameObject.Find("Reachy2").GetComponent<Reachy2Controller.Reachy2Controller>();
        }

        void UpdateModelRobot()
        {
            reachy = GameObject.Find("Reachy2Ghost").GetComponent<Reachy2Controller.Reachy2Controller>();
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