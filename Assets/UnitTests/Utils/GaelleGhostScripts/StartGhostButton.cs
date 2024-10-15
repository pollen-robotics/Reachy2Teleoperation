using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class StartGhostButton : MonoBehaviour
    {
        [SerializeField]
        public Button startButton;

        private TransitionRoomManager transitionRoomManager;
        private RobotStatus robotStatus;

        void Start()
        {
            Button btn = startButton.GetComponent<Button>();
		    btn.onClick.AddListener(StartTeleoperation);

            transitionRoomManager = TransitionRoomManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void StartTeleoperation()
        {
            transitionRoomManager.ValidateTracker();
            transitionRoomManager.ExitTransitionRoomRequested();
            EventManager.TriggerEvent(EventNames.OnStartTeleoperation);
        }
    }
}