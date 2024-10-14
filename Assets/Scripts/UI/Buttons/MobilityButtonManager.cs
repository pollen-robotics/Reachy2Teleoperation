using System;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class MobilityButtonManager : MonoBehaviour
    {
        public Button mobilityButton;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;
        private ConnectionStatus connectionStatus;

        private bool needUpdateButton = false;
        private bool isInteractable = false;
        private ColorBlock buttonColor;
        private string buttonText;

        void Awake()
        {
            mobilityButton.onClick.AddListener(SwitchButtonMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            connectionStatus = ConnectionStatus.Instance;

            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckMobileBasePresence);
            robotConfig.event_OnConfigChanged.AddListener(CheckMobileBasePresence);

            mobilityButton.interactable = false;

            robotStatus.SetMobilityOn(PlayerPrefs.GetString("mobility_on") != "" ? Convert.ToBoolean(PlayerPrefs.GetString("mobility_on")) : true);

            CheckMobileBasePresence();
        }

        void SwitchButtonMode()
        {
            robotStatus.SetMobilityOn(!robotStatus.IsMobilityOn());
            PlayerPrefs.SetString("mobility_on", robotStatus.IsMobilityOn().ToString());

            if (robotStatus.IsMobilityOn())
            {
                mobilityButton.colors = ColorsManager.colorsActivated;
                mobilityButton.transform.GetChild(0).GetComponent<Text>().text = "Mobile base ON";
            }
            else
            {
                mobilityButton.colors = ColorsManager.colorsDeactivated;
                mobilityButton.transform.GetChild(0).GetComponent<Text>().text = "Mobile base OFF";
            }
        }

        void Update()
        {
            if (needUpdateButton)
            {
                mobilityButton.interactable = isInteractable;
                mobilityButton.colors = buttonColor;
                mobilityButton.transform.GetChild(0).GetComponent<Text>().text = buttonText;
                needUpdateButton = false;
            }
        }

        void CheckMobileBasePresence()
        {
            if (robotConfig.HasMobileBase())
            {
                isInteractable = true;
                if (robotStatus.IsMobilityOn())
                {
                    buttonColor = ColorsManager.colorsActivated;
                    buttonText = "Mobile base ON";
                }
                else
                {
                    buttonColor = ColorsManager.colorsDeactivated;
                    buttonText = "Mobile base OFF";
                }
            }
            else
            {
                buttonColor = ColorsManager.colorsDeactivated;
                buttonText = "Mobile base OFF";
                isInteractable = false;
            }
            needUpdateButton = true;
        }
    }
}