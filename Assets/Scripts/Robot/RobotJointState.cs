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

            // EventManager.StartListening(EventNames.QuitMirrorScene, UpdateRobot);
            // EventManager.StartListening(EventNames.MirrorSceneLoaded, UpdateModelRobot);
        }

        // void UpdateRobot()
        // {
        //     reachy = GameObject.Find("Reachy2").GetComponent<Reachy2Controller.Reachy2Controller>();
        // }

        // void UpdateModelRobot()
        // {
        //     reachy = GameObject.Find("Reachy2Ghost").GetComponent<Reachy2Controller.Reachy2Controller>();
        // }

        protected void UpdateJointsState(Dictionary<string, float> PresentPositions)
        {
            // if (reachy != null)
            // {
            //     reachy.HandleCommand(PresentPositions);
            // }
            event_OnPresentPositionsChanged.Invoke(PresentPositions);
        }
    }
}