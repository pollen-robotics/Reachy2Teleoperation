using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;


namespace TeleopReachy
{
    public class PartButtonManager : MonoBehaviour
    {
        [SerializeField]
        private Part part;

        [SerializeField]
        private Button partButton;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private bool needUpdateButton = false;
        private bool isInteractable = false;
        private ColorBlock buttonColor;
        private string buttonText;

        void Awake()
        {
            partButton.onClick.AddListener(SwitchButtonMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckPartPresence);

            partButton.interactable = false;

            CheckPartPresence();
        }

        string PartToString()
        {
            string[] split =  Regex.Split(Enum.GetName(typeof(Part), part), @"(?<!^)(?=[A-Z])");
            string nameToString = "";
            foreach(string word in split)
            {
                nameToString += word + " ";
            }
            return nameToString;
        }

        void SwitchButtonMode()
        {
            robotStatus.SetPartOn(part, !robotStatus.IsPartOn(part));

            if (robotStatus.IsPartOn(part))
            {
                partButton.colors = ColorsManager.colorsActivated;
                partButton.transform.GetChild(0).GetComponent<Text>().text = PartToString() + "ON";
            }
            else
            {
                partButton.colors = ColorsManager.colorsDeactivated;
                partButton.transform.GetChild(0).GetComponent<Text>().text = PartToString() + "OFF";
            }
        }

        void Update()
        {
            if (needUpdateButton)
            {
                partButton.interactable = isInteractable;
                partButton.colors = buttonColor;
                partButton.transform.GetChild(0).GetComponent<Text>().text = buttonText;
                needUpdateButton = false;
            }
        }

        void CheckPartPresence()
        {
            if (robotConfig.HasPart(part))
            {
                isInteractable = true;
                if (robotStatus.IsPartOn(part))
                {
                    buttonColor = ColorsManager.colorsActivated;
                    buttonText = PartToString() + "ON";
                }
                else
                {
                    buttonColor = ColorsManager.colorsDeactivated;
                    buttonText = PartToString() + "OFF";
                }
            }
            else
            {
                buttonColor = ColorsManager.colorsDeactivated;
                buttonText = PartToString() + "OFF";
                isInteractable = false;
            }
            needUpdateButton = true;
        }
    }
}