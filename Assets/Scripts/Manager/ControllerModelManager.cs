using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR.Interaction.Toolkit;


namespace TeleopReachy
{
    public class ControllerModelManager : MonoBehaviour
    {
        private RobotStatus robotStatus;

        private void Start()
        {
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
        }

        private void Init()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(HideControllers);
            robotStatus.event_OnStopTeleoperation.AddListener(ShowControllers);
        }

        void HideControllers()
        {
            transform.ActivateChildren(false);
        }

        void ShowControllers()
        {
            transform.ActivateChildren(true);
        }
    }
}


