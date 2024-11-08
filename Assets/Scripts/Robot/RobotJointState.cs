using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class RobotJointState : MonoBehaviour
    {
        private DataMessageManager dataController;
        public UnityEvent<Dictionary<string, float>> event_OnPresentPositionsChanged; 

        void Start()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdatePresentPositions.AddListener(UpdateJointsState);
        }

        protected void UpdateJointsState(Dictionary<string, float> PresentPositions)
        {
            event_OnPresentPositionsChanged.Invoke(PresentPositions);
        }
    }
}