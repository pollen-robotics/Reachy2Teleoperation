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

        private float previousR;

        public UnityEvent<Emotion> event_OnEmotionSelected;

        void Awake()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            isEmotionSelected = false;
            previousR = 0;

            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);
        }

        void Init()
        {
            RobotDataManager.Instance.RobotStatus.event_OnEmotionStart.AddListener(SetEmotionSelected);
            RobotDataManager.Instance.RobotStatus.event_OnEmotionOver.AddListener(EmotionIsOver);
        }
 
        void Update()
        {
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out selectedDirection);

            float phi = Mathf.Atan2(selectedDirection[1], selectedDirection[0]);
            float r = Mathf.Sqrt(Mathf.Pow(selectedDirection[0], 2) + Mathf.Pow(selectedDirection[1], 2));

            if (!isEmotionSelected)
            {
                if (Mathf.Abs(phi) < (Mathf.PI / 8)) selectedEmotion = Emotion.Confused;
                else if ((phi > (Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.Happy;
                else if (Mathf.Abs(phi) > (Mathf.PI - Mathf.PI / 8)) selectedEmotion = Emotion.Sad;
                else if ((phi > (-Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (-Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.NoEmotion;
                if (r >= 0.5f && previousR < 0.5f) event_OnEmotionSelected.Invoke(selectedEmotion);
                previousR = r;
            }
            if ((phi > (-Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (-Mathf.PI / 2 + Mathf.PI / 8)))
            {
                selectedEmotion = Emotion.NoEmotion;
                if (r >= 0.5f && previousR < 0.5f) event_OnEmotionSelected.Invoke(selectedEmotion);
                previousR = r;
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