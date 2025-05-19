/* Copyright(c) Pollen Robotics, all rights reserved.
 This source code is licensed under the license found in the
 LICENSE file in the root directory of this source tree. */

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Android;

namespace GstreamerWebRTC
{
    public class GStreamerRenderingPlugin
    {
        [DllImport("UnityGStreamerPlugin")]
        private static extern void CreatePipeline(string uri, string remote_peer_id);

        [DllImport("UnityGStreamerPlugin")]
        private static extern void CreateDevice();

        [DllImport("UnityGStreamerPlugin")]
        private static extern void DestroyPipeline();

        [DllImport("UnityGStreamerPlugin")]
        private static extern IntPtr CreateTexture(uint width, uint height, bool left);


#if UNITY_ANDROID
        [DllImport("UnityGStreamerPlugin")]
        private static extern void SetTextureFromUnity(IntPtr texture, bool left, int width, int height);
#endif

        [DllImport("UnityGStreamerPlugin")]
        private static extern void ReleaseTexture(IntPtr texture);

        [DllImport("UnityGStreamerPlugin")]
        private static extern IntPtr GetRenderEventFunc();


        private IntPtr leftTextureNativePtr;

        private IntPtr rightTextureNativePtr;

        private string _signallingServerURL;
        private BaseSignalling _signalling;

        public bool producer = false;
        public string remote_producer_name = "robot";

        const uint width = 960;
        const uint height = 720;

        public UnityEvent event_OnPipelineStarted;
        public UnityEvent event_OnPipelineStopped;

        CommandBuffer _command = null;

        private bool _autoreconnect = false;



#if UNITY_ANDROID

        private bool nativeTexPtrSet = false;

        private class PluginCallback : AndroidJavaProxy
        {
            private Action<int> callback;
            public PluginCallback(Action<int> callback) : base("com.pollenrobotics.gstreamer.GstreamerActivity$OnInitializedListener")
            {
                this.callback = callback;
            }
            private void onInitialized(int textureId)
            {
                this.callback(textureId);
            }
        }
#endif

        public GStreamerRenderingPlugin(string ip_address, ref Texture leftTexture, ref Texture rightTexture)
        {
            _autoreconnect = true;
            _signallingServerURL = "ws://" + ip_address + ":8443";

            _signalling = new BaseSignalling(_signallingServerURL, remote_producer_name);

            _signalling.event_OnRemotePeerId.AddListener(StartPipeline);
            _signalling.event_OnRemotePeerLeft.AddListener(StopPipeline);

            event_OnPipelineStarted = new UnityEvent();
            event_OnPipelineStopped = new UnityEvent();
            _command = new CommandBuffer();

#if UNITY_ANDROID
            CheckMicPermission();

#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            CreateDevice();
            leftTexture = CreateRenderTexture(true, ref leftTextureNativePtr);
            rightTexture = CreateRenderTexture(false, ref rightTextureNativePtr);
#endif
        }

#if UNITY_ANDROID
        private void CheckMicPermission()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("Microphone permission already granted");
            }
            else
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

        }

        public bool IsNativePtrSet()
        {
            return nativeTexPtrSet;
        }

        public Texture SetTextures(bool left)
        {
            if (left)
                return CreateRenderTexture(left, ref leftTextureNativePtr);
            else
                return CreateRenderTexture(left, ref rightTextureNativePtr);
        }

#endif

        public void Connect()
        {
            _signalling.Connect();
        }

        Texture CreateRenderTexture(bool left, ref IntPtr textureNativePtr)
        {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            textureNativePtr = CreateTexture(width, height, left);

            if (textureNativePtr != IntPtr.Zero)
            {
                Debug.Log("Texture is set " + left + " " + textureNativePtr);
                return Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.RGBA32, mipChain: false, linear: true, textureNativePtr);
            }
            else
            {
                Debug.LogError("Texture is null " + left);
                return null;
            }
#elif UNITY_ANDROID
            Debug.Log("CreateRenderTexture " + left);
            Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            textureNativePtr = tex.GetNativeTexturePtr();
            SetTextureFromUnity(textureNativePtr, left, (int)width, (int)height);
            Debug.Log("Texture is set " + left + " " + textureNativePtr);
            return tex;
#endif
        }

        void StartPipeline(string remote_peer_id)
        {
            Debug.Log("start rendering pipe " + remote_peer_id);
            CreatePipeline(_signallingServerURL, remote_peer_id);
            event_OnPipelineStarted.Invoke();
        }



        void StopPipeline()
        {
            DestroyPipeline();
            event_OnPipelineStopped.Invoke();
            if (_autoreconnect)
                Connect();
        }

        public void Cleanup()
        {
            Debug.Log("Cleanup");

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
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            if (_started)
            {
                _command.Clear();
                _command.IssuePluginEvent(GetRenderEventFunc(), 1);
                Graphics.ExecuteCommandBuffer(_command);
            }
#elif UNITY_ANDROID
            if (!nativeTexPtrSet)
            {
                _command.Clear();
                _command.IssuePluginEvent(GetRenderEventFunc(), 0);
                Graphics.ExecuteCommandBuffer(_command);
                nativeTexPtrSet = true;
                Debug.Log("Android texture created");
            }
            else
            {
                _command.Clear();
                _command.IssuePluginEvent(GetRenderEventFunc(), 1);
                Graphics.ExecuteCommandBuffer(_command);

            }
#endif

        }

    }
}