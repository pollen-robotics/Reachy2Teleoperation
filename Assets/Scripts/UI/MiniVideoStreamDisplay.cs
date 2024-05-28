using UnityEngine;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class MiniVideoStreamDisplay : MonoBehaviour
    {
        private GStreamerPluginCustom audioVideoController;
        private Renderer screen;

        // Used to update display when going back in mirror scene
        void Start()
        {
            screen = GetComponent<Renderer>();
            audioVideoController = WebRTCManager.Instance.webRTCVideoController;
            screen.material.SetTexture("_LeftTex", audioVideoController.GetLeftTexture());
            screen.material.SetTexture("_RightTex", audioVideoController.GetLeftTexture());
        }

    }
}
