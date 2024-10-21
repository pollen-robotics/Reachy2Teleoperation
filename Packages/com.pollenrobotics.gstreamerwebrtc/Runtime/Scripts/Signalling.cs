using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Net.WebSockets;
using System.Text;

namespace GstreamerWebRTC
{
    public class Signalling : BaseSignalling
    {
        private string _session_id;
        public UnityEvent<string> event_OnSDPOffer;
        public UnityEvent<string, int> event_OnICECandidate;
        public UnityEvent<string> event_OnSessionID;

        public Signalling(string url, string remote_producer_name = "") : base(url, remote_producer_name)
        {
            event_OnRemotePeerId = new UnityEvent<string>();
            event_OnSDPOffer = new UnityEvent<string>();
            event_OnICECandidate = new UnityEvent<string, int>();
        }

        protected override void StartSession(string id)
        {
            Debug.Log("Start session data");
            SendStartSession(id);
        }

        protected override void ProcessMessage(string message)
        {
            base.ProcessMessage(message);

            if (message != null)
            {
                var msg = JsonUtility.FromJson<SignalingMessage>(message);

                /*if (msg.type == MessageType.PeerStatusChanged.ToString())
                {
                    Debug.Log(msg.ToString());
                    if (msg.meta?.name == _remote_producer_name && msg.roles.Contains(MessageRole.Producer.ToString()))
                    {
                        Debug.Log("Start Session");
                        SendStartSession(msg.peerId);
                    }
                }*/
                /*if (sessionStatus == SessionStatus.Ended && msg.type == MessageType.List.ToString())
                {
                    //Debug.Log("processing list..");
                    foreach (var p in msg.producers)
                    {
                        if (p.meta.name == _remote_producer_name)
                        {
                            //tasks_running = false;
                            Debug.Log("Start session data");
                            SendStartSession(p.id);
                            //event_OnRemotePeerId.Invoke(p.id);
                            //sessionStatus = SessionStatus.Started;
                            //break;
                        }
                    }
                }*/

                if (msg.type == MessageType.StartSession.ToString())
                {
                    _session_id = msg.sessionId;
                    //event_OnConnectionStatus.Invoke(ConnectionStatus.Ready);
                }
                else if (msg.type == MessageType.SessionStarted.ToString())
                {
                    _session_id = msg.sessionId;
                    //Debug.Log("session id: " + _session_id);
                    Debug.Log("Session started. peer id:" + msg.peerId + " session id:" + msg.sessionId);
                    //event_OnConnectionStatus.Invoke(ConnectionStatus.Ready);
                    sessionStatus = SessionStatus.Started;
                }
                else if (msg.type == MessageType.SessionEnded.ToString())
                {
                    _session_id = null;
                    Debug.Log("session ended: " + msg.sessionId);

                    //event_OnConnectionStatus.Invoke(ConnectionStatus.Waiting);
                    sessionStatus = SessionStatus.Ended;
                }
                else if (msg.type == MessageType.Peer.ToString())
                {
                    if (msg.ice.IsValid())
                    {
                        Debug.Log("received ice candidate " + msg.ice.candidate + " " + msg.ice.sdpMLineIndex);
                        event_OnICECandidate.Invoke(msg.ice.candidate, msg.ice.sdpMLineIndex);
                    }
                    else if (msg.sdp.IsValid())
                    {
                        if (msg.sdp.type == "offer")
                        {
                            Debug.Log("received offer " + msg.sdp.sdp);
                            event_OnSDPOffer.Invoke(msg.sdp.sdp);
                        }
                        else if (msg.sdp.type == "answer")
                        {
                            Debug.LogWarning("received answer");
                        }
                    }
                }
                else
                {
                    Debug.Log("Message not processed " + msg);
                }
            }
        }

        public async void SendSDP(string sdp_msg, string type = "answer")
        {
            string msg = JsonUtility.ToJson(new SDPMessage
            {
                type = MessageType.Peer.ToString(),
                sessionId = _session_id,
                sdp = new SdpMessage
                {
                    type = type,
                    sdp = sdp_msg,
                },
            });
            //await webSocket.SendText(msg);
            Debug.Log("Send SDP answer " + msg);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
        }

        public async void SendICECandidate(string candidate, int mline_index)
        {
            string msg = JsonUtility.ToJson(new ICEMessage
            {
                type = MessageType.Peer.ToString(),
                sessionId = _session_id,
                ice = new ICECandidateMessage(candidate, mline_index),
            });
            // await webSocket.SendText(msg);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
        }

        /*private async void SendMessage(MessageType type, MessageRole role)
        {
            Debug.Log("SetPeerStatus");
            string msg = JsonUtility.ToJson(new SignalingMessage
            {
                type = type.ToString(),
                roles = new string[] { role.ToString() },
            });
            //await webSocket.SendText(msg);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
        }*/

        private async void SendStartSession(string peer_id)
        {
            Debug.Log("StartSessionMessage");
            string msg = JsonUtility.ToJson(new StartSessionMessage
            {
                type = MessageType.StartSession.ToString(),
                roles = new string[] { MessageRole.Consumer.ToString() },
                peerId = peer_id,
            });
            //await webSocket.SendText(msg);
            Debug.Log("send start session " + msg);
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
            sessionStatus = SessionStatus.Asked;
        }
    }


}