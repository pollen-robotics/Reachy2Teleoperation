using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class LeftGripperButtonManager : MonoBehaviour
    {
        [SerializeField]
        public Button leftArmButton;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private bool needUpdateButton = false;
        private bool isInteractable = false;
        private ColorBlock buttonColor;
        private string buttonText;

        void Awake()
        {
            leftArmButton.onClick.AddListener(SwitchButtonMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckLeftArmPresence);

            leftArmButton.interactable = false;

            CheckLeftArmPresence();
        }

        void SwitchButtonMode()
        {
            robotStatus.SetLeftGripperOn(!robotStatus.IsLeftGripperOn());

            if (robotStatus.IsLeftGripperOn())
            {
                leftArmButton.colors = ColorsManager.colorsActivated;
                leftArmButton.transform.GetChild(0).GetComponent<Text>().text = "Left gripper ON";
            }
            else
            {
                leftArmButton.colors = ColorsManager.colorsDeactivated;
                leftArmButton.transform.GetChild(0).GetComponent<Text>().text = "Left gripper OFF";
            }
        }

        void Update()
        {
            if (needUpdateButton)
            {
                leftArmButton.interactable = isInteractable;
                leftArmButton.colors = buttonColor;
                leftArmButton.transform.GetChild(0).GetComponent<Text>().text = buttonText;
                needUpdateButton = false;
            }
        }

        void CheckLeftArmPresence()
        {
            if (robotConfig.HasLeftArm())
            {
                isInteractable = true;
                if (robotStatus.IsLeftGripperOn())
                {
                    buttonColor = ColorsManager.colorsActivated;
                    buttonText = "Left gripper ON";
                }
                else
                {
                    buttonColor = ColorsManager.colorsDeactivated;
                    buttonText = "Left gripper OFF";
                }
            }
            else
            {
                buttonColor = ColorsManager.colorsDeactivated;
                buttonText = "Left gripper OFF";
                isInteractable = false;
            }
            needUpdateButton = true;
        }
    }
}
