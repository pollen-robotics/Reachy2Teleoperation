using UnityEngine;
using System.Collections.Generic;

namespace DataAcquisition
{
    public class SessionParamsPageManager : OrderedPagesManager
    {
        protected override void Start()
        {
            base.Start();
            if (DataAcquisitionManager.Instance != null)
            {
                DataAcquisitionManager.Instance.RecordingSessionManager.event_OnStartSessionOver.AddListener(ConfigurationOver);
            }
        }

        private void ConfigurationOver(bool success)
        {
            if (success) NextPage();
        }
    }
}