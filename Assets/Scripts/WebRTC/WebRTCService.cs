using Unity.WebRTC;
using UnityEngine;


namespace TeleopReachy {
    public class WebRTCService : GenericSingletonClass<WebRTCService>
    {
        void Start()
        {
            StartCoroutine(WebRTC.Update());
        }

        public void AskForWebRTCDisconnection()
        {
            WebRTCManager.Instance.webRTCDataController.Disconnection();
            WebRTCManager.Instance.webRTCVideoController.Disconnection();
            WebRTCManager.Instance.webRTCAudioSender.Disconnection();
            Destroy(gameObject);
        }
    }
}
