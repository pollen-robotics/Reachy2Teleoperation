using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class ConnectionStatus : MonoBehaviour
    {
        private bool isRobotConfigReady;
        private bool isRobotInDataRoom;
        private bool isRobotInVideoRoom;
        private bool isRobotInAudioReceiverRoom;
        private bool isRobotInAudioSenderRoom;
        private bool isRobotInRestartRoom;
        private bool hasRobotJustLeftDataRoom;

        private bool isRobotReady;

        private bool areRobotServicesRestarting;

        private GstreamerUnityGStreamerPlugin audioVideoController;
        //private WebRTCAudioSender microphoneController;
        private WebRTCData dataController;

        public UnityEvent event_OnConnectionStatusHasChanged;
        public UnityEvent event_OnRobotReady;

        public UnityEvent event_OnRobotUnready;

        private bool statusChanged;

        private RobotConfig robotConfig;

        private Coroutine waitForConnection;

        void Start()
        {
            dataController = WebRTCManager.Instance.webRTCDataController;
            audioVideoController = WebRTCManager.Instance.webRTCVideoController;
            //microphoneController = WebRTCManager.Instance.webRTCAudioSender;

            robotConfig = RobotDataManager.Instance.RobotConfig;

            robotConfig.event_OnConfigChanged.AddListener(RobotConfigurationChanged);

            isRobotConfigReady = false;
            isRobotInDataRoom = false;
            isRobotInVideoRoom = false;
            isRobotInAudioReceiverRoom = false;
            isRobotInAudioSenderRoom = false;
            isRobotInRestartRoom = false;

            isRobotReady = false;

            hasRobotJustLeftDataRoom = false;

            areRobotServicesRestarting = true;

            statusChanged = false;

            if (audioVideoController != null)
            {
                audioVideoController.event_OnVideoRoomStatusHasChanged.AddListener(VideoControllerStatusHasChanged);
                audioVideoController.event_OnAudioReceiverRoomStatusHasChanged.AddListener(AudioReceiverControllerStatusHasChanged);
                audioVideoController.event_OnVideoRoomStatusHasChanged.AddListener(AudioSenderStatusHasChanged);
            }
            if (dataController != null) dataController.event_DataControllerStatusHasChanged.AddListener(DataControllerStatusHasChanged);
            //if (microphoneController != null) microphoneController.event_AudioSenderStatusHasChanged.AddListener(AudioSenderStatusHasChanged);

            waitForConnection = StartCoroutine(WaitForConnection());
        }

        public bool IsRobotInDataRoom()
        {
            return isRobotInDataRoom;
        }

        public bool HasRobotJustLeftDataRoom()
        {
            return hasRobotJustLeftDataRoom;
        }

        public bool IsRobotConfigReady()
        {
            return isRobotConfigReady;
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

        public bool IsRobotInRestartRoom()
        {
            return isRobotInRestartRoom;
        }

        public bool IsRobotReady()
        {
            return isRobotReady;
        }

        public bool IsServerConnected()
        {
            //return isServerConnected;
            //Todo
            return true;
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
            Debug.Log("[ConnectionStatus] DataControllerStatusHasChanged");
            isRobotInDataRoom = isRobotInRoom;
            if (isRobotInDataRoom)
            {
                hasRobotJustLeftDataRoom = false;
            }
            else
            {
                hasRobotJustLeftDataRoom = true;
            }
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
                if (isRobotInDataRoom && isRobotConfigReady && ((robotConfig.HasHead() && isRobotInVideoRoom) || !robotConfig.HasHead()))
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
                if (!isRobotInDataRoom)
                {
                    hasRobotJustLeftDataRoom = false;
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