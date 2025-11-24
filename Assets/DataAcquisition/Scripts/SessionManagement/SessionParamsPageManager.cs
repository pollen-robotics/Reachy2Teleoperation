using UnityEngine;
using System.Collections.Generic;

namespace DataAcquisition
{
    public class SessionParamsPageManager : OrderedPagesManager
    {
        protected void Start()
        {
            if (DataAcquisitionManager.Instance != null)
            {
                DataAcquisitionManager.Instance.RecordingSessionManager.event_OnStartSessionOver.AddListener(ConfigurationOver);
            }
        }

        private void ConfigurationOver(bool success)
        {
            if (success) OpenPageByIndex(5);
        }
    }
}