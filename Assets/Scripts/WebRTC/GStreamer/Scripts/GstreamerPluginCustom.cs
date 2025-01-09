using UnityEngine;
using UnityEngine.Events;
using Bridge;
using TeleopReachy;
using Reachy;

namespace GstreamerWebRTC
{
    public class GStreamerPluginCustom : GStreamerPlugin
    {
        // public Renderer screen;
        public UnityEvent<bool> event_OnVideoRoomStatusHasChanged;
        public UnityEvent<bool> event_OnAudioReceiverRoomStatusHasChanged;
        public UnityEvent<bool> event_AudioSenderStatusHasChanged;
        public UnityEvent<bool> event_DataControllerStatusHasChanged;
        public UnityEvent<Texture> event_LeftVideoTextureReady;
        public UnityEvent<Texture> event_RightVideoTextureReady;

        private Texture left = null;
        private Texture right = null;

        private DataMessageManager dataMessageManager;

        override protected void InitAV()
        {
            // if (screen == null)
            //     Debug.LogError("Screen is not assigned!");

            dataMessageManager = DataMessageManager.Instance;

            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            event_LeftVideoTextureReady.Invoke(left);
            event_RightVideoTextureReady.Invoke(right);
            // screen.material.SetTexture("_LeftTex", left);
            // screen.material.SetTexture("_RightTex", right);

            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.event_OnPipelineStopped.AddListener(PipelineStopped);
            GStreamerRenderingPlugin.event_OnFrameDrawn.AddListener(FrameRendered);

            renderingPlugin.Connect();
        }

        override protected void InitData()
        {
            dataPlugin = new GStreamerDataPlugin(ip_address);
            dataPlugin.event_OnPipelineStarted.AddListener(PipelineDataStarted);
            dataPlugin.event_OnPipelineStopped.AddListener(PipelineDataStopped);
            GStreamerDataPlugin.event_OnChannelServiceOpen.AddListener(OnChannelServiceOpen);
            GStreamerDataPlugin.event_OnChannelCommandOpen.AddListener(OnChannelCommandOpen);
            GStreamerDataPlugin.event_OnChannelServiceData.AddListener(OnChannelServiceData);
            GStreamerDataPlugin.event_OnChannelStateData.AddListener(OnDataChannelStateMessage);
            GStreamerDataPlugin.event_OnChannelAuditData.AddListener(OnDataChannelAuditMessage);
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

        override protected void PipelineStopped()
        {
            Debug.Log("Pipeline stopped");
            event_OnVideoRoomStatusHasChanged.Invoke(false);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(false);
            event_AudioSenderStatusHasChanged.Invoke(false);
        }

        override protected void PipelineDataStopped()
        {
            Debug.Log("Pipeline data stopped");
            event_DataControllerStatusHasChanged.Invoke(false);
        }

        override protected void OnDisable()
        {
            event_OnVideoRoomStatusHasChanged.Invoke(false);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(false);
            event_AudioSenderStatusHasChanged.Invoke(false);
            event_DataControllerStatusHasChanged.Invoke(false);
            base.OnDisable();
        }

        public bool IsFrameRendered()
        {
            return frameRendered;
        }

        public void ResetFrameRendered()
        {
            frameRendered = false;
        }

        public Texture GetLeftTexture()
        {
            return left;
        }

        public Texture GetRightTexture()
        {
            return right;
        }

        protected override void OnChannelCommandOpen()
        {
            Debug.Log("Pipeline data started Custom");
            event_DataControllerStatusHasChanged.Invoke(true);
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
                    dataMessageManager.GetReachyId(response.ConnectionStatus.Reachy);
                    event_DataControllerStatusHasChanged.Invoke(true);
                }

                var req = new ServiceRequest
                {
                    Connect = new Connect
                    {
                        ReachyId = _connectionStatus.Reachy.Id,
                        UpdateFrequency = 60,
                        AuditFrequency = 1,
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
            SendCommandToChannel(Google.Protobuf.MessageExtensions.ToByteArray(commands));
        }

        void OnDataChannelStateMessage(byte[] data)
        {
            ReachyState _reachyState = ReachyState.Parser.ParseFrom(data);
            dataMessageManager.StreamReachyState(_reachyState);
        }

        void OnDataChannelAuditMessage(byte[] data)
        {
            ReachyStatus _reachyState = ReachyStatus.Parser.ParseFrom(data);
            dataMessageManager.StreamReachyStatus(_reachyState);
        }
    }
}
