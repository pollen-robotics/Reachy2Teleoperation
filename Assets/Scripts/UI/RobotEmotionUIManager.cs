using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotEmotionUIManager : MonoBehaviour
    {
        private RobotStatus robotStatus;
        private UserEmotionInput userEmotionInput;

        void OnEnable()
        {
            if (robotStatus == null && RobotDataManager.Instance != null)
            {
                Init();
            }
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
            robotStatus.event_OnEmotionOver.AddListener(delegate { HighlightNoEmotion(true); });

            userEmotionInput = UserInputManager.Instance.UserEmotionInput;
        }

        void HighlightSelectedEmotion()
        {
            HighlightNoEmotion(false);
            Emotion emotion = userEmotionInput.GetSelectedEmotion();
            transform.GetChild((int)emotion).localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.GetChild((int)emotion).GetComponent<RawImage>().color = new Color32(255, 255, 255, 150);
        }

        void HighlightNoEmotion(bool setEmotionsInteractable=true)
        {
            foreach (Transform child in transform)
            {
                child.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                if (setEmotionsInteractable)
                {
                    child.GetComponent<RawImage>().color = new Color32(255, 255, 255, 150);
                }
                else
                {
                    child.GetComponent<RawImage>().color = new Color32(70, 70, 70, 150);
                }
            }
        }
    }
}