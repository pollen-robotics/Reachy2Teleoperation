using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationManager : MonoBehaviour
    {
        private TransitionRoomManager transitionRoomManager;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;


        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachy;
        private bool robotDisplayed;

        void Start()
        {
            EventManager.StartListening(EventNames.QuitMirrorScene, StartTeleoperation);
            EventManager.StartListening(EventNames.BackToMirrorScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, SuspendTeleoperation);
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(DisplayReachy);

            DisplayReachy(true);

            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void StartTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: StartTeleoperation");

            DisplayReachy(false);

            robotStatus.TurnRobotStiff();
            /*if (robotConfig.HasHead())
            {
                robotStatus.SetEmotionsActive(true);
            }*/
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
            robotStatus.StartRobotTeleoperation();
        }

        void StopTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: StopTeleoperation");
            //robotStatus.SetEmotionsActive(false);
            robotStatus.SetMobilityActive(false);
            robotStatus.StopRobotTeleoperation();
            DisplayReachy(true);
        }

        void SuspendTeleoperation()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                robotStatus.SuspendRobotTeleoperation();
            }
        }

        private void DisplayReachy()
        {
            DisplayReachy(robotDisplayed);
        }

        private void DisplayReachy(bool enabled)
        {
            robotDisplayed = enabled;
            reachy.transform.switchRenderer(enabled);
            if (robotConfig.GotReachyConfig())
            {
                reachy.head.transform.switchRenderer(robotConfig.HasHead() && enabled);
                reachy.l_arm.transform.switchRenderer(robotConfig.HasLeftArm() && enabled);
                reachy.r_arm.transform.switchRenderer(robotConfig.HasRightArm() && enabled);
                reachy.mobile_base.transform.switchRenderer(robotConfig.HasMobileBase() && enabled);
            }
        }
    }
}