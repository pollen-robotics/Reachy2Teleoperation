/* Copyright(c) Pollen Robotics, all rights reserved.
 This source code is licensed under the license found in the
 LICENSE file in the root directory of this source tree. */

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using AOT;

namespace GstreamerWebRTC
{
    public class GStreamerDataPlugin
    {


#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void CreateDataPipeline();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void DestroyDataPipeline();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void SetSDPOffer(string sdp_offer);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void SetICECandidate(string candidate, int mline_index);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        public static extern void SendBytesChannelService(byte[] array, int size);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        public static extern void SendBytesChannelCommand(byte[] array, int size);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterICECallback(iceCallback cb);
        delegate void iceCallback(IntPtr candidate, int size_candidate, int mline_index);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterSDPCallback(sdpCallback cb);
        delegate void sdpCallback(IntPtr request, int size);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterChannelServiceOpenCallback(channelServiceOpenCallback cb);
        delegate void channelServiceOpenCallback();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterChannelCommandOpenCallback(channelCommandOpenCallback cb);
        delegate void channelCommandOpenCallback();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterChannelServiceDataCallback(channelServiceDataCallback cb);
        delegate void channelServiceDataCallback(IntPtr data, int size_data);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterChannelStateDataCallback(channelStateDataCallback cb);
        delegate void channelStateDataCallback(IntPtr data, int size_data);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        static extern void RegisterChannelAuditDataCallback(channelAuditDataCallback cb);
        delegate void channelAuditDataCallback(IntPtr data, int size_data);

        private string _signallingServerURL;
        private Signalling _signalling;

        public bool producer = false;
        public string remote_producer_name = "grpc_webrtc_bridge";

        public UnityEvent event_OnPipelineStarted;
        public UnityEvent event_OnPipelineStopped;

        private static UnityEvent<string> event_OnSDPAnswer;

        private static UnityEvent<string, int> event_OnICE;
        public static UnityEvent event_OnChannelServiceOpen;
        public static UnityEvent event_OnChannelCommandOpen;
        public static UnityEvent<byte[]> event_OnChannelServiceData;
        public static UnityEvent<byte[]> event_OnChannelStateData;
        public static UnityEvent<byte[]> event_OnChannelAuditData;

        private bool _autoreconnect = false;

        public GStreamerDataPlugin(string ip_address)
        {
            _autoreconnect = true;
            RegisterICECallback(OnICECallback);
            RegisterSDPCallback(OnSDPCallback);
            RegisterChannelServiceOpenCallback(OnChannelServiceOpenCallback);
            RegisterChannelCommandOpenCallback(OnChannelCommandOpenCallback);
            RegisterChannelServiceDataCallback(OnChannelServiceDataCallback);
            RegisterChannelStateDataCallback(OnChannelStateDataCallback);
            RegisterChannelAuditDataCallback(OnChannelAuditDataCallback);

            _signallingServerURL = "ws://" + ip_address + ":8443";

            _signalling = new Signalling(_signallingServerURL, remote_producer_name);

            _signalling.event_OnRemotePeerId.AddListener(StartPipeline);
            _signalling.event_OnRemotePeerLeft.AddListener(StopPipeline);
            _signalling.event_OnSDPOffer.AddListener(OnSDPOffer);
            _signalling.event_OnICECandidate.AddListener(OnReceivedICE);

            event_OnPipelineStarted = new UnityEvent();
            event_OnPipelineStopped = new UnityEvent();

            event_OnSDPAnswer = new UnityEvent<string>();
            event_OnSDPAnswer.AddListener(SendSDPAnswer);

            event_OnICE = new UnityEvent<string, int>();
            event_OnICE.AddListener(OnICE);

            event_OnChannelServiceOpen = new UnityEvent();
            event_OnChannelCommandOpen = new UnityEvent();
            event_OnChannelServiceData = new UnityEvent<byte[]>();
            event_OnChannelStateData = new UnityEvent<byte[]>();
            event_OnChannelAuditData = new UnityEvent<byte[]>();
        }

        public void Connect()
        {
            _signalling.Connect();
        }

        void StartPipeline(string remote_peer_id)
        {
            Debug.Log("start data pipe " + remote_peer_id);
            CreateDataPipeline();
            event_OnPipelineStarted.Invoke();
        }

        void StopPipeline()
        {
            event_OnPipelineStopped.Invoke();
            if (_autoreconnect)
                Connect();
        }

        void OnSDPOffer(string sdp_offer)
        {
            SetSDPOffer(sdp_offer);
        }

        void OnICE(string candidate, int mline_index)
        {
            _signalling.SendICECandidate(candidate, mline_index);
        }

        void OnReceivedICE(string candidate, int mline_index)
        {
            SetICECandidate(candidate, mline_index);
        }

        void SendSDPAnswer(string sdp_answer)
        {
            _signalling.SendSDP(sdp_answer);
        }

        public void Cleanup()
        {
            _signalling.Close();
            _signalling.RequestStop();
            DestroyDataPipeline();
        }

        [MonoPInvokeCallback(typeof(iceCallback))]
        static void OnICECallback(IntPtr candidate, int size_candidate, int mline_index)
        {
            string candidate_msg = Marshal.PtrToStringAnsi(candidate, size_candidate);
            Debug.Log("ICE Candidate: " + candidate_msg + " " + mline_index);
            event_OnICE.Invoke(candidate_msg, mline_index);
        }

        [MonoPInvokeCallback(typeof(sdpCallback))]
        static void OnSDPCallback(IntPtr request, int size)
        {
            string sdp_answer = Marshal.PtrToStringAnsi(request, size);
            Debug.Log("SDP " + sdp_answer);
            event_OnSDPAnswer.Invoke(sdp_answer);
        }

        [MonoPInvokeCallback(typeof(channelServiceOpenCallback))]
        static void OnChannelServiceOpenCallback()
        {
            event_OnChannelServiceOpen.Invoke();
        }

        [MonoPInvokeCallback(typeof(channelCommandOpenCallback))]
        static void OnChannelCommandOpenCallback()
        {
            event_OnChannelCommandOpen.Invoke();
        }

        [MonoPInvokeCallback(typeof(channelServiceDataCallback))]
        static void OnChannelServiceDataCallback(IntPtr data, int size)
        {
            byte[] data_bytes = new byte[size];
            Marshal.Copy(data, data_bytes, 0, size);
            event_OnChannelServiceData.Invoke(data_bytes);
        }

        [MonoPInvokeCallback(typeof(channelStateDataCallback))]
        static void OnChannelStateDataCallback(IntPtr data, int size)
        {
            byte[] data_bytes = new byte[size];
            Marshal.Copy(data, data_bytes, 0, size);
            event_OnChannelStateData.Invoke(data_bytes);
        }

        [MonoPInvokeCallback(typeof(channelAuditDataCallback))]
        static void OnChannelAuditDataCallback(IntPtr data, int size)
        {
            byte[] data_bytes = new byte[size];
            Marshal.Copy(data, data_bytes, 0, size);
            event_OnChannelAuditData.Invoke(data_bytes);
        }

    }
}
