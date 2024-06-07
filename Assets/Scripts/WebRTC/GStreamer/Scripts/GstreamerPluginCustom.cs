using UnityEngine;
using UnityEngine.Events;
using Bridge;

namespace GstreamerWebRTC
{
    public class GStreamerPluginCustom : GStreamerPlugin
    {
        public Renderer screen;
        public UnityEvent<bool> event_OnVideoRoomStatusHasChanged;
        public UnityEvent<bool> event_OnAudioReceiverRoomStatusHasChanged;
        public UnityEvent<bool> event_AudioSenderStatusHasChanged;
        public UnityEvent<bool> event_DataControllerStatusHasChanged;

        private Texture left = null;
        private Texture right = null;

        override protected void Init()
        {
            if (screen == null)
                Debug.LogError("Screen is not assigned!");

            if (ip_address == "")
            {
                ip_address = PlayerPrefs.GetString("robot_ip");
                Debug.Log("Set IP address to: " + ip_address);
            }

            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            screen.material.SetTexture("_LeftTex", left);
            screen.material.SetTexture("_RightTex", right);

            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.Connect();

            dataPlugin = new GStreamerDataPlugin(ip_address);
            dataPlugin.event_OnPipelineStarted.AddListener(PipelineDataStarted);
            GStreamerDataPlugin.event_OnChannelServiceOpen.AddListener(OnChannelServiceOpen);
            dataPlugin.Connect();
        }

        override protected void PipelineStarted()
        {
            Debug.Log("Pipeline started Custom");
            event_OnVideoRoomStatusHasChanged.Invoke(true);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(true);
            event_AudioSenderStatusHasChanged.Invoke(true);
        }

        override protected void PipelineDataStarted()
        {
            Debug.Log("Pipeline data started Custom");
            event_DataControllerStatusHasChanged.Invoke(true);
        }

        override protected void OnDisable()
        {
            event_OnVideoRoomStatusHasChanged.Invoke(false);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(false);
            event_AudioSenderStatusHasChanged.Invoke(false);
            event_DataControllerStatusHasChanged.Invoke(false);
            base.OnDisable();
        }

        /*void Update()
        {
            renderingPlugin.Render();
        }*/

        public Texture GetLeftTexture()
        {
            return left;
        }

        public Texture GetRightTexture()
        {
            return right;
        }

        protected override void OnChannelServiceOpen()
        {
            var req = new ServiceRequest
            {
                GetReachy = new GetReachy()
            };
            byte[] bytes = Google.Protobuf.MessageExtensions.ToByteArray(req);
            GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
        }

        public void SendCommandMessage(AnyCommands commands)
        {
            Debug.Log("Todo " + commands);
            //if (_reachyCommandChannel != null) _reachyCommandChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(commands));
        }

    }
}