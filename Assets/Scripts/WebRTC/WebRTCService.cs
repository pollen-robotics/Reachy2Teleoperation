using Unity.WebRTC;
using UnityEngine;
using System.Collections;


namespace TeleopReachy {
    public class WebRTCService : GenericSingletonClass<WebRTCService>
    {
        void Start()
        {
            StartCoroutine(WebRTC.Update());
        }
    }
}
