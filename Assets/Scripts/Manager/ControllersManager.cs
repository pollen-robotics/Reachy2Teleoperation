using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR.Interaction.Toolkit;


namespace TeleopReachy
{
    public class ControllersManager : Singleton<ControllersManager>
    {
        public enum SupportedDevices
        {
            Oculus, HTCVive, ValveIndex, MetaQuest3
        }

        public UnityEngine.XR.InputDevice rightHandDevice;
        public UnityEngine.XR.InputDevice leftHandDevice;
        public UnityEngine.XR.InputDevice headDevice;

        public SupportedDevices controllerDeviceType;
        public SupportedDevices headsetType;

        public UnityEvent event_OnDevicesUpdate;

        void Start()
        {
            UpdateDevicesList();

            UnityEngine.XR.InputDevices.deviceConnected += UpdateDevicesList;
        }

        private void UpdateDevicesList(UnityEngine.XR.InputDevice device)
        {
            UpdateDevicesList();
        }

        private void UpdateDevicesList()
        {
            var rightDevices = new List<UnityEngine.XR.InputDevice>();
            var leftDevices = new List<UnityEngine.XR.InputDevice>();
            var headDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightDevices);
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftDevices);
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.Head, headDevices);

            if (rightDevices.Count == 1) rightHandDevice = rightDevices[0];
            else if (rightDevices.Count > 1) Debug.LogError("Too many right controllers detected");
            if (leftDevices.Count == 1) leftHandDevice = leftDevices[0];
            else if (leftDevices.Count > 1) Debug.LogError("Too many left controllers detected");

            if(headDevices.Count == 1) 
            {
                headDevice = headDevices[0];
                if(headDevice.name.Contains("Oculus")) headsetType = SupportedDevices.Oculus;
                if(headDevice.name.Contains("Meta")) headsetType = SupportedDevices.MetaQuest3;
            }

            if(rightDevices.Count != 0)
            {
                if(rightHandDevice.name.Contains("Oculus"))
                {
                    controllerDeviceType = SupportedDevices.Oculus;
                }
                if(rightHandDevice.name.Contains("Vive"))
                {
                    controllerDeviceType = SupportedDevices.HTCVive;
                }
                if(rightHandDevice.name.Contains("Index"))
                {
                    controllerDeviceType = SupportedDevices.ValveIndex;
                }
            }

            event_OnDevicesUpdate.Invoke();
        }
    }
}
