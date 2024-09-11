using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GstreamerWebRTC;
using TeleopReachy;

public class CopyRightImage : MonoBehaviour
{
    private GStreamerPluginCustom webRTCController;
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        webRTCController = WebRTCManager.Instance.webRTCController;
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rend.material.mainTexture = webRTCController.GetRightTexture();
    }
}
