using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class WarningMessageServiceDisconnectedUIManager : InformationalPanel
    {
        private ConnectionStatus connectionStatus;
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private TeleoperationManager teleoperationManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.32f, 0.8f));

            connectionStatus = ConnectionStatus.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            teleoperationManager = TeleoperationManager.Instance;
            teleoperationManager.event_OnTriedToSendMobilityCommands.AddListener(DisplayMessageForMobility);

            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(DisplayNewConnectionStatus);
            connectionStatus.event_OnRobotReady.AddListener(HideInfoMessage);
            connectionStatus.event_OnRobotUnready.AddListener(ShowInfoMessage);

            HideInfoMessage();
        }

        void DisplayNewConnectionStatus()
        {
            if (!connectionStatus.IsRobotInDataRoom())
            {
                if (!connectionStatus.IsRobotInVideoRoom())
                {
                    textToDisplay = "Robot services have been disconnected";
                }
                else
                {
                    textToDisplay = "Motor control has been disconnected";
                }
            }
            else
            {
                if (!connectionStatus.IsRobotInVideoRoom())
                {
                    textToDisplay = "Video stream has been disconnected";
                }
            }
            ShowInfoMessage();
        }

        void DisplayMessageForMobility()
        {
            if (robotConfig.HasMobileBase())
            {
                textToDisplay = "Mobile base has been disabled in options";
                ShowInfoMessage();
            }
        }
    }
}