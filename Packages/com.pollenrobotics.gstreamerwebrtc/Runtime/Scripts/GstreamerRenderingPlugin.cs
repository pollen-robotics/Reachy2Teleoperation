using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GstreamerWebRTC
{
    public class GStreamerRenderingPlugin
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
        private static extern void CreateDevice();

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void DestroyPipeline();
        /*
        #if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
            [DllImport("__Internal")]
        #else
                [DllImport("UnityGStreamerPlugin")]
        #endif
                private static extern IntPtr GetTexturePtr(bool left);
                */

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern IntPtr CreateTexture(uint width, uint height, bool left);

#if (PLATFORM_IOS || PLATFORM_TVOS || PLATFORM_BRATWURST || PLATFORM_SWITCH) && !UNITY_EDITOR
    [DllImport("__Internal")]
#else
        [DllImport("UnityGStreamerPlugin")]
#endif
        private static extern void ReleaseTexture(IntPtr texture);

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
        private static extern IntPtr GetTextureUpdateCallback();

        private IntPtr leftTextureNativePtr;

        private IntPtr rightTextureNativePtr;

        private string _signallingServerURL;
        private BaseSignalling _signalling;

        public bool producer = false;
        public string remote_producer_name = "robot";

        const uint width = 960;
        const uint height = 720;

        public UnityEvent event_OnPipelineStarted;

        CommandBuffer _command = null;

        private bool _started = false;
        private bool _autoreconnect = false;

        public GStreamerRenderingPlugin(string ip_address, ref Texture leftTexture, ref Texture rightTexture)
        {
            _started = false;
            _autoreconnect = true;
            _signallingServerURL = "ws://" + ip_address + ":8443";

            _signalling = new BaseSignalling(_signallingServerURL, remote_producer_name);

            _signalling.event_OnRemotePeerId.AddListener(StartPipeline);
            _signalling.event_OnRemotePeerLeft.AddListener(StopPipeline);

            event_OnPipelineStarted = new UnityEvent();
            _command = new CommandBuffer();

            CreateDevice();
            leftTexture = CreateRenderTexture(true, ref leftTextureNativePtr);
            rightTexture = CreateRenderTexture(false, ref rightTextureNativePtr);
        }

        public void Connect()
        {
            _signalling.Connect();
        }

        Texture CreateRenderTexture(bool left, ref IntPtr textureNativePtr)
        {
            textureNativePtr = CreateTexture(width, height, left);
            //textureNativePtr = GetTexturePtr(left);

            if (textureNativePtr != IntPtr.Zero)
            {
                return Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.RGBA32, mipChain: false, linear: true, textureNativePtr);
            }
            else
            {
                Debug.LogError("Texture is null");
                return null;
            }
        }

        void StartPipeline(string remote_peer_id)
        {
            Debug.Log("start pipe " + remote_peer_id);
            CreatePipeline(_signallingServerURL, remote_peer_id);
            event_OnPipelineStarted.Invoke();
            _started = true;
        }

        void StopPipeline()
        {
            Debug.Log("Stop");
            _started = false;
            DestroyPipeline();
            if (_autoreconnect)
                Connect();
        }

        public void Cleanup()
        {
            Debug.Log("Cleanup");
            //_command = null;
            _autoreconnect = false;
            _signalling.Close();
            _signalling.RequestStop();

            StopPipeline();
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
            _command.Dispose();
        }


        public void Render()
        {
            if (_started)
            {
                //CommandBuffer _command = new CommandBuffer();
                _command.Clear();
                _command.IssuePluginEvent(GetRenderEventFunc(), 1);
                Graphics.ExecuteCommandBuffer(_command);
            }
        }

    }
}