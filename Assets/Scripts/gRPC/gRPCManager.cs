using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeleopReachy
{
    public class gRPCManager : Singleton<gRPCManager>
    {
        public ConnectionStatus ConnectionStatus { get; private set; }
        public gRPCDataController gRPCDataController { get; private set; }
        public gRPCMobileBaseController gRPCMobileBaseController { get; private set; }

        protected override void Init()
        {
            ConnectionStatus = GetComponent<ConnectionStatus>();
            gRPCDataController = GetComponent<gRPCDataController>();
            gRPCMobileBaseController = GetComponent<gRPCMobileBaseController>();
        }
    }
}
