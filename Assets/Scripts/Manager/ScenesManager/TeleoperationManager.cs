using System.Collections;
using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;

namespace TeleopReachy
{
    public class TeleoperationManager : Singleton<TeleoperationManager>
    {
        private ConnectionStatus connectionStatus;
        private RobotConfig robotConfig;
        private RobotStatus robotStatus;
        private RobotJointCommands jointsCommands;
        private RobotMobilityCommands mobilityCommands;

        private UserMovementsInput userMovementsInput;
        private UserMobilityInput userMobilityInput;

        public enum TeleoperationSuspensionCase 
        {
            None, HeadsetRemoved, EmergencyStopActivated,
        }

        public TeleoperationSuspensionCase reasonForSuspension { get; private set; }

        void Start()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, StartTeleoperation);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, InitUserInputs);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);
            EventManager.StartListening(EventNames.OnEmergencyStop, EmergencyStopActivated);

            EventManager.StartListening(EventNames.RobotDataSceneLoaded, InitRobotData);
        }

        void InitUserInputs()
        {
            userMovementsInput = UserInputManager.Instance.UserMovementsInput;
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;
        }

        void InitRobotData()
        {
            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnRobotReady.AddListener(ReadyForTeleop);

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            jointsCommands = RobotDataManager.Instance.RobotJointCommands;
            mobilityCommands = RobotDataManager.Instance.RobotMobilityCommands;
        }

        void ReadyForTeleop()
        {
            EventManager.TriggerEvent(EventNames.OnInitializeRobotStateRequested);
        }

        void StartTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);
            EventManager.TriggerEvent(EventNames.OnStartTeleoperation);
        }
        
        void StopTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnStopTeleoperation);
        }

        void EmergencyStopActivated()
        {
            reasonForSuspension = TeleoperationSuspensionCase.EmergencyStopActivated;
            SuspendTeleoperation();
        }

        void HeadsetRemoved()
        {
            reasonForSuspension = TeleoperationSuspensionCase.HeadsetRemoved;
            SuspendTeleoperation();
        }

        void SuspendTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnSuspendTeleoperation);
        }

        public void AskForResumingTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnResumeTeleoperation);
        }

        public void AskForStartingArmTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnStartArmTeleoperation);
        }

        public void AskForRobotSmoothlyCompliant()
        {
            EventManager.TriggerEvent(EventNames.OnRobotSmoothlyCompliantRequested);
        }

        void Update()
        {
            if(robotStatus != null && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
                ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();

                NeckJointGoal headTarget = userMovementsInput.GetHeadTarget();

                float pos_left_gripper = userMovementsInput.GetLeftGripperTarget();
                float pos_right_gripper = userMovementsInput.GetRightGripperTarget();

                jointsCommands.SendFullBodyCommands(leftEndEffector, rightEndEffector, headTarget);
                jointsCommands.SendGrippersCommands(pos_left_gripper, pos_right_gripper);
                // robotStatus.LeftGripperClosed(left_gripper_closed);
                // robotStatus.RightGripperClosed(right_gripper_closed);

                Vector3 direction = userMobilityInput.GetTargetDirectionCommand();
                mobilityCommands.SendMobileBaseDirection(direction);
            }
        }
    }
}
