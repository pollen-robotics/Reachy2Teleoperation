using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private WebRTCAVReceiver audioVideoController;
        private bool update_mini_viewer = false;

        private Texture leftEyeStream;

        void Start()
        {
            audioVideoController = WebRTCManager.Instance.webRTCVideoController;
            audioVideoController.event_OnVideoTextureReceived.AddListener(UpdateVideoStream);
        }

        void UpdateVideoStream(Texture tex)
        {
            leftEyeStream = tex;
            update_mini_viewer = true;
        }

        void Update()
        {
            if (update_mini_viewer)
            {
                update_mini_viewer = false;
                GameObject miniViewer = GameObject.Find("VideoStreamMini");
                if (miniViewer != null)
                {
                    miniViewer.GetComponent<Renderer>().material.SetTexture("_LeftTex", leftEyeStream);
                    miniViewer.GetComponent<Renderer>().material.SetTexture("_RightTex", leftEyeStream);
                }
            }
        }

        public Texture GetLeftEyeTexture()
        {
            return leftEyeStream;
        }
    }
}