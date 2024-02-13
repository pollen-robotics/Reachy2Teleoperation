using Unity.WebRTC;
using UnityEngine;
using System.Collections;


namespace TeleopReachy {
    public class WebRTCService : GenericSingletonClass<WebRTCService>
    {

        Coroutine dataDisconnection;
        Coroutine videoDisconnection;
        Coroutine audioDisconnection;

        void Start()
        {
            StartCoroutine(WebRTC.Update());
        }

        public IEnumerator AskForWebRTCDisconnection()
        {
            dataDisconnection = StartCoroutine(WebRTCManager.Instance.webRTCDataController.Disconnection());
            videoDisconnection = StartCoroutine(WebRTCManager.Instance.webRTCVideoController.Disconnection());
            audioDisconnection = StartCoroutine(WebRTCManager.Instance.webRTCAudioSender.Disconnection());
            if (dataDisconnection != null) yield return dataDisconnection;
            if (videoDisconnection != null) yield return videoDisconnection;
            if (audioDisconnection != null) yield return audioDisconnection;
            yield return null;
            Destroy(gameObject);
        }
    }
}
