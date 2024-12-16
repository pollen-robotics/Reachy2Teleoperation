using System;
using UnityEngine;
using UnityEngine.Events;

namespace TeleopReachy
{
    public class EmotionMenuManager : MonoBehaviour
    {
        private enum OnlineMenuItem
        {
            Cancel, Happy, Confused, Sad
        }

        public Transform Headset;
        public ControllersManager controllers;

        public Transform CancelIcon;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private OnlineMenuItem selectedItem;
        private Emotion selectedEmotion;

        private bool isOnlineMenuOpen;
        private bool canMenuOpen;

        private bool leftPrimaryButtonPreviouslyPressed;

        private Vector2 rightJoystickDirection;

        //private int nbEnum;

        public UnityEvent<Emotion> event_OnAskEmotion;

        public UnityEvent<Emotion> event_OnEmotionSelected;

        public UnityEvent event_OnNoEmotionSelected;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);

            canMenuOpen = true;
            controllers = ActiveControllerManager.Instance.ControllersManager;

            EventManager.StartListening(EventNames.OnStartEmotionTeleoperation, ActivateEmotion);
            EventManager.StartListening(EventNames.OnStopEmotionTeleoperation, DeactivateEmotion);
        }

        private void OnDisable()
        {
            EventManager.StopListening(EventNames.RobotDataSceneLoaded, Init);
        }

        private void Init()
        {
            // Headset = HeadsetPermanentTrackerWorldManager.Instance.transform;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideOnlineMenu);

            robotConfig = RobotDataManager.Instance.RobotConfig;
        }

        // Start is called before the first frame update
        void Start()
        {
            HideOnlineMenu();

            selectedItem = OnlineMenuItem.Cancel;
            selectedEmotion = Emotion.NoEmotion;

            //nbEnum = Enum.GetNames(typeof(OnlineMenuItem)).Length;
        }

        void ActivateEmotion()
        {
            canMenuOpen = true;
        }

        void DeactivateEmotion()
        {
            canMenuOpen = false;
            HideOnlineMenu();
        }

        // Update is called once per frame
        void Update()
        {
            bool leftPrimaryButtonPressed = false;

            if (canMenuOpen)
            {   
                if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && !leftPrimaryButtonPreviouslyPressed)
                {
                    if (!isOnlineMenuOpen)
                    {
                        ShowOnlineMenu();
                        HighlightCancel();
                        EventManager.TriggerEvent(EventNames.OnEmotionMode);
                        selectedItem = OnlineMenuItem.Cancel;
                    }

                    else
                    {
                        HideOnlineMenu();
                        EventManager.TriggerEvent(EventNames.OnMobilityMode);
                    }
                }
            }
                    
           
            if (robotConfig.IsVirtual() || robotConfig.HasHead())
            {
                switch (selectedItem)
                {
                    case OnlineMenuItem.Sad:
                        {
                            if (selectedEmotion != Emotion.Sad)
                            {
                                event_OnEmotionSelected.Invoke(Emotion.Sad);
                            }
                            selectedEmotion = Emotion.Sad;
                            RemoveHighlightCancel();
                            break;
                        }
                    case OnlineMenuItem.Confused:
                        {
                            if (selectedEmotion != Emotion.Confused)
                            {
                                event_OnEmotionSelected.Invoke(Emotion.Confused);
                            }
                            selectedEmotion = Emotion.Confused;
                            RemoveHighlightCancel();
                            break;
                        }
                    case OnlineMenuItem.Happy:
                        {
                            if (selectedEmotion != Emotion.Happy)
                            {
                                event_OnEmotionSelected.Invoke(Emotion.Happy);
                            }
                            selectedEmotion = Emotion.Happy;
                            RemoveHighlightCancel();
                            break;
                        }
                    case OnlineMenuItem.Cancel:
                        {
                            event_OnNoEmotionSelected.Invoke();
                            selectedEmotion = Emotion.NoEmotion;
                            HighlightCancel();
                            break;
                        }
                }
            }
                    

            if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && !leftPrimaryButtonPressed && leftPrimaryButtonPreviouslyPressed)
            {
                switch (selectedItem)
                {
                    case OnlineMenuItem.Happy:
                    case OnlineMenuItem.Confused:
                    case OnlineMenuItem.Sad:
                    {
                        if (!robotStatus.IsEmotionPlaying())
                        {
                            event_OnAskEmotion.Invoke(selectedEmotion);
                        }
                        break;
                    }
                }
            }

        }

        void HighlightCancel()
        {
            CancelIcon.localScale = new Vector3(1.5f * 0.3f, 1.5f * 0.3f, 1.5f);
        }

        void RemoveHighlightCancel()
        {
            CancelIcon.localScale = new Vector3(1.0f * 0.3f, 1.0f * 0.3f, 1.0f);
        }

        void ShowOnlineMenu()
        {
            if (robotConfig.IsVirtual() || robotConfig.HasHead())
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
            isOnlineMenuOpen = true;
        }

        void HideOnlineMenu()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            isOnlineMenuOpen = false;
        }
    }
}
