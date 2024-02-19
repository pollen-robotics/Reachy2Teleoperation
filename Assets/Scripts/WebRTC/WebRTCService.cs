using Unity.WebRTC;
using UnityEngine;


namespace TeleopReachy {
    public class WebRTCService : GenericSingletonClass<WebRTCService>
    {
        void Start()
        {
            StartCoroutine(WebRTC.Update());
        }
    }
}
