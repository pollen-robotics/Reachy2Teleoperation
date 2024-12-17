using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace TeleopReachy
{
    public class EmotionMenuManager : Singleton<EmotionMenuManager>
    {
        public Transform Headset;
        public ControllersManager controllers;

        public Transform CancelIcon;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private bool isEmotionMenuOpen;
        private bool canMenuOpen;

        private bool leftPrimaryButtonPreviouslyPressed;

        private Vector2 rightJoystickDirection;

        private Coroutine menuHidingCoroutine;
        private bool menuHidingRequested;

        //private int nbEnum;

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
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideImmediatelyEmotionMenu);
            UserInputManager.Instance.UserEmotionInput.event_OnEmotionSelected.AddListener(CheckCancel);

            robotConfig = RobotDataManager.Instance.RobotConfig;
        }

        // Start is called before the first frame update
        void Start()
        {
            HideImmediatelyEmotionMenu();
            menuHidingRequested = false;
        }

        void ActivateEmotion()
        {
            canMenuOpen = true;
        }

        void DeactivateEmotion()
        {
            canMenuOpen = false;
            menuHidingCoroutine = StartCoroutine(HideEmotionMenu());
        }

        void CheckCancel(Emotion emotion)
        {
            if (emotion == Emotion.NoEmotion)
            {
                HideImmediatelyEmotionMenu();
            }
        }

        // Update is called once per frame
        void Update()
        {
            bool leftPrimaryButtonPressed = false;

            if (canMenuOpen)
            {   
                if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && !leftPrimaryButtonPreviouslyPressed)
                {
                    if (!isEmotionMenuOpen)
                    {
                        ShowEmotionMenu();
                        HighlightCancel();
                        EventManager.TriggerEvent(EventNames.OnEmotionMode);
                    }

                    else
                    {
                        menuHidingCoroutine = StartCoroutine(HideEmotionMenu());
                        EventManager.TriggerEvent(EventNames.OnMobilityMode);
                    }
                }
                leftPrimaryButtonPreviouslyPressed = leftPrimaryButtonPressed;
            }
            if (menuHidingRequested)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                isEmotionMenuOpen = false;
                menuHidingRequested = false;
                EventManager.TriggerEvent(EventNames.OnMobilityMode);
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

        void ShowEmotionMenu()
        {
            if (robotConfig.IsVirtual() || robotConfig.HasHead())
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
            isEmotionMenuOpen = true;
        }

        IEnumerator HideEmotionMenu()
        {
            yield return new WaitForSeconds(0.5f);
            menuHidingRequested = true;
        }

        void HideImmediatelyEmotionMenu()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            isEmotionMenuOpen = false;
        }
    }
}
