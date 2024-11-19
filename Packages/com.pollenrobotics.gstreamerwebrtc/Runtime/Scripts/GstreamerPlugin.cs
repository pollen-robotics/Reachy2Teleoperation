/* Copyright(c) Pollen Robotics, all rights reserved.
 This source code is licensed under the license found in the
 LICENSE file in the root directory of this source tree. */

using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Events;

namespace GstreamerWebRTC
{
    public class GStreamerPlugin : MonoBehaviour
    {

        [Tooltip("RawImage on which the left texture will be rendered")]
        public RawImage leftRawImage;
        [Tooltip("RawImage on which the right texture will be rendered")]
        public RawImage rightRawImage;
        protected GStreamerRenderingPlugin renderingPlugin = null;

        protected GStreamerDataPlugin dataPlugin = null;
        protected DebugFromPlugin debug = null;

        [Tooltip("IP address of the robot (i.e. signalling server). PlayerPrefs.GetString(\"ip_address\") if empty")]
        public string ip_address = "";

        private Thread cleaning_thread = null;
        private Thread init_thread = null;

        public UnityEvent<bool> event_OnPipelineRenderingRunning = new UnityEvent<bool>();
        public UnityEvent<bool> event_OnPipelineDataRunning = new UnityEvent<bool>();

        void OnEnable()
        {
            debug = new DebugFromPlugin();
        }

        void OnDestroy()
        {
            cleaning_thread?.Join();
        }

        void Start()
        {
            if (cleaning_thread != null)
            {
                cleaning_thread.Join();
            }

            if (ip_address == "")
            {
                ip_address = PlayerPrefs.GetString("robot_ip");
                Debug.Log("Set IP address to: " + ip_address);
            }

            //GStreamerRenderingPlugin has to run in main thread
            InitAV();

            init_thread = new Thread(InitData);
            init_thread.Start();
        }

        protected virtual void InitAV()
        {
            if (leftRawImage == null)
                Debug.LogError("Left image is not assigned!");

            if (rightRawImage == null)
                Debug.LogError("Right image is not assigned!");

            Texture left = null, right = null;
            renderingPlugin = new GStreamerRenderingPlugin(ip_address, ref left, ref right);
            leftRawImage.texture = left;
            rightRawImage.texture = right;
            renderingPlugin.event_OnPipelineStarted.AddListener(PipelineStarted);
            renderingPlugin.event_OnPipelineStopped.AddListener(PipelineStopped);
            renderingPlugin.Connect();
        }

        protected virtual void InitData()
        {
            dataPlugin = new GStreamerDataPlugin(ip_address);
            dataPlugin.event_OnPipelineStarted.AddListener(PipelineDataStarted);
            dataPlugin.event_OnPipelineStopped.AddListener(PipelineDataStopped);
            GStreamerDataPlugin.event_OnChannelServiceOpen.AddListener(OnChannelServiceOpen);
            GStreamerDataPlugin.event_OnChannelServiceData.AddListener(OnChannelServiceData);
            dataPlugin.Connect();
        }

        protected virtual void PipelineStarted()
        {
            Debug.Log("Pipeline started");
            event_OnPipelineRenderingRunning.Invoke(true);
        }

        protected virtual void PipelineDataStarted()
        {
            Debug.Log("Pipeline data started");
            event_OnPipelineDataRunning.Invoke(true);
        }

        protected virtual void PipelineStopped()
        {
            Debug.Log("Pipeline stopped");
            event_OnPipelineRenderingRunning.Invoke(false);
        }

        protected virtual void PipelineDataStopped()
        {
            Debug.Log("Pipeline data stopped");
            event_OnPipelineDataRunning.Invoke(false);
        }

        protected virtual void OnDisable()
        {
            if (init_thread != null)
            {
                init_thread.Join();
            }

            cleaning_thread = new Thread(Cleanup);
            cleaning_thread.Start();
        }

        void Cleanup()
        {
            renderingPlugin.Cleanup();
            dataPlugin.Cleanup();
        }

        void Update()
        {
            renderingPlugin.Render();
        }

        protected virtual void OnChannelCommandOpen()
        {
            Debug.Log("Channel command open");
        }

        //Data channels
        protected virtual void OnChannelServiceOpen()
        {
            byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x20, 0x20, 0x20, 0x20 }; ;
            GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
        }


        protected virtual void OnChannelServiceData(byte[] data)
        {
            byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x20, 0x20, 0x20, 0x20 }; ;
            GStreamerDataPlugin.SendBytesChannelService(bytes, bytes.Length);
        }

        protected void SendCommandToChannel(byte[] commands)
        {
            GStreamerDataPlugin.SendBytesChannelCommand(commands, commands.Length);
        }
    }
}
