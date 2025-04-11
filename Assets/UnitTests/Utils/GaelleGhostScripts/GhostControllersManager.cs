using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR.Interaction.Toolkit;


namespace TeleopReachy
{
    public class GhostControllersManager : ControllersManager
    {
        protected override void Start()
        {
            base.Start();
            rightHandDeviceIsTracked = true;
            leftHandDeviceIsTracked = true;
        }

        protected override void Update()
        {
            
        }
    }
}
