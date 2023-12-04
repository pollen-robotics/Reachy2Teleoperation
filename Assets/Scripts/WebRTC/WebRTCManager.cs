using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeleopReachy
{
    public class WebRTCManager : Singleton<WebRTCManager>
    {
        // public ConnectionStatus ConnectionStatus { get; private set; }
        public WebRTCData webRTCDataController { get; private set; }
        // public gRPCMobileBaseController gRPCMobileBaseController { get; private set; }

        protected override void Init()
        {
            // ConnectionStatus = GetComponent<ConnectionStatus>();
            webRTCDataController = GetComponent<WebRTCData>();
            // gRPCMobileBaseController = GetComponent<gRPCMobileBaseController>();
        }
    }
}
