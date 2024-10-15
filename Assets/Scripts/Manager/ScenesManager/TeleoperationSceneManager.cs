using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationSceneManager : MonoBehaviour
    {
        private TransitionRoomManager transitionRoomManager;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private bool needUpdateRobotDisplay;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachy;
        private bool robotDisplayed;

        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(DisplayReachy);

            if (robotConfig.HasMobileBase())
            {
                robotStatus.SetMobilityActive(true);
            }
            if (robotStatus.IsRobotPositionLocked || robotStatus.IsGraspingLockActivated())
            {
                if (robotStatus.IsLeftGripperClosed())
                {
                    UserInputManager.Instance.UserMovementsInput.ForceLeftGripperStatus(true);
                    robotStatus.SetGraspingLockActivated(true);
                }
                if (robotStatus.IsRightGripperClosed())
                {
                    UserInputManager.Instance.UserMovementsInput.ForceRightGripperStatus(true);
                    robotStatus.SetGraspingLockActivated(true);
                }
            }
        }

        void StopTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: StopTeleoperation");
            //robotStatus.SetEmotionsActive(false);
            robotStatus.SetMobilityActive(false);
        }

        void Update()
        {
            if(needUpdateRobotDisplay)
            {
                needUpdateRobotDisplay = false;
                reachy.transform.switchRenderer(robotDisplayed);
                if (robotConfig.GotReachyConfig())
                {
                    reachy.head.transform.switchRenderer(robotConfig.HasHead() && robotDisplayed);
                    reachy.l_arm.transform.switchRenderer(robotConfig.HasLeftArm() && robotDisplayed);
                    reachy.r_arm.transform.switchRenderer(robotConfig.HasRightArm() && robotDisplayed);
                    reachy.mobile_base.transform.switchRenderer(robotConfig.HasMobileBase() && robotDisplayed);
                }
            }
        }

        private void DisplayReachy()
        {
            DisplayReachy(robotDisplayed);
        }

        private void DisplayReachy(bool enabled)
        {
            robotDisplayed = enabled;
            needUpdateRobotDisplay = true;
        }
    }
}