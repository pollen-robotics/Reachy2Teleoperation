using UnityEngine;
using UnityEngine.UI;

namespace GstreamerWebRTC
{
    public class GStreamerPlugin : MonoBehaviour
    {

        [Tooltip("RawImage on which the left texture will be rendered")]
        public RawImage leftRawImage;
        [Tooltip("RawImage on which the right texture will be rendered")]
        public RawImage rightRawImage;
        protected GStreamerRenderingPlugin renderingPlugin = null;
        protected DebugFromPlugin debug = null;

        [Tooltip("IP address of the robot (i.e. signalling server). PlayerPrefs.GetString(\"ip_address\") if empty")]
        public string ip_address = "";


        void OnEnable()
        {
            debug = new DebugFromPlugin();
        }

        void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (leftRawImage == null)
                Debug.LogError("Left image is not assigned!");

            if (rightRawImage == null)
                Debug.LogError("Right image is not assigned!");

            if (ip_address == "")
            {
                ip_address = PlayerPrefs.GetString("ip_address");
                Debug.Log("Set IP address to: " + ip_address);
            }
            Texture left = null, right = null;
            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            leftRawImage.texture = left;
            rightRawImage.texture = right;
            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.Connect();
        }

        protected virtual void PipelineStarted()
        {
            Debug.Log("Pipeline strated");
        }

        void OnDisable()
        {
            renderingPlugin.Cleanup();
        }

        void Update()
        {
            renderingPlugin.Render();
        }

    }
}