using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class Reachy2JointStateUpdater : MonoBehaviour
    {
        private RobotJointState robotJointState;

        [Tooltip("Robot that will be updated")]
        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachy;

        void Start()
        {
            robotJointState = RobotDataManager.Instance.RobotJointState;
            robotJointState.event_OnPresentPositionsChanged.AddListener(UpdateJointsState);
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