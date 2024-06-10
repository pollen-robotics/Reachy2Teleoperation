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

        protected GStreamerDataPlugin dataPlugin = null;
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
                ip_address = PlayerPrefs.GetString("robot_ip");
                Debug.Log("Set IP address to: " + ip_address);
            }
            Texture left = null, right = null;
            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            leftRawImage.texture = left;
            rightRawImage.texture = right;
            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.Connect();
            dataPlugin = new GStreamerDataPlugin(ip_address);
            dataPlugin.event_OnPipelineStarted.AddListener(PipelineDataStarted);
            GStreamerDataPlugin.event_OnChannelServiceOpen.AddListener(OnChannelServiceOpen);
            GStreamerDataPlugin.event_OnChannelServiceData.AddListener(OnChannelServiceData);
            dataPlugin.Connect();
        }

        protected virtual void PipelineStarted()
        {
            Debug.Log("Pipeline started");
        }

        protected virtual void PipelineDataStarted()
        {
            Debug.Log("Pipeline data started");
        }

        protected virtual void OnDisable()
        {
            renderingPlugin.Cleanup();
            dataPlugin.Cleanup();
            dataPlugin = null;
        }

        void Update()
        {
            renderingPlugin.Render();
        }

        //Data channels
        protected virtual void OnChannelServiceOpen()
        {
            byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x20, 0x20, 0x20, 0x20 }; ;
            GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
        }


        protected virtual void OnChannelServiceData(byte[] data)
        {
            byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x20, 0x20, 0x20, 0x20 }; ;
            GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
        }

        protected void SendCommandToChannel(byte[] commands)
        {
            GStreamerDataPlugin.SendBytesChannelCommand(commands, commands.Length);
        }
    }
}