using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Grpc.Core;


namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private WebRTCAVReceiver audioVideoController;

        private Texture leftEyeStream;

        void Start()
        {
            audioVideoController = WebRTCManager.Instance.webRTCVideoController;
            audioVideoController.event_OnVideoTextureReceived.AddListener(UpdateVideoStream);
        }

        void UpdateVideoStream (Texture tex) 
        {
            leftEyeStream = tex;
        }

        public Texture GetLeftEyeTexture() 
        {
            return leftEyeStream;
        }
    }
}