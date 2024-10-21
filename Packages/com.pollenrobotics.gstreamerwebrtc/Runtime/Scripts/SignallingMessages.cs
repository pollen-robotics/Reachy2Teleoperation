using System;


namespace GstreamerWebRTC
{
    //Classes used for building json messages

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
}