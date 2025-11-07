using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using TeleopReachy;

namespace DataAcquisition
{
    public class ChooseFooterOnDatasetType : MonoBehaviour
    {
        public Transform existingDatasetFooter;
        public Transform newDatasetFooter;

        public RecordingSessionSetup sessionSetup;

        void OnEnable()
        {
            existingDatasetFooter.ActivateChildren(!sessionSetup.isNewDataset);
            newDatasetFooter.ActivateChildren(sessionSetup.isNewDataset);
        }
    }
}
