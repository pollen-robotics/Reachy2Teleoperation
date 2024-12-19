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
        private void Start()
        {
            EventManager.StartListening(EventNames.OnStartTeleoperation, HideControllers);
            EventManager.StartListening(EventNames.OnStopTeleoperation, ShowControllers);
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


