using UnityEngine;
using UnityEngine.UI;
using Reachy.Part.Arm;

namespace TeleopReachy
{
    public class IKModeButtonsManager : MonoBehaviour
    {
        public Button modeFullControlButton;
        public Button modeHumanLikeButton;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private bool needUpdateButton;
        private bool isInteractable = false;
        //private ColorBlock buttonColorFullControl;
        //private ColorBlock buttonColorLocked;

        void Awake()
        {
            modeFullControlButton.onClick.AddListener(delegate { SwitchToGraspingLockMode(IKMode.Unconstrained); });
            modeHumanLikeButton.onClick.AddListener(delegate { SwitchToGraspingLockMode(IKMode.LowElbow); });

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckArmPresence);
            robotStatus.event_OnStopTeleoperation.AddListener(UpdateIKModeButtons);

            modeFullControlButton.interactable = isInteractable;
            modeHumanLikeButton.interactable = isInteractable;

            needUpdateButton = false;

            CheckArmPresence();
        }

        void SwitchToGraspingLockMode(IKMode mode)
        {
            robotStatus.SetIKMode(mode);
            needUpdateButton = true;
        }

        void Update()
        {
            if (needUpdateButton)
            {
                modeFullControlButton.interactable = isInteractable;
                modeHumanLikeButton.interactable = isInteractable;
                if (robotStatus.GetIKMode() == IKMode.LowElbow)
                {
                    modeFullControlButton.colors = ColorsManager.colorsDeactivated;
                    modeHumanLikeButton.colors = ColorsManager.colorsActivated;

                }
                else if (robotStatus.GetIKMode() == IKMode.Unconstrained)
                {
                    modeFullControlButton.colors = ColorsManager.colorsActivated;
                    modeHumanLikeButton.colors = ColorsManager.colorsDeactivated;
                }
                needUpdateButton = false;
            }
        }

        void UpdateIKModeButtons()
        {
            needUpdateButton = true;
        }

        void CheckArmPresence()
        {
            if (robotConfig.HasRightArm() || robotConfig.HasLeftArm())
            {
                isInteractable = true;
            }
            else
            {
                isInteractable = false;
            }
            needUpdateButton = true;
        }
    }
}