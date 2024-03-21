using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private WebRTCAVReceiver audioVideoController;

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