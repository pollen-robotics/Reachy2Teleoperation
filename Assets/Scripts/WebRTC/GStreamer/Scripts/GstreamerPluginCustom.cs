using UnityEngine;
using UnityEngine.Events;

namespace GstreamerWebRTC
{
    public class GStreamerPluginCustom : GStreamerPlugin
    {
        public Renderer screen;
        public UnityEvent<bool> event_OnVideoRoomStatusHasChanged;
        public UnityEvent<bool> event_OnAudioReceiverRoomStatusHasChanged;
        public UnityEvent<bool> event_AudioSenderStatusHasChanged;

        private Texture left = null;
        private Texture right = null;

        override protected void Init()
        {
            if (screen == null)
                Debug.LogError("Screen is not assigned!");

            if (ip_address == "")
            {
                ip_address = PlayerPrefs.GetString("robot_ip");
                Debug.Log("Set IP address to: " + ip_address);
            }

            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            screen.material.SetTexture("_LeftTex", left);
            screen.material.SetTexture("_RightTex", left);

            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.Connect();
        }

        override protected void PipelineStarted()
        {
            Debug.Log("Pipeline started Custom");
            event_AudioSenderStatusHasChanged.Invoke(true);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(true);
            event_AudioSenderStatusHasChanged.Invoke(true);
        }

        override protected void OnDisable()
        {
            event_AudioSenderStatusHasChanged.Invoke(false);
            event_OnAudioReceiverRoomStatusHasChanged.Invoke(false);
            event_AudioSenderStatusHasChanged.Invoke(false);
            base.OnDisable();
        }

        void Update()
        {
            renderingPlugin.Render();
        }

        public Texture GetLeftTexture()
        {
            return left;
        }

        public Texture GetRightTexture()
        {
            return right;
        }

    }
}