using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class RobotJointState : MonoBehaviour
    {
        private DataMessageManager dataController;

        [Tooltip("Robot that will be updated")]
        public Reachy2Controller.Reachy2Controller reachy;

        void Start()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdatePresentPositions.AddListener(UpdateJointsState);

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