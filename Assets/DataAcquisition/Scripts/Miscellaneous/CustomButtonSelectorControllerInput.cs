using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DataAcquisition
{
    public class CustomButtonSelectorControllerInput : MonoBehaviour
    {
        private float previousR;
        private CustomButtonSelector buttonSelector;

        private TeleopReachy.ControllersManager controllers;

        void Start()
        {
            controllers = TeleopReachy.ControllersManager.Instance;

            buttonSelector = GetComponent<CustomButtonSelector>();
            Vector2 selectedDirection;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out selectedDirection);
            previousR = Mathf.Sqrt(Mathf.Pow(selectedDirection[0], 2) + Mathf.Pow(selectedDirection[1], 2));
        }

        void Update()
        {
            Vector2 selectedDirection;
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out selectedDirection);
            float phi = Mathf.Atan2(selectedDirection[1], selectedDirection[0]);
            float r = Mathf.Sqrt(Mathf.Pow(selectedDirection[0], 2) + Mathf.Pow(selectedDirection[1], 2));

            if (r >= 0.5f && previousR < 0.5f) 
            {
                if (Mathf.Abs(phi) < (Mathf.PI / 8)) buttonSelector.failButton.GetComponent<Button>().onClick?.Invoke();
                else if (r >= 0.5f && (Mathf.Abs(phi) > (Mathf.PI - Mathf.PI / 8))) 
                {
                    buttonSelector.SelectSuccessButton();
                    DataAcquisitionManager.Instance.RecordingSessionManager.ResumeCurrentPhase();
                    DataAcquisitionManager.Instance.RecordingSessionManager.ClosePanelByName("DeleteEpisodePanel");
                }
            }   
            previousR = r;
        }
    }
}