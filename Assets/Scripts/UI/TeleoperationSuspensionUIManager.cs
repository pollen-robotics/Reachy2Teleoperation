using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TeleopReachy
{
    public class TeleoperationSuspensionUIManager : MonoBehaviour
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

        // Start is called before the first frame update
        void Start()
        {
            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);

            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(HideSuspensionWarning);

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
            if(robotStatus.IsRobotTeleoperationActive())
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
            if(needUpdateText)
            {
                needUpdateText = false;
                suspensionReasonText.text = reasonString;
            }
        }
    }
}

