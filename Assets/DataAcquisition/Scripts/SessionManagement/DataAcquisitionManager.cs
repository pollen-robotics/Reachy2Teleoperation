using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Grpc.Core;
using System.Threading.Tasks;

using Data.Acquisition;


namespace DataAcquisition
{
    public class DataAcquisitionManager : Singleton<DataAcquisitionManager>
    {
        public gRPCDataController DataController { get; private set; }
        public RecordingSessionManager RecordingSessionManager { get; private set; }
        public RecordingSessionParameters RecordingSessionParameters { get; private set; }

        protected override void Init()
        {
            DataController = GetComponent<gRPCDataController>();
            RecordingSessionManager = GetComponent<RecordingSessionManager>();
            RecordingSessionParameters = GetComponent<RecordingSessionParameters>();
        }
    }
}