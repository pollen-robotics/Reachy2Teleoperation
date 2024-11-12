/* Copyright(c) Pollen Robotics, all rights reserved.
 This source code is licensed under the license found in the
 LICENSE file in the root directory of this source tree. */

using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using System.Net.WebSockets;
using System.Text;

namespace GstreamerWebRTC
{

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

    public class BaseSignalling
    {
        protected ClientWebSocket webSocket;
        protected string _remote_producer_name;
        protected string _peer_id;
        //private string _session_id;
        public UnityEvent<string> event_OnRemotePeerId;
        public UnityEvent event_OnRemotePeerLeft;

        protected SessionStatus sessionStatus;
        private Task task_askForList;
        private Task task_updateMessages;
        private Task task_checkconnection;
        private bool tasks_running = false;
        private Uri _uri;
        protected CancellationTokenSource _cts;

        private const int MAX_CONNECTION_ATTEMPTS = 100;

        private bool request_stop = false;

        public BaseSignalling(string url, string remote_producer_name = "")
        {
            if (remote_producer_name == "")
                Debug.LogError("Remote producer name should be set for a consumer role");

            _uri = new Uri(url);

            _remote_producer_name = remote_producer_name;
            sessionStatus = SessionStatus.Ended;

            event_OnRemotePeerId = new UnityEvent<string>();
            event_OnRemotePeerLeft = new UnityEvent();

            request_stop = false;
        }


        ~BaseSignalling()
        {
            Debug.Log("Finish");
            request_stop = true;
            Close();
            webSocket?.Dispose();
        }

        public async void Connect()
        {
            for (int i = 0; i < MAX_CONNECTION_ATTEMPTS; i++)
            {
                try
                {
                    Close();
                    webSocket = new ClientWebSocket();
                    _cts = new CancellationTokenSource();
                    Debug.Log("trying to connect...");
                    await webSocket.ConnectAsync(_uri, _cts.Token);

                    if (webSocket.State == WebSocketState.Open)
                    {
                        Debug.Log("Connected to WebSocket server.");
                        tasks_running = true;
                        task_updateMessages = new Task(() => UpdateMessages());
                        task_updateMessages.Start();
                        task_checkconnection = new Task(() => CheckConnectionStatus());
                        task_checkconnection.Start();
                        break;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to connect to WebSocket server. Attempt " + i);
                    }
                }
                catch (WebSocketException ex)
                {
                    Debug.LogWarning("Failed to connect to WebSocket server. Attempt " + i + ". Exception " + ex);
                    if (request_stop)
                        break;
                }

            }
            if (webSocket.State != WebSocketState.Open && !request_stop)
                Debug.LogError("Failed to connect to WebSocket server.");
        }

        private async void CheckConnectionStatus()
        {
            while (tasks_running)
            {
                //Debug.Log("Check connection. Status :" + webSocket.State);
                if (webSocket.State != WebSocketState.Open)
                {
                    Debug.LogError("Connnection lost. Status : " + webSocket.State);
                    break;
                }
                await Task.Delay(1000);
            }
            if (!request_stop)
                event_OnRemotePeerLeft.Invoke();
            Close();
            Debug.Log("Quit check connection");
        }

        public async void UpdateMessages()
        {
            //tasks_running = true;
            while (tasks_running)
            {
                // webSocket.DispatchMessageQueue();
                var responseBuffer = new byte[1024];
                //Debug.Log("wait receiv " + webSocket.State);
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(responseBuffer), _cts.Token);
                var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, result.Count);
                //Debug.Log("message " + responseMessage);
                ProcessMessage(responseMessage);
                //Debug.Log("here");
                //return responseMessage;
                //await Task.Delay(200);
            }
            Debug.Log("Quit update message");
        }

        protected virtual void StartSession(string id)
        {

        }

        protected virtual void ProcessMessage(string message)
        {
            if (message != null)
            {
                var msg = JsonUtility.FromJson<SignalingMessage>(message);

                if (msg.type == MessageType.Welcome.ToString())
                {
                    _peer_id = msg.peerId;
                    Debug.Log("peer id : " + _peer_id);
                    task_askForList = new Task(() => AskList());
                    task_askForList.Start();
                }
                else if (sessionStatus == SessionStatus.Ended && msg.type == MessageType.List.ToString())
                {
                    Debug.Log("processing list..");
                    foreach (var p in msg.producers)
                    {
                        if (p.meta.name == _remote_producer_name)
                        {
                            event_OnRemotePeerId.Invoke(p.id);
                            StartSession(p.id);
                            sessionStatus = SessionStatus.Started;
                            break;
                        }
                    }
                }
                else if (sessionStatus == SessionStatus.Started && msg.type == MessageType.List.ToString())
                {
                    Debug.Log("Checking presence of producer " + _remote_producer_name);
                    bool producer_present = false;
                    foreach (var p in msg.producers)
                    {
                        if (p.meta.name == _remote_producer_name)
                        {
                            producer_present = true;
                            break;
                        }
                    }
                    if (!producer_present)
                    {
                        Debug.LogWarning("Producer has " + _remote_producer_name + " left");
                        sessionStatus = SessionStatus.Ended;
                        if (!request_stop)
                            event_OnRemotePeerLeft.Invoke();
                        Close();
                    }
                }
                else if (msg.type == MessageType.SessionEnded.ToString())
                {
                    Debug.Log("Session ended");
                    sessionStatus = SessionStatus.Ended;
                    if (!request_stop)
                        event_OnRemotePeerLeft.Invoke();
                    Close();
                }
                else
                {
                    Debug.LogWarning("Message not processed " + msg);
                }
            }
        }

        public void Close()
        {
            Debug.Log("Close signalling");
            tasks_running = false;
            sessionStatus = SessionStatus.Ended;
            //webSocket.Close();
            //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token);
            _cts?.Cancel();
            task_askForList?.Wait();
            task_updateMessages?.Wait();
            task_checkconnection?.Wait();
        }

        public void RequestStop()
        {
            request_stop = true;
        }

        private async void AskList()
        {
            //tasks_running = true;
            while (tasks_running)
            {
                //if (sessionStatus == SessionStatus.Ended)
                //{
                Debug.Log("Ask for list");
                string msg = JsonUtility.ToJson(new SignalingMessage
                {
                    type = MessageType.List.ToString(),
                });
                // await webSocket.SendText(msg);
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
                //}
                await Task.Delay(1000);
            }
            Debug.Log("Quit ask for list");
        }
    }
}
