using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using TeleopReachy;

namespace DataAcquisition
{
    public class ChooseEndTextOnPush : MonoBehaviour
    {
        public Transform pushDatasetText;
        public Transform noPushDatasetText;

        void OnEnable()
        {
            pushDatasetText.ActivateChildren(DataAcquisitionManager.Instance.RecordingSessionManager.pushRequested);
            noPushDatasetText.ActivateChildren(!DataAcquisitionManager.Instance.RecordingSessionManager.pushRequested);
        }
    }
}
