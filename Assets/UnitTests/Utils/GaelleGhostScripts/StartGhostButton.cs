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

        private MirrorSceneManager sceneManager;
        private RobotStatus robotStatus;

        void Start()
        {
            Button btn = startButton.GetComponent<Button>();
		    btn.onClick.AddListener(StartTeleoperation);

            sceneManager = MirrorSceneManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void StartTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.EnterTeleoperationScene);
        }
    }
}