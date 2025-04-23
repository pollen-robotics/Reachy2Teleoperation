using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace TeleopReachy
{
    public class EmotionMenuManager : Singleton<EmotionMenuManager>
    {
        // public Transform Headset;
        private ControllersManager controllers;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private bool isEmotionMenuOpen;
        private bool canMenuOpen;

        private bool leftPrimaryButtonPreviouslyPressed;

        private Vector2 rightJoystickDirection;

        private Coroutine menuHidingCoroutine;
        private bool menuHidingRequested;

        private void Init()
        {
            // Headset = HeadsetPermanentTrackerWorldManager.Instance.transform;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideImmediatelyEmotionMenu);
            robotStatus.event_OnEmotionStart.AddListener(HideAfterSeconds);
            // UserInputManager.Instance.UserEmotionInput.event_OnEmotionSelected.AddListener(CheckCancel);

            robotConfig = RobotDataManager.Instance.RobotConfig;
        }

        // Start is called before the first frame update
        void Start()
        {
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);

            canMenuOpen = true;
            controllers = ActiveControllerManager.Instance.ControllersManager;

            EventManager.StartListening(EventNames.OnStartEmotionTeleoperation, ActivateEmotion);
            EventManager.StartListening(EventNames.OnStopEmotionTeleoperation, DeactivateEmotion);

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
            if (menuHidingCoroutine != null) StopCoroutine(menuHidingCoroutine);
            menuHidingCoroutine = StartCoroutine(HideEmotionMenu());
        }

        // void CheckCancel(Emotion emotion)
        // {
        //     if (emotion == Emotion.NoEmotion)
        //     {
        //         if (menuHidingCoroutine != null) StopCoroutine(menuHidingCoroutine);
        //         menuHidingCoroutine = StartCoroutine(HideEmotionMenu());
        //     }
        // }

        // Update is called once per frame
        void Update()
        {
            bool leftPrimaryButtonPressed = false;

            if (canMenuOpen)
            {   
                if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && leftPrimaryButtonPressed && !leftPrimaryButtonPreviouslyPressed)
                {
                    if (!isEmotionMenuOpen)
                    {
                        ShowEmotionMenu();
                        EventManager.TriggerEvent(EventNames.OnEmotionMode);
                    }

                    else
                    {
                        HideImmediatelyEmotionMenu();
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

        void HideAfterSeconds()
        {
            if (menuHidingCoroutine != null) StopCoroutine(menuHidingCoroutine);
            menuHidingCoroutine = StartCoroutine(HideEmotionMenu());
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
            if (menuHidingCoroutine != null) StopCoroutine(menuHidingCoroutine);
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            isEmotionMenuOpen = false;
            canMenuOpen = true;
        }
    }
}
