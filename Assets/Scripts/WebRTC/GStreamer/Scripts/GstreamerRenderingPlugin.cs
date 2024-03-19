using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Events;

namespace GstreamerWebRTC
{
    public class GstreamerUnityGStreamerPlugin : MonoBehaviour
    {

#if PLATFORM_SWITCH && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterPlugin();
#endif

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void CreatePipeline(string uri, string remote_peer_id);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void DestroyPipeline();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void CreateDevice();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern System.IntPtr GetTexturePtr(bool left);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void CreateTexture(uint width, uint height, bool left);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void ReleaseTexture(System.IntPtr texture);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern IntPtr GetRenderEventFunc();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern System.IntPtr GetTextureUpdateCallback();

        private IntPtr leftTextureNativePtr;

        public Renderer screen;

        private IntPtr rightTextureNativePtr;

        public UnityEvent<Texture> event_OnVideoTextureReceived;
        public UnityEvent<bool> event_OnVideoRoomStatusHasChanged;
        public UnityEvent<bool> event_OnAudioReceiverRoomStatusHasChanged;

        private string _signallingServerURL;
        private Signalling _signalling;

        public bool producer = false;
        public string remote_producer_name = "robot";

        const uint width = 960;
        const uint height = 720;

        void Start()
        {
#if PLATFORM_SWITCH && !UNITY_EDITOR
        RegisterPlugin();
#endif
            string ip_address = PlayerPrefs.GetString("robot_ip");
            //string ip_address = "10.0.1.36";
            // string ip_address="0.0.0.0";
            //string ip_address = "192.168.1.108";
            _signallingServerURL = "ws://" + ip_address + ":8443";

            _signalling = new Signalling(_signallingServerURL, producer, remote_producer_name);

            _signalling.event_OnRemotePeerId.AddListener(StartPipeline);

            CreateDevice();
            CreateRenderTexture(true, ref leftTextureNativePtr, "_LeftTex");
            CreateRenderTexture(false, ref rightTextureNativePtr, "_RightTex");

            _signalling.Connect();
        }

        void CreateRenderTexture(bool left, ref IntPtr textureNativePtr, string texturename)
        {
            CreateTexture(width, height, left);
            textureNativePtr = GetTexturePtr(left);

            if (textureNativePtr != IntPtr.Zero)
            {
                var texture = Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.RGBA32, mipChain: false, linear: true, textureNativePtr);
                screen.material.SetTexture(texturename, texture);
                event_OnVideoTextureReceived.Invoke(texture);
            }
            else
            {
                Debug.LogError("Texture is null");
            }

        }

        void StartPipeline(string remote_peer_id)
        {
            Debug.Log("start pipe " + remote_peer_id);
            CreatePipeline(_signallingServerURL, remote_peer_id);
            event_OnVideoRoomStatusHasChanged.Invoke(true);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(true);
        }

        void OnDisable()
        {
            _signalling.Close();
            DestroyPipeline();
            if (leftTextureNativePtr != IntPtr.Zero)
            {
                ReleaseTexture(leftTextureNativePtr);
                leftTextureNativePtr = IntPtr.Zero;
            }
            if (rightTextureNativePtr != IntPtr.Zero)
            {
                ReleaseTexture(rightTextureNativePtr);
                rightTextureNativePtr = IntPtr.Zero;
            }
        }

        void Update()
        {
            CommandBuffer _command = new CommandBuffer();
            _command.IssuePluginEvent(GetRenderEventFunc(), 1);
            Graphics.ExecuteCommandBuffer(_command);
        }

    }
}