using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class TeleoperationSuspensionUIManager : LazyFollow
    {
        [SerializeField]
        private Transform loaderA;

        [SerializeField]
        private Text suspensionReasonText;

        private UserEmergencyStopInput userEmergencyStop;

        private bool isLoaderActive = false;

        private RobotStatus robotStatus;
        private TeleoperationSuspensionManager suspensionManager;

        private bool needUpdateText = false;
        private string reasonString;
        //private ControllersManager controllers;

        // Start is called before the first frame update
        void Start()
        {
            //controllers = ActiveControllerManager.Instance.ControllersManager;
            // if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            // {
            //     targetOffset = new Vector3(0, -0.15f, 0.8f);

            // }
            // else {
            targetOffset = new Vector3(0, -0.15f, 0.8f);

            // }
            maxDistanceAllowed = 0;

            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);

            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideSuspensionWarning);

            suspensionManager = TeleoperationSuspensionManager.Instance;

            HideSuspensionWarning();
        }

        void Init()
        {
            userEmergencyStop = UserInputManager.Instance.UserEmergencyStopInput;
            userEmergencyStop.event_OnEmergencyStopCalled.AddListener(EmergencyStopCalled);
        }

        void HeadsetRemoved()
        {
            reasonString = "Headset has been removed";
            needUpdateText = true;
            DisplaySuspensionWarning();
        }

        void EmergencyStopCalled()
        {
            reasonString = "Emergency stop activated";
            needUpdateText = true;
            DisplaySuspensionWarning();
        }

        // Update is called once per frame
        void DisplaySuspensionWarning()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                isLoaderActive = true;
                transform.ActivateChildren(true);
            }
        }

        void HideSuspensionWarning()
        {
            loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = suspensionManager.indicatorTimer;
            isLoaderActive = false;
            transform.ActivateChildren(false);
        }


        // Update is called once per frame
        void Update()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = suspensionManager.indicatorTimer;
            }
            if (needUpdateText)
            {
                needUpdateText = false;
                suspensionReasonText.text = reasonString;
            }
        }
    }
}

