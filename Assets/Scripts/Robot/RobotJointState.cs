using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class RobotJointState : MonoBehaviour
    {
        private DataMessageManager dataController;

        [Tooltip("Robot that will be updated")]
        public ReachyController.ReachyController reachy;

        void Start()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdatePresentPositions.AddListener(UpdateJointsState);

            EventManager.StartListening(EventNames.QuitMirrorScene, UpdateRobot);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, UpdateModelRobot);
        }

        void UpdateRobot()
        {
            reachy = GameObject.Find("Reachy").GetComponent<ReachyController.ReachyController>();
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