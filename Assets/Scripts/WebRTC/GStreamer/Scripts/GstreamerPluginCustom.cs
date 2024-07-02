using UnityEngine;
using UnityEngine.Events;
using Bridge;
using TeleopReachy;
using Reachy;

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

        private DataMessageManager dataMessageManager;

        override protected void Init()
        {
            if (screen == null)
                Debug.LogError("Screen is not assigned!");

            if (ip_address == "")
            {
                ip_address = PlayerPrefs.GetString("robot_ip");
                Debug.Log("Set IP address to: " + ip_address);
            }

            dataMessageManager = DataMessageManager.Instance;

            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            screen.material.SetTexture("_LeftTex", left);
            screen.material.SetTexture("_RightTex", right);

            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);

            dataPlugin = new GStreamerDataPlugin(ip_address);
            dataPlugin.event_OnPipelineStarted.AddListener(PipelineDataStarted);
            GStreamerDataPlugin.event_OnChannelServiceOpen.AddListener(OnChannelServiceOpen);
            GStreamerDataPlugin.event_OnChannelServiceData.AddListener(OnChannelServiceData);
            GStreamerDataPlugin.event_OnChannelStateData.AddListener(OnDataChannelStateMessage);

            renderingPlugin.Connect();
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

        protected override void OnChannelServiceData(byte[] data)
        {
            ServiceResponse response = ServiceResponse.Parser.ParseFrom(data);
            Debug.Log(response);

            if (response.ConnectionStatus != null)
            {
                Bridge.ConnectionStatus _connectionStatus = response.ConnectionStatus;
                Debug.Log(_connectionStatus.ToString());

                if (response.ConnectionStatus.Connected)
                {
                    Debug.Log("GstreamerPlugin: config received" + response.ConnectionStatus.Reachy);
                    dataMessageManager.GetReachyId(response.ConnectionStatus.Reachy);
                    event_DataControllerStatusHasChanged.Invoke(true);
                }

                var req = new ServiceRequest
                {
                    Connect = new Connect
                    {
                        ReachyId = _connectionStatus.Reachy.Id,
                        UpdateFrequency = 50 //FixedUpdate refresh rate is 0.02 sec
                    }
                };
                byte[] bytes = Google.Protobuf.MessageExtensions.ToByteArray(req);
                GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
            }

            if (response.Error != null && !string.IsNullOrEmpty(response.Error.ToString()))
            {
                Debug.LogError($"Received error message: {response.Error.ToString()}");
            }
        }

        public void SendCommandMessage(AnyCommands commands)
        {
            //Debug.Log("send command");
            //if (_reachyCommandChannel != null) _reachyCommandChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(commands));
            SendCommandToChannel(Google.Protobuf.MessageExtensions.ToByteArray(commands));
        }

        void OnDataChannelStateMessage(byte[] data)
        {
            ReachyState _reachyState = ReachyState.Parser.ParseFrom(data);
            Debug.Log("received message " + _reachyState);
            dataMessageManager.StreamReachyState(_reachyState);
        }
    }
}