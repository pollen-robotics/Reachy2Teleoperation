using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private GstreamerUnityGStreamerPlugin audioVideoController;

        public UnityEvent event_OnVideoTextureReady;
        private Texture leftEyeStream;

        void Start()
        {
            audioVideoController = WebRTCManager.Instance.webRTCVideoController;
            audioVideoController.event_OnVideoTextureReceived.AddListener(UpdateVideoStream);
        }

        void UpdateVideoStream(Texture tex)
        {
            leftEyeStream = tex;
            event_OnVideoTextureReady.Invoke();
        }

        public Texture GetLeftEyeTexture()
        {
            return leftEyeStream;
        }
    }
}