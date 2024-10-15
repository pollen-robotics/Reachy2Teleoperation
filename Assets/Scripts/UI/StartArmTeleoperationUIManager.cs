using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class StartArmTeleoperationUIManager : LazyFollow
    {
        //private RobotConfig robotConfig;

        private ControllersManager controllers;

        private bool needUpdateInfoMessage;
        private bool wantInfoMessageDisplayed;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, -0.1f, 0.5f);
            }
            else
            {
                targetOffset = new Vector3(0, -0.1f, 0.7f);
            }
            maxDistanceAllowed = 0;
            //robotConfig = RobotDataManager.Instance.RobotConfig;

            EventManager.StartListening(EventNames.OnStartArmTeleoperation, HideInfoMessage);
            EventManager.StartListening(EventNames.OnStartTeleoperation, ShowInfoMessage);
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideInfoMessage);

            needUpdateInfoMessage = false;
            wantInfoMessageDisplayed = false;

            EventManager.StartListening(EventNames.OnEmergencyStop, HideInfoMessage);

            transform.ActivateChildren(false);
        }

        void ShowInfoMessage()
        {
            wantInfoMessageDisplayed = true;
            needUpdateInfoMessage = true;
        }

        void Update()
        {
            if (needUpdateInfoMessage)
            {
                transform.ActivateChildren(wantInfoMessageDisplayed);
                needUpdateInfoMessage = false;
            }
        }

        void HideInfoMessage()
        {
            wantInfoMessageDisplayed = false;
            needUpdateInfoMessage = true;
        }
    }
}