using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class ConnectionStatus : Singleton<ConnectionStatus>
    {
        private bool isRobotConfigReady;
        private bool isRobotInDataRoom;
        private bool isRobotInVideoRoom;
        private bool isRobotInAudioReceiverRoom;
        private bool isRobotInAudioSenderRoom;

        private bool isRobotReady;

        private bool areRobotServicesRestarting;

        private GStreamerPluginCustom gstreamerPlugin;

        public UnityEvent event_OnConnectionStatusHasChanged;
        public UnityEvent event_OnRobotReady;
        public UnityEvent event_OnRobotUnready;

        private bool statusChanged;

        private RobotConfig robotConfig;

        private Coroutine waitForConnection;

        [SerializeField]
        private bool checkVideoStream;

        void Start()
        {
            gstreamerPlugin = WebRTCManager.Instance.gstreamerPlugin;

            robotConfig = RobotDataManager.Instance.RobotConfig;

            robotConfig.event_OnConfigChanged.AddListener(RobotConfigurationChanged);

            isRobotConfigReady = false;
            isRobotInDataRoom = false;
            isRobotInVideoRoom = false;
            isRobotInAudioReceiverRoom = false;
            isRobotInAudioSenderRoom = false;

            isRobotReady = false;

            areRobotServicesRestarting = true;

            statusChanged = false;

            if (gstreamerPlugin != null)
            {
                gstreamerPlugin.event_OnVideoRoomStatusHasChanged.AddListener(VideoControllerStatusHasChanged);
                gstreamerPlugin.event_OnAudioReceiverRoomStatusHasChanged.AddListener(AudioReceiverControllerStatusHasChanged);
                gstreamerPlugin.event_OnVideoRoomStatusHasChanged.AddListener(AudioSenderStatusHasChanged);
                gstreamerPlugin.event_DataControllerStatusHasChanged.AddListener(DataControllerStatusHasChanged);
            }

            waitForConnection = StartCoroutine(WaitForConnection());
            Debug.LogError(checkVideoStream);
        }

        public bool IsRobotInDataRoom()
        {
            return isRobotInDataRoom;
        }

        public bool IsRobotInVideoRoom()
        {
            return isRobotInVideoRoom;
        }

        public bool IsRobotInAudioReceiverRoom()
        {
            return isRobotInAudioReceiverRoom;
        }

        public bool IsRobotInAudioSenderRoom()
        {
            return isRobotInAudioSenderRoom;
        }

        public bool IsRobotReady()
        {
            return isRobotReady;
        }

        public bool AreRobotServicesRestarting()
        {
            return areRobotServicesRestarting;
        }

        public void SetRobotServicesRestarting(bool areRestarting)
        {
            areRobotServicesRestarting = areRestarting;
        }


        void VideoControllerStatusHasChanged(bool isRobotInRoom)
        {
            Debug.Log("[ConnectionStatus] VideoControllerStatusHasChanged");
            isRobotInVideoRoom = isRobotInRoom;
            statusChanged = true;
        }

        void AudioReceiverControllerStatusHasChanged(bool isRobotInRoom)
        {
            Debug.Log("[ConnectionStatus] AudioReceiverControllerStatusHasChanged");
            isRobotInAudioReceiverRoom = isRobotInRoom;
            statusChanged = true;
        }

        void AudioSenderStatusHasChanged(bool isRobotInRoom)
        {
            Debug.Log("[ConnectionStatus] AudioSenderStatusHasChanged");
            isRobotInAudioSenderRoom = isRobotInRoom;
            statusChanged = true;
        }


        void DataControllerStatusHasChanged(bool isRobotInRoom)
        {
            Debug.Log("[ConnectionStatus] DataControllerStatusHasChanged :" + isRobotInRoom);
            isRobotInDataRoom = isRobotInRoom;
            statusChanged = true;
        }

        void RobotConfigurationChanged()
        {
            Debug.Log("[ConnectionStatus] Config Changed");
            isRobotConfigReady = robotConfig.GotReachyConfig();
            statusChanged = true;
        }

        void Update()
        {
            if (statusChanged)
            {
                statusChanged = false;
                if (isRobotInDataRoom && isRobotConfigReady && (
                    (checkVideoStream && (robotConfig.HasHead() && isRobotInVideoRoom) || !robotConfig.HasHead())
                    || !checkVideoStream))
                {
                    if (!isRobotReady)
                    {
                        isRobotReady = true;
                        if (waitForConnection != null) StopCoroutine(waitForConnection);
                        areRobotServicesRestarting = false;
                        event_OnRobotReady.Invoke();
                    }
                }
                else
                {
                    isRobotReady = false;
                    event_OnRobotUnready.Invoke();
                }

                event_OnConnectionStatusHasChanged.Invoke();
            }
        }

        IEnumerator WaitForConnection()
        {
            yield return new WaitForSeconds(5);
            areRobotServicesRestarting = false;
            statusChanged = true;
        }
    }
}