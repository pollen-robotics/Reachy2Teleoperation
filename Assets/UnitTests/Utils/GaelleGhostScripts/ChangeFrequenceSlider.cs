using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class ChangeFrequenceSlider : MonoBehaviour
    {
        public Slider frequencySlider;
        public Text currentFrequency;

        private TransitionRoomManager transitionRoomManager;
        private RobotStatus robotStatus;

        void Start()
        {
            frequencySlider = frequencySlider.GetComponent<Slider>();
            frequencySlider.value = Application.targetFrameRate;
		    frequencySlider.onValueChanged.AddListener(delegate {ChangeFrequency();});

            transitionRoomManager = TransitionRoomManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void ChangeFrequency()
        {
            Application.targetFrameRate = (int)frequencySlider.value;
            currentFrequency.text = frequencySlider.value.ToString();
        }
    }
}