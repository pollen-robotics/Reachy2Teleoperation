using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Reachy.Part.Arm;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System;


namespace TeleopReachy
{
    public class RobotReachabilityUIManager : LazyFollow
    {
        private Dictionary<string, float> panelTemperature;
        private Dictionary<string, string> panelStatus;

        private bool isRArmPanelStatusActive;
        private bool needUpdateRArmPanelInfo;
        private bool isLArmPanelStatusActive;
        private bool needUpdateLArmPanelInfo;

        private RobotReachabilityManager reachabilityManager;

        string lArmMessage;
        string rArmMessage;

        [SerializeField]
        private Transform leftArmPanel;
        [SerializeField]
        private Transform rightArmPanel;

        private Coroutine leftArmPanelDisplay;
        private Coroutine rightArmPanelDisplay;

        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, 0f, 0.5f);
            }
            else
            {
                targetOffset = new Vector3(0, 0f, 0.7f);
            }
            maxDistanceAllowed = 0;

            reachabilityManager = RobotDataManager.Instance.RobotReachabilityManager;
            reachabilityManager.event_OnLArmPositionUnreachable.AddListener(HandleLeftArmReachabilityError);
            reachabilityManager.event_OnRArmPositionUnreachable.AddListener(HandleRightArmReachabilityError);

            EventManager.StartListening(EventNames.OnStopTeleoperation, HideMessages);

            HideMessages();
        }

        private void HandleLeftArmReachabilityError(ReachabilityError error)
        {
            SelectMessage(error, ref lArmMessage);
            needUpdateLArmPanelInfo = true;
        }


        private void HandleRightArmReachabilityError(ReachabilityError error)
        {
            SelectMessage(error, ref rArmMessage);
            needUpdateRArmPanelInfo = true;
        }

        void SelectMessage(ReachabilityError error, ref string message)
        {
            if(error == ReachabilityError.DistanceLimit)
            {
                message = "arm is too short";
            }
            if(error == ReachabilityError.ShoulderLimit)
            {
                message = "shoulder limit reached";
            }
            if(error == ReachabilityError.ElbowLimit)
            {
                message = "elbow limit reached";
            }
            if(error == ReachabilityError.WristLimit)
            {
                message = "wrist limit reached";
            }
            if(error == ReachabilityError.ContinuityLimit)
            {
                message = "discontinuity issue";
                Debug.Log("discontinuity required to reach pose");
            }
            if(error == ReachabilityError.Other)
            {
                message = "elbow elevation limit reached";
            }
        }

        void Update()
        {
            if (needUpdateLArmPanelInfo)
            {
                if (leftArmPanelDisplay != null) StopCoroutine(leftArmPanelDisplay);
                leftArmPanel.ActivateChildren(true);
                leftArmPanel.GetChild(2).GetComponent<Text>().text = lArmMessage;
                leftArmPanelDisplay = StartCoroutine(HidePanelAfterSeconds(1, leftArmPanel));
                needUpdateLArmPanelInfo = false;
            }
            if (needUpdateRArmPanelInfo)
            {
                if (rightArmPanelDisplay != null) StopCoroutine(rightArmPanelDisplay);
                rightArmPanel.ActivateChildren(true);
                rightArmPanel.GetChild(2).GetComponent<Text>().text = rArmMessage;
                rightArmPanelDisplay = StartCoroutine(HidePanelAfterSeconds(1, rightArmPanel));
                needUpdateRArmPanelInfo = false;
            }
        }

        void HideMessages()
        {
            if (leftArmPanelDisplay != null) StopCoroutine(leftArmPanelDisplay);
            if (rightArmPanelDisplay != null) StopCoroutine(rightArmPanelDisplay);
            leftArmPanel.ActivateChildren(false);
            rightArmPanel.ActivateChildren(false);
        }

        void HideMessages(object sender, EventArgs e)
        {
            HideMessages();
        }

        IEnumerator HidePanelAfterSeconds(int seconds, Transform masterPanel)
        {
            yield return new WaitForSeconds(seconds);
            masterPanel.ActivateChildren(false);
        }
    }
}
