using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DataAcquisition
{
    public class PushSessionPageManager : OrderedPagesManager
    {
        private RecordingSessionManager sessionManager;

        protected override void Start()
        {
            base.Start();
            sessionManager = DataAcquisitionManager.Instance.RecordingSessionManager;
            sessionManager.event_OnPushOver.AddListener(IndicatePushIsOver);
            sessionManager.event_OnConsolidationOver.AddListener(IndicateConsolidationIsOver);
        }

        private void IndicatePushIsOver(bool state)
        {
            NextPage();
        }

        private void IndicateConsolidationIsOver(bool state)
        {
            if (sessionManager.pushRequested) NextPage(); 
            else OpenPageByName("AfterPush");
        }
    }
}
