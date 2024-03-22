using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationManager : MonoBehaviour
    {
        private TransitionRoomManager transitionRoomManager;

        private RobotStatus robotStatus;

        [SerializeField]
        private Transform reachy;

        private RobotConfig robotConfig;

        void Start()
        {
            EventManager.StartListening(EventNames.QuitMirrorScene, StartTeleoperation);
            EventManager.StartListening(EventNames.BackToMirrorScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, SuspendTeleoperation);
            robotConfig = RobotDataManager.Instance.RobotConfig;

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

        private void DisplayReachy(bool enabled)
        {
            reachy.switchRenderer(enabled);
            if (robotConfig.HasHead())
                reachy.GetChild(0).switchRenderer(enabled);
            if (robotConfig.HasLeftArm())
                reachy.GetChild(1).switchRenderer(enabled);
            if (robotConfig.HasRightArm())
                reachy.GetChild(3).switchRenderer(enabled);
            if (robotConfig.HasMobileBase())
                reachy.GetChild(5).switchRenderer(enabled);
        }
    }
}