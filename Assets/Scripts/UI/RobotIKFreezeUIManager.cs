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
    public class RobotIKFreezeUIManager : InformationalPanel
    {
        private RobotReachabilityManager reachabilityManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, 0f, 0.5f));

            reachabilityManager = RobotDataManager.Instance.RobotReachabilityManager;
            reachabilityManager.event_OnArmIKFreeze.AddListener(HandleIKFreezeError);

            HideInfoMessage();
        }

        private void HandleIKFreezeError(ReachabilityError error)
        {
            switch (error)
            {
                case ReachabilityError.DiscontinuityFreeze:
                    textToDisplay = "discontinuity detected";
                    break;
                case ReachabilityError.MultiturnFreeze:
                    textToDisplay = "multiturn command detected";
                    break;
                default:
                    textToDisplay = "pose cannot be reached";
                    break;
            }
            ShowInfoMessage();
        }
    }
}
