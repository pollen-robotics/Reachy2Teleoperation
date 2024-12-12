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
    public class RobotReachabilityUIManager : MonoBehaviour
    {
        private enum Arm
        {
            Right, Left
        }

        [SerializeField]
        private Arm armSide;

        [SerializeField]
        protected Text infoText;

        protected Coroutine infoPanelDisplay;

        protected bool needInfoPanelUpdate;

        protected string textToDisplay;

        protected int displayDuration = 3;

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
                infoPanelDisplay = StartCoroutine(HidePanelAfterSeconds(displayDuration, transform));

                needInfoPanelUpdate = false;
            }
        }

        protected virtual void ShowInfoMessage()
        {
            needInfoPanelUpdate = true;
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
            reachabilityManager = RobotDataManager.Instance.RobotReachabilityManager;
            if (armSide == Arm.Left) reachabilityManager.event_OnLArmPositionUnreachable.AddListener(HandleReachabilityError);
            else reachabilityManager.event_OnRArmPositionUnreachable.AddListener(HandleReachabilityError);

            HideInfoMessage();
        }

        private void HandleReachabilityError(ReachabilityError error)
        {
            switch (error)
            {
                case ReachabilityError.DistanceLimit:
                    textToDisplay = "arm is too short";
                    break;
                case ReachabilityError.ShoulderLimit:
                    textToDisplay = "shoulder limit reached";
                    break;
                case ReachabilityError.ElbowLimit:
                    textToDisplay = "elbow limit reached";
                    break;
                case ReachabilityError.WristLimit:
                    textToDisplay = "wrist limit reached";
                    break;
                case ReachabilityError.SingularityAvoidance:
                    textToDisplay = "workspace limit reached";
                    break;
                case ReachabilityError.Other:
                    textToDisplay = "elbow elevation limit reached";
                    break;
                default:
                    textToDisplay = "pose cannot be reached";
                    break;
            }
            ShowInfoMessage();
        }
    }
}
