using System;
using UnityEngine;
using System.Threading.Tasks;
using NativeWebSocket;
using System.Linq;
using UnityEngine.Events;
using Codice.Client.BaseCommands;
using PlasticPipe.PlasticProtocol.Messages;

namespace GstreamerWebRTC
{

    public enum ConnectionStatus
    {
        Waiting,
        Ready,
        Kicked,
    }

    public enum SessionStatus
    {
        Asked,
        Started,
        Ended,
    }

    public class MessageType
    {
        private MessageType(string value) { Value = value; }

        public string Value { get; private set; }

        public static MessageType Welcome { get { return new MessageType("welcome"); } }
        public static MessageType SetPeerStatus { get { return new MessageType("setPeerStatus"); } }
        public static MessageType PeerStatusChanged { get { return new MessageType("peerStatusChanged"); } }
        public static MessageType StartSession { get { return new MessageType("startSession"); } }
        public static MessageType SessionStarted { get { return new MessageType("sessionStarted"); } }
        public static MessageType SessionEnded { get { return new MessageType("endSession"); } }
        public static MessageType Peer { get { return new MessageType("peer"); } }
        public static MessageType List { get { return new MessageType("list"); } }
        public override string ToString()
        {
            return Value;
        }
    }

    public class MessageRole
    {
        private MessageRole(string value) { Value = value; }

        public string Value { get; private set; }

        public static MessageRole Listener { get { return new MessageRole("listener"); } }
        public static MessageRole Producer { get { return new MessageRole("producer"); } }
        public static MessageRole Consumer { get { return new MessageRole("consumer"); } }
        public override string ToString()
        {
            return Value;
        }
    }

    public class Signalling
    {
        private WebSocket webSocket;
        private string _remote_producer_name;
        private string _peer_id;
        private string _session_id;
        public UnityEvent<string> event_OnRemotePeerId;
        public UnityEvent<string> event_OnSDPOffer;
        public UnityEvent<string, int> event_OnICECandidate;
        public UnityEvent<string> event_OnSessionID;

        private SessionStatus sessionStatus;
        private Task task_askForList;
        private Task task_updateMessages;
        private bool tasks_running = false;

        public Signalling(string url, bool producer, string remote_producer_name = "")
        {
            if (!producer && remote_producer_name == "")
                Debug.LogError("Remote producer name should be set for a consumer role");

            _remote_producer_name = remote_producer_name;
            sessionStatus = SessionStatus.Ended;

            event_OnRemotePeerId = new UnityEvent<string>();
            event_OnSDPOffer = new UnityEvent<string>();
            event_OnICECandidate = new UnityEvent<string, int>();

            webSocket = new WebSocket(url);

            webSocket.OnOpen += () =>
            {
                if (producer)
                    SendMessage(MessageType.SetPeerStatus, MessageRole.Producer);
                else
                {
                    SendMessage(MessageType.SetPeerStatus, MessageRole.Listener);
                }
            };
            webSocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.LogWarning(message);
                if (message != null)
                {
                    var msg = JsonUtility.FromJson<SignalingMessage>(message);

                    if (msg.type == MessageType.Welcome.ToString())
                    {
                        _peer_id = msg.peerId;
                        //event_OnConnectionStatus.Invoke(ConnectionStatus.Waiting);
                        Debug.Log("peer id : " + _peer_id);
                        if (!producer)
                        {
                            task_askForList = new Task(() => AskList());
                            task_askForList.Start();
                        }
                    }
                    else if (msg.type == MessageType.PeerStatusChanged.ToString())
                    {
                        Debug.Log(msg.ToString());
                        if (msg.meta?.name == _remote_producer_name && msg.roles.Contains(MessageRole.Producer.ToString()))
                        {
                            //event_OnRemotePeerId.Invoke(msg.peerId);
                            Debug.Log("Start Session");
                            SendStartSession(msg.peerId);
                        }
                    }
                    else if (sessionStatus == SessionStatus.Ended && msg.type == MessageType.List.ToString())
                    {
                        Debug.Log("processing list..");
                        foreach (var p in msg.producers)
                        {
                            if (p.meta.name == _remote_producer_name)
                            {
                                SendStartSession(p.id);
                                event_OnRemotePeerId.Invoke(p.id);
                                sessionStatus = SessionStatus.Started;
                                break;
                            }
                        }
                    }
                    else if (msg.type == MessageType.StartSession.ToString())
                    {
                        Debug.Log("1 " + msg.sessionId);
                        _session_id = msg.sessionId;
                        //event_OnConnectionStatus.Invoke(ConnectionStatus.Ready);
                    }
                    else if (msg.type == MessageType.SessionStarted.ToString())
                    {
                        Debug.Log("2 " + msg.sessionId);
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
                            /*RTCIceCandidate candidate = new RTCIceCandidate(
                                new RTCIceCandidateInit
                                {
                                    candidate = msg.ice.candidate,
                                    sdpMid = msg.ice.sdpMid,
                                    sdpMLineIndex = msg.ice.sdpMLineIndex,
                                }
                            );
                            event_OnICECandidate.Invoke(candidate);*/
                        }
                        else if (msg.sdp.IsValid())
                        {
                            if (msg.sdp.type == "offer")
                            {
                                Debug.Log("received offer " + msg.sdp.sdp);
                                event_OnSDPOffer.Invoke(msg.sdp.sdp);
                                /*var offer = new RTCSessionDescription
                                {
                                    type = RTCSdpType.Offer,
                                    sdp = msg.sdp.sdp,
                                };
                                event_OnOffer.Invoke(offer);*/
                            }
                            else if (msg.sdp.type == "answer")
                            {
                                Debug.LogWarning("received answer");
                                /*var answser = new RTCSessionDescription
                                {
                                    type = RTCSdpType.Answer,
                                    sdp = msg.sdp.sdp,
                                };
                                event_OnAnswer.Invoke(answser);*/
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Unrecognized message !" + sessionStatus);
                    }
                }
            };
            webSocket.OnError += (e) =>
                {
                    Debug.LogError($"WS error {e}");
                };
            webSocket.OnClose += (e) =>
            {
                Debug.Log($"WS closed");
            };

#if !UNITY_WEBGL || UNITY_EDITOR
            task_updateMessages = new Task(() => UpdateMessages());
            task_updateMessages.Start();
#endif
        }
        public void Connect()
        {
            webSocket.Connect();
        }

        public async void UpdateMessages()
        {
            tasks_running = true;
            while (tasks_running)
            {
                webSocket.DispatchMessageQueue();
                await Task.Delay(200);
            }
        }
        public void Close()
        {
            tasks_running = false;
            webSocket.Close();
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
            await webSocket.SendText(msg);
        }

        public async void SendICECandidate(string candidate, int mline_index)
        {
            string msg = JsonUtility.ToJson(new ICEMessage
            {
                type = MessageType.Peer.ToString(),
                sessionId = _session_id,
                ice = new ICECandidateMessage(candidate, mline_index),
            });
            await webSocket.SendText(msg);
        }

        private async void AskList()
        {
            tasks_running = true;
            while (tasks_running)
            {
                if (sessionStatus == SessionStatus.Ended)
                {
                    Debug.Log("Ask for list");
                    string msg = JsonUtility.ToJson(new SignalingMessage
                    {
                        type = MessageType.List.ToString(),
                    });
                    await webSocket.SendText(msg);
                }
                await Task.Delay(1000);
            }
        }

        private async void SendMessage(MessageType type, MessageRole role)
        {
            Debug.Log("SetPeerStatus");
            string msg = JsonUtility.ToJson(new SignalingMessage
            {
                type = type.ToString(),
                roles = new string[] { role.ToString() },
            });
            await webSocket.SendText(msg);
        }

        private async void SendStartSession(string peer_id)
        {
            Debug.Log("StartSessionMessage");
            string msg = JsonUtility.ToJson(new StartSessionMessage
            {
                type = MessageType.StartSession.ToString(),
                roles = new string[] { MessageRole.Consumer.ToString() },
                peerId = peer_id,
            });
            await webSocket.SendText(msg);
            sessionStatus = SessionStatus.Asked;
        }
    }

    //Class used for building json messages

    [System.Serializable]
    class Meta
    {
        public string name = "UnityClient";
    }

    [System.Serializable]
    class Producer
    {
        public string id;
        public Meta meta;
    }

    [System.Serializable]
    class SdpMessage
    {
        public string type;
        public string sdp;

        public bool IsValid()
        {
            return sdp != default(string);
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", type, sdp);
        }
    }

    [System.Serializable]
    class ICECandidateMessage
    {
        public string candidate;
        //public string sdpMid;
        public int sdpMLineIndex;
        // public string usernameFragment;

        /*public ICECandidateMessage(RTCIceCandidate candidate)
        {
            this.candidate = candidate.Candidate;
            this.sdpMid = candidate.SdpMid;
            this.sdpMLineIndex = candidate.SdpMLineIndex ?? 0;
            this.usernameFragment = candidate.UserNameFragment;
        }*/

        public ICECandidateMessage(string candidate, int mline_index)
        {
            this.candidate = candidate;
            sdpMLineIndex = mline_index;
        }

        public bool IsValid()
        {
            //ignore null ice candidate. Ongoing patch with Unity
            return candidate != default(string) && candidate != "";
        }
    }

    [System.Serializable]
    class SignalingMessage
    {
        public string type;
        public string peerId;
        public string sessionId;
        public Meta meta;
        public string[] roles;
        public ICECandidateMessage ice;
        public SdpMessage sdp;
        public Producer[] producers;

        public override string ToString()
        {
            return String.Format("{0} : {1} : {2} : {3}", type, peerId, meta, roles);
        }

    }

    [System.Serializable]
    class SDPMessage
    {
        public string type;
        public string sessionId;
        public SdpMessage sdp;
    }

    [System.Serializable]
    class ICEMessage
    {
        public string type;
        public string sessionId;
        public ICECandidateMessage ice;
    }

    [System.Serializable]
    class StartSessionMessage
    {
        public string type;
        public string peerId;
        public string[] roles;
    }
}