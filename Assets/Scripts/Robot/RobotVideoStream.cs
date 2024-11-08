using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GstreamerWebRTC;


namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private GStreamerPluginCustom videoController;


        void Start()
        {
            videoController = WebRTCManager.Instance.gstreamerPlugin;
        }

        public Texture GetLeftTexture()
        {
            return videoController.GetLeftTexture();
        }

        public Texture GetRightTexture()
        {
            return videoController.GetLeftTexture();
        }
    }
}