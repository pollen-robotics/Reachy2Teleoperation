using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Reachy.Part.Arm;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class ControllerTrackingUIManager : MonoBehaviour
    {
        private enum Arm
        {
            Right, Left
        }

        [SerializeField]
        private Arm armSide;

        [SerializeField]
        protected Text errorText;

        [SerializeField]
        protected Text infoText;

        protected Coroutine infoPanelDisplay;

        protected bool needInfoPanelUpdate;

        protected string textToDisplay;
        protected string errorToDisplay;

        protected bool toHideAfterSeconds;

        protected int displayDuration = 4;

        protected Color32 backgroundColor = ColorsManager.error_black;

        protected void SetMinimumTimeDisplayed(int seconds)
        {
            displayDuration = seconds;
        }

        protected virtual void Update()
        {
            if (needInfoPanelUpdate)
            {
                if (infoPanelDisplay != null) StopCoroutine(infoPanelDisplay);
                transform.ActivateChildren(true);
                infoText.text = textToDisplay;
                errorText.text = errorToDisplay;

                if (toHideAfterSeconds) infoPanelDisplay = StartCoroutine(HidePanelAfterSeconds(displayDuration, transform));

                needInfoPanelUpdate = false;
            }
        }

        protected virtual void ShowInfoMessage()
        {
            if (TeleoperationManager.Instance.IsArmTeleoperationActive &&
                (((armSide == Arm.Left) && RobotDataManager.Instance.RobotStatus.IsLeftArmOn()) ||
                ((armSide == Arm.Right) && RobotDataManager.Instance.RobotStatus.IsRightArmOn()))
            )
            {
                needInfoPanelUpdate = true;
            }
        }

        protected virtual void HideInfoMessage()
        {
            if (infoPanelDisplay != null) StopCoroutine(infoPanelDisplay);
            transform.ActivateChildren(false);
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }

        private RobotReachabilityManager reachabilityManager;

        void Start()
        {
            if (armSide == Arm.Left)
            {
                EventManager.StartListening(EventNames.LeftControllerTrackingLost, TrackingLost);
                EventManager.StartListening(EventNames.LeftControllerTrackingRetrieved, TrackingRetrieved);
                EventManager.StartListening(EventNames.OnStartArmTeleoperation, CheckTrackingState);
            }
            else
            {
                EventManager.StartListening(EventNames.RightControllerTrackingLost, TrackingLost);
                EventManager.StartListening(EventNames.RightControllerTrackingRetrieved, TrackingRetrieved);
                EventManager.StartListening(EventNames.OnStartArmTeleoperation, CheckTrackingState);
            }

            HideInfoMessage();
        }

        private void CheckTrackingState()
        {
            if (armSide == Arm.Left && !ControllersManager.Instance.leftHandDeviceIsTracked) TrackingLost();
            if (armSide == Arm.Right && !ControllersManager.Instance.rightHandDeviceIsTracked) TrackingLost();
        }

        private void TrackingLost()
        {
            errorToDisplay = armSide.ToString() + " controller tracking lost:";
            textToDisplay = "arm movements stopped";
            toHideAfterSeconds = false;

            ShowInfoMessage();
        }

        private void TrackingRetrieved()
        {
            errorToDisplay = armSide.ToString() + " controller tracking retrieved.";
            textToDisplay = "Resetting arm full speed...";
            toHideAfterSeconds = true;

            ShowInfoMessage();
        }
    }
}
