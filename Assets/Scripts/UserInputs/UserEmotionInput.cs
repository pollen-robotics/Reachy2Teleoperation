using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public enum Emotion
    {
        Happy, Sad, Confused, NoEmotion
    }

    public class UserEmotionInput : MonoBehaviour
    {
        private ControllersManager controllers;
        
        private Vector2 selectedDirection;

        private Emotion selectedEmotion;

        private bool isEmotionSelected;

        public UnityEvent<Emotion> event_OnEmotionSelected;

        void Awake()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            isEmotionSelected = false;

            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);
        }

        void Init()
        {
            RobotDataManager.Instance.RobotStatus.event_OnEmotionStart.AddListener(SetEmotionSelected);
            RobotDataManager.Instance.RobotStatus.event_OnEmotionOver.AddListener(EmotionIsOver);
        }

        void Update()
        {
            // For joystick commands
            if (!isEmotionSelected)
            {
                controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out selectedDirection);

                float phi = Mathf.Atan2(selectedDirection[1], selectedDirection[0]);

                if (Mathf.Abs(phi) < (Mathf.PI / 8)) selectedEmotion = Emotion.Happy;
                else if ((phi > (Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.Sad;
                else if (Mathf.Abs(phi) > (Mathf.PI - Mathf.PI / 8)) selectedEmotion = Emotion.Confused;
                else if ((phi > (-Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (-Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.NoEmotion;

                event_OnEmotionSelected.Invoke(selectedEmotion);
            }
        }

        private void EmotionIsOver()
        {
            isEmotionSelected = false;
        }

        private void SetEmotionSelected()
        {
            isEmotionSelected = true;
        }

        public Emotion GetSelectedEmotion()
        {
            return selectedEmotion;
        }
    }
}