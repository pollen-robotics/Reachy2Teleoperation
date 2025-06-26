using UnityEngine;
using System.Collections.Generic;

namespace DataAcquisition
{
    public class SessionParamsPageManager : OrderedPagesManager
    {
        public Transform advancedSettings;

        protected override void Start()
        {
            base.Start();
            if (DataAcquisitionManager.Instance != null)
            {
                DataAcquisitionManager.Instance.RecordingSessionManager.event_OnStartSessionOver.AddListener(ConfigurationOver);
            }
        }

        public void OpenAdvancedSettings()
        {
            advancedSettings.gameObject.SetActive(true);
        }

        public void CloseAdvancedSettings()
        {
            advancedSettings.gameObject.SetActive(false);
        }

        private void ConfigurationOver(bool success)
        {
            if (success) NextPage();
        }
    }
}