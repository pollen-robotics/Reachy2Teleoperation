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
    public class RobotReachabilityUIManager : InformationalPanel
    {
        private enum Arm
        {
            Right, Left
        }

        [SerializeField]
        private Arm armSide;

        private RobotReachabilityManager reachabilityManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, 0f, 0.5f));

            reachabilityManager = RobotDataManager.Instance.RobotReachabilityManager;
            if(armSide == Arm.Left) reachabilityManager.event_OnLArmPositionUnreachable.AddListener(HandleReachabilityError);
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
