using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class StopGhostButton : MonoBehaviour
    {
        [SerializeField]
        public Button stopButton;

        void Start()
        {
            Button btn = stopButton.GetComponent<Button>();
		    btn.onClick.AddListener(StopTeleoperation);
        }

        void StopTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.QuitTeleoperationScene);
        }
    }
}