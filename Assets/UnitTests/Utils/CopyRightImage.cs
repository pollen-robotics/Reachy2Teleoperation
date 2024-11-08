using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GstreamerWebRTC;
using TeleopReachy;

public class CopyRightImage : MonoBehaviour
{
    private GStreamerPluginCustom gstreamerPlugin;
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        gstreamerPlugin = WebRTCManager.Instance.gstreamerPlugin;
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rend.material.mainTexture = gstreamerPlugin.GetRightTexture();
    }
}
