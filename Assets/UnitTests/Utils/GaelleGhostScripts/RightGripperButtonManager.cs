using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class RightGripperButtonManager : MonoBehaviour
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

            robotConfig.event_OnConfigChanged.AddListener(CheckRightArmPresence);

            leftArmButton.interactable = false;

            CheckRightArmPresence();
        }

        void SwitchButtonMode()
        {
            robotStatus.SetRightGripperOn(!robotStatus.IsRightGripperOn());

            if (robotStatus.IsRightGripperOn())
            {
                leftArmButton.colors = ColorsManager.colorsActivated;
                leftArmButton.transform.GetChild(0).GetComponent<Text>().text = "Right gripper ON";
            }
            else
            {
                leftArmButton.colors = ColorsManager.colorsDeactivated;
                leftArmButton.transform.GetChild(0).GetComponent<Text>().text = "Right gripper OFF";
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

        void CheckRightArmPresence()
        {
            if (robotConfig.HasRightArm())
            {
                isInteractable = true;
                if (robotStatus.IsRightGripperOn())
                {
                    buttonColor = ColorsManager.colorsActivated;
                    buttonText = "Right gripper ON";
                }
                else
                {
                    buttonColor = ColorsManager.colorsDeactivated;
                    buttonText = "Right gripper OFF";
                }
            }
            else
            {
                buttonColor = ColorsManager.colorsDeactivated;
                buttonText = "Right gripper OFF";
                isInteractable = false;
            }
            needUpdateButton = true;
        }
    }
}