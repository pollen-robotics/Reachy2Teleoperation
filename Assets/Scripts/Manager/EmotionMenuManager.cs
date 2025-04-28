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

        private void InitManager()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideImmediatelyEmotionMenu);
            robotStatus.event_OnEmotionStart.AddListener(HideAfterSeconds);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            canMenuOpen = true;
        }

        void Start()
        {
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, InitManager);
            EventManager.StartListening(EventNames.EnterConnectionScene, DisableEmotionMenu);
            EventManager.StartListening(EventNames.EnterTeleoperationScene, HideImmediatelyEmotionMenu);

            controllers = ActiveControllerManager.Instance.ControllersManager;

            EventManager.StartListening(EventNames.OnStartEmotionTeleoperation, ActivateEmotion);
            EventManager.StartListening(EventNames.OnStopEmotionTeleoperation, DeactivateEmotion);

            HideImmediatelyEmotionMenu();
            canMenuOpen = false;
            menuHidingRequested = false;
        }

        private void DisableEmotionMenu()
        {
            if (menuHidingCoroutine != null) StopCoroutine(menuHidingCoroutine);
            HideImmediatelyEmotionMenu();
            canMenuOpen = false;
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
                transform.ActivateChildren(false);
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
                transform.ActivateChildren(true);
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
            transform.ActivateChildren(false);
            isEmotionMenuOpen = false;
            canMenuOpen = true;
        }
    }
}
