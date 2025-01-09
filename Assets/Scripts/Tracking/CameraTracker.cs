using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class CameraTracker : MonoBehaviour
    {
        public enum FollowMode
        {
            TranslationOnly,
            All,
            RotationOnFrameUpdate, // recenter the sphere when a camera frame is rendered. otherwise the user can move freely
        };

        public FollowMode mode = FollowMode.RotationOnFrameUpdate;

        // Start is called before the first frame update
        private GStreamerPluginCustom videoController;

        void Start()
        {
            videoController = WebRTCManager.Instance.gstreamerPlugin;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = Camera.main.transform.position;
            if (mode == FollowMode.All || (mode == FollowMode.RotationOnFrameUpdate && videoController.IsFrameRendered()))
            {
                transform.rotation = Camera.main.transform.rotation;
                transform.Rotate(180, 90, 0);
                videoController.ResetFrameRendered();
            }
        }
    }
}
