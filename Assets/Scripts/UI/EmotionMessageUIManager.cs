using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class EmotionMessageUIManager : InformationalPanel
    {
        [SerializeField]
        private Texture sadImage;

        [SerializeField]
        private Texture happyImage;

        [SerializeField]
        private Texture confusedImage;

        private Texture emojiToDisplay;

        [SerializeField]
        private RawImage emoji;

        private RobotStatus robotStatus;

        private Dictionary<Emotion, Texture> emotionImages;

        private EmotionMenuManager emotionMenuManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.24f, 0.8f));

            emotionImages = new Dictionary<Emotion, Texture>();
            emotionImages.Add(Emotion.Sad, sadImage);
            emotionImages.Add(Emotion.Happy, happyImage);
            emotionImages.Add(Emotion.Confused, confusedImage);

            HideInfoMessage();
        }

        void Init()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnEmotionStart.AddListener(ChooseEmotionAndShow);
        }

        protected override void Update()
        {
            if(needInfoPanelUpdate) emoji.texture = emojiToDisplay;
            base.Update();
        }

        void ChooseEmotionAndShow()
        {
            Emotion emotion = UserInputManager.Instance.UserEmotionInput.GetSelectedEmotion();
            switch (emotion)
            {
                case Emotion.Sad:
                    textToDisplay = "Emotion sad is playing";
                    emojiToDisplay = emotionImages[Emotion.Sad];
                    break;

                case Emotion.Happy:
                    textToDisplay = "Emotion happy is playing";
                    emojiToDisplay = emotionImages[Emotion.Happy];
                    break;

                case Emotion.Confused:
                    textToDisplay = "Emotion confused is playing";
                    emojiToDisplay = emotionImages[Emotion.Confused];
                    break;
            }
            ShowInfoMessage();
        }
    }
}