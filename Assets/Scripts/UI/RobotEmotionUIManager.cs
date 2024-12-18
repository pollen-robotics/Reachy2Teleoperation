using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotEmotionUIManager : MonoBehaviour
    {
        private RobotStatus robotStatus;
        private UserEmotionInput userEmotionInput;

        void Awake()
        {
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
        }

        void OnEnable()
        {
            if (robotStatus != null)
            {
                if (robotStatus.IsEmotionPlaying()) HighlightSelectedEmotion();
                else HighlightNoEmotion();
            }
        }

        private void Init()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnEmotionStart.AddListener(HighlightSelectedEmotion);
            robotStatus.event_OnEmotionOver.AddListener(HighlightNoEmotion);

            userEmotionInput = UserInputManager.Instance.UserEmotionInput;
            // userEmotionInput.event_OnEmotionSelected.AddListener(CheckNoEmotion);
        }

        // void CheckNoEmotion(Emotion emotion)
        // {
        //     if (emotion == Emotion.NoEmotion) HighlightSelectedEmotion();
        // }

        void HighlightSelectedEmotion()
        {
            HighlightNoEmotion();
            Emotion emotion = userEmotionInput.GetSelectedEmotion();
            transform.GetChild((int)emotion).localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        void HighlightNoEmotion()
        {
            foreach (Transform child in transform)
            {
                child.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
    }
}