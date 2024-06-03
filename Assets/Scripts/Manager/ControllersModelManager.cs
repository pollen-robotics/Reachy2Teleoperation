using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR.Interaction.Toolkit;


namespace TeleopReachy
{
    public class ControllersModelManager : MonoBehaviour
    {
        private ControllersManager controllers;

        public GameObject AButton;
        public GameObject BButton;
        public GameObject XButton;
        public GameObject YButton;
        public GameObject LThumstick;
        public GameObject RThumstick;

        public Transform canvaAButton;

        public Transform headsetOrientation;

        public Material pressedButton;
        public Material touchedButton;

        void Start()
        {
            controllers = ControllersManager.Instance;
            AButton.SetActive(false);
            BButton.SetActive(false);
            XButton.SetActive(false);
            YButton.SetActive(false);
            LThumstick.SetActive(false);
            RThumstick.SetActive(false);
        }

        private void Update()
        {
            bool rightPrimaryButtonTouched;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryTouch, out rightPrimaryButtonTouched);
            AButton.SetActive(rightPrimaryButtonTouched);

            bool rightPrimaryButtonPressed;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed);
            if(rightPrimaryButtonPressed) 
            {
                AButton.transform.localScale = new Vector3(0.012f, 0.006f, 0.012f);
                AButton.GetComponent<Renderer>().material = pressedButton;
            }
            else 
            {
                AButton.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                AButton.GetComponent<Renderer>().material = touchedButton;
            }
            
            bool rightSecondaryButtonTouched;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryTouch, out rightSecondaryButtonTouched);
            BButton.SetActive(rightSecondaryButtonTouched);

            bool rightSecondaryButtonPressed;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out rightSecondaryButtonPressed);
            if(rightSecondaryButtonPressed)
            {
                BButton.transform.localScale = new Vector3(0.012f, 0.006f, 0.012f);
                BButton.GetComponent<Renderer>().material = pressedButton;
            }
            else
            {
                BButton.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                BButton.GetComponent<Renderer>().material = touchedButton;
            }

            bool rightPrimary2DAxisTouched;
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out rightPrimary2DAxisTouched);
            RThumstick.SetActive(rightPrimary2DAxisTouched);

            bool leftPrimaryButtonTouched;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryTouch, out leftPrimaryButtonTouched);
            XButton.SetActive(leftPrimaryButtonTouched);

            bool leftPrimaryButtonPressed;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed);
            if(leftPrimaryButtonPressed)
            {
                XButton.transform.localScale = new Vector3(0.012f, 0.006f, 0.012f);
                XButton.GetComponent<Renderer>().material = pressedButton;
            }
            else
            {
                XButton.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                XButton.GetComponent<Renderer>().material = touchedButton;
            }

            bool leftSecondaryButtonTouched;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryTouch, out leftSecondaryButtonTouched);
            YButton.SetActive(leftSecondaryButtonTouched);

            bool leftSecondaryButtonPressed;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftSecondaryButtonPressed);
            if(leftSecondaryButtonPressed)
            {
                YButton.transform.localScale = new Vector3(0.012f, 0.006f, 0.012f);
                YButton.GetComponent<Renderer>().material = pressedButton;
            }
            else
            {
                YButton.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                YButton.GetComponent<Renderer>().material = touchedButton;
            }

            bool leftPrimary2DAxisTouched;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out leftPrimary2DAxisTouched);
            LThumstick.SetActive(leftPrimary2DAxisTouched);
        }
    }
}
