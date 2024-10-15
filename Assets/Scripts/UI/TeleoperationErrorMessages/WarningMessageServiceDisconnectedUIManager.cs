using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class WarningMessageServiceDisconnectedUIManager : LazyFollow
    {
        private ConnectionStatus connectionStatus;
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        [SerializeField]
        private Text warningMessage;

        private string messageToDisplay;

        private bool needUpdateWarningMessage;
        private bool wantWarningMessageDisplayed;

        private bool onlyMobileServicesAffected;
        private Coroutine limitDisplayInTime;

        private UserMobilityInput userMobilityInput;
        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, -0.32f, 0.8f);
            }
            else
            { // If oculus 3 or other
                targetOffset = new Vector3(0, -0.32f, 0.7f);
            }
            maxDistanceAllowed = 0;

            connectionStatus = ConnectionStatus.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, ListenToUserMobilityInput);

            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckNewStatus);
            connectionStatus.event_OnRobotReady.AddListener(HideWarningMessage);
            connectionStatus.event_OnRobotUnready.AddListener(ShowWarningMessage);
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideWarningMessage);
            

            needUpdateWarningMessage = false;
            wantWarningMessageDisplayed = false;

            transform.ActivateChildren(false);
        }

        void ListenToUserMobilityInput()
        {
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;
            userMobilityInput.event_OnTriedToSendCommands.AddListener(DisplayMessageForMobility);
        }


        void CheckNewStatus()
        {
            onlyMobileServicesAffected = false;
            if (!connectionStatus.IsRobotInDataRoom())
            {
                if (!connectionStatus.IsRobotInVideoRoom())
                {
                    messageToDisplay = "Robot services have been disconnected";
                }
                else
                {
                    messageToDisplay = "Motor control has been disconnected";
                }
            }
            else
            {
                if (!connectionStatus.IsRobotInVideoRoom())
                {
                    messageToDisplay = "Video stream has been disconnected";
                }
                else
                {
                    wantWarningMessageDisplayed = false;
                }
            }
            needUpdateWarningMessage = true;
        }

        void DisplayMessageForMobility()
        {
            if (!wantWarningMessageDisplayed)
            {
                onlyMobileServicesAffected = true;
                if (robotConfig.HasMobileBase())
                {
                    if (!robotStatus.IsMobilityActive())
                    {
                        messageToDisplay = "Mobile services have been disconnected";
                    }
                    else
                    {
                        if (!robotStatus.IsMobilityOn()) messageToDisplay = "Mobile base has been disabled in options";
                    }
                    ShowWarningMessage();
                }
            }
        }

        void ShowWarningMessage()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                wantWarningMessageDisplayed = true;
                needUpdateWarningMessage = true;
            }
        }

        void Update()
        {
            if (needUpdateWarningMessage)
            {
                warningMessage.text = messageToDisplay;
                if (onlyMobileServicesAffected)
                {
                    if (wantWarningMessageDisplayed)
                    {
                        if (limitDisplayInTime != null) StopCoroutine(limitDisplayInTime);
                        limitDisplayInTime = StartCoroutine(DisplayLimitedInTime());
                    }
                }
                else
                {
                    if (limitDisplayInTime != null) StopCoroutine(limitDisplayInTime);
                    if (wantWarningMessageDisplayed) transform.ActivateChildren(true);
                    else { transform.ActivateChildren(false); }
                }
                needUpdateWarningMessage = false;
            }
        }

        IEnumerator DisplayLimitedInTime()
        {
            transform.ActivateChildren(true);
            yield return new WaitForSeconds(3);
            transform.ActivateChildren(false);
            wantWarningMessageDisplayed = false;
        }

        void HideWarningMessage()
        {
            wantWarningMessageDisplayed = false;
            needUpdateWarningMessage = true;
        }
    }
}