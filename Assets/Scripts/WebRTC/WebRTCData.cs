using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Events;
using Bridge;
using Reachy;

namespace TeleopReachy
{
    public class WebRTCData : WebRTCBase
    {
        private RTCDataChannel _serviceChannel = null;
        private RTCDataChannel _reachyStateChannel = null;
        private RTCDataChannel _reachyAuditChannel = null;
        private RTCDataChannel _reachyCommandChannel = null;

        private Bridge.ConnectionStatus _connectionStatus = null;

        private ReachyState _reachyState = null;
        private ReachyStatus _reachyStatus = null;

        private DataMessageManager dataMessageManager;

        public UnityEvent<bool> event_DataControllerStatusHasChanged;
        private bool isRobotInRoom = false;

        protected override void Start()
        {
            base.Start();
            dataMessageManager = DataMessageManager.Instance;
        }

        protected override void WebRTCCall()
        {
            base.WebRTCCall();

            if (_pc != null)
            {
                _pc.OnDataChannel = channel =>
                {
                    Debug.Log($"Receiving new channel: {channel.Label}");
                    if (channel.Label == "service")
                    {
                        SetupConnection(channel);
                    }
                    else if (channel.Label.StartsWith("reachy_state"))
                    {
                        SetupStateChannel(channel);
                    }
                    else if (channel.Label.StartsWith("reachy_command"))
                    {
                        SetupCommandChannel(channel);
                    }
                    else if (channel.Label.StartsWith("reachy_audit"))
                    {
                        SetupAuditChannel(channel);
                    }
                    else
                    {
                        Debug.LogWarning($"Channel {channel.Label} is unknown");
                    }
                };
            }
        }

        void SetupStateChannel(RTCDataChannel channel)
        {
            _reachyStateChannel = channel;
            _reachyStateChannel.OnMessage = OnDataChannelStateMessage;
        }

        void SetupAuditChannel(RTCDataChannel channel)
        {
            _reachyAuditChannel = channel;
            _reachyAuditChannel.OnMessage = OnDataChannelAuditMessage;
        }

        void OnDataChannelStateMessage(byte[] data)
        {
            _reachyState = ReachyState.Parser.ParseFrom(data);

            dataMessageManager.StreamReachyState(_reachyState);
        }

        void OnDataChannelAuditMessage(byte[] data)
        {
            _reachyStatus = ReachyStatus.Parser.ParseFrom(data);

            dataMessageManager.StreamReachyStatus(_reachyStatus);
        }

        void SetupCommandChannel(RTCDataChannel channel)
        {
            _reachyCommandChannel = channel;
        }

        void SetupConnection(RTCDataChannel channel)
        {
            _serviceChannel = channel;
            _serviceChannel.OnMessage = OnDataChannelServiceMessage;
            var req = new ServiceRequest
            {
                GetReachy = new GetReachy()
            };
            _serviceChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(req));
        }

        void OnDataChannelServiceMessage(byte[] data)
        {
            ServiceResponse response = ServiceResponse.Parser.ParseFrom(data);
            Debug.Log(response);

            if (response.ConnectionStatus != null)
            {
                _connectionStatus = response.ConnectionStatus;
                Debug.Log(_connectionStatus.ToString());

                if (response.ConnectionStatus.Connected)
                {
                    dataMessageManager.GetReachyId(response.ConnectionStatus.Reachy);
                    isRobotInRoom = true;
                    event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
                }

                var req = new ServiceRequest
                {
                    Connect = new Connect
                    {
                        ReachyId = _connectionStatus.Reachy.Id,
                        UpdateFrequency = 50, //FixedUpdate refresh rate is 0.02 sec
                        AuditFrequency = 1,
                    }
                };
                _serviceChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(req));
            }

            if (response.Error != null && !string.IsNullOrEmpty(response.Error.ToString()))
            {
                Debug.LogError($"Received error message: {response.Error.ToString()}");
            }

        }

        new void OnDestroy()
        {
            _reachyCommandChannel = null;
            base.OnDestroy();
        }

        public void SendCommandMessage(AnyCommands commands)
        {
            if (_reachyCommandChannel != null) _reachyCommandChannel.Send(Google.Protobuf.MessageExtensions.ToByteArray(commands));
        }
    }
}
