using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Reachy.Part.Arm;
using Reachy.Part.Head;

namespace TeleopReachy
{
    public class TeleoperationManager : Singleton<TeleoperationManager>
    {
        private RobotConfig robotConfig;
        private RobotStatus robotStatus;
        private RobotJointCommands jointsCommands;
        private RobotMobilityCommands mobilityCommands;

        private UserMovementsInput userMovementsInput;
        private UserMobilityInput userMobilityInput;
        private UserEmotionInput userEmotionInput;

        private enum JoystickMode
        {
            Emotion, Mobility
        }
        private JoystickMode joystickMode;

        public bool IsRobotTeleoperationActive { get; private set; }
        public bool IsArmTeleoperationActive { get; private set; }
        public bool IsMobileBaseTeleoperationActive { get; private set; }

        public enum TeleoperationSuspensionCase 
        {
            None, HeadsetRemoved, EmergencyStopActivated,
        }

        public TeleoperationSuspensionCase reasonForSuspension { get; private set; }

        public UnityEvent event_OnTriedToSendMobilityCommands;

        void Start()
        {
            IsRobotTeleoperationActive = false;
            IsArmTeleoperationActive = false;
            IsMobileBaseTeleoperationActive = false;
            joystickMode = JoystickMode.Mobility;

            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, StartTeleoperation);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, InitUserInputs);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);
            EventManager.StartListening(EventNames.OnEmergencyStop, EmergencyStopActivated);

            EventManager.StartListening(EventNames.OnStartArmTeleoperation, StartArmTeleoperation);
            EventManager.StartListening(EventNames.OnStartMobileBaseTeleoperation, StartMobileBaseTeleoperation);
            EventManager.StartListening(EventNames.OnStopMobileBaseTeleoperation, StopMobileBaseTeleoperation);
            EventManager.StartListening(EventNames.OnEmotionMode, ActivateEmotionMode);
            EventManager.StartListening(EventNames.OnMobilityMode, ActivateMobilityMode);

            EventManager.StartListening(EventNames.RobotDataSceneLoaded, InitRobotData);
        }

        void InitUserInputs()
        {
            userMovementsInput = UserInputManager.Instance.UserMovementsInput;
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;
            userEmotionInput = UserInputManager.Instance.UserEmotionInput;
        }

        void InitRobotData()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            jointsCommands = RobotDataManager.Instance.RobotJointCommands;
            mobilityCommands = RobotDataManager.Instance.RobotMobilityCommands;
        }

        private void StartArmTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: Start arm teleoperation");
            IsArmTeleoperationActive = true;
        }

        private void StartMobileBaseTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: Start mobile base teleoperation");
            userMobilityInput.event_OnStartMoving.AddListener(ActivateOnNewMobilityMovement);
            if (!robotStatus.IsMobileBaseOn()) userMobilityInput.event_OnStartMoving.AddListener(FailToSendMobilityCommand);
        }

        private void ActivateOnNewMobilityMovement()
        {
            userMobilityInput.event_OnStartMoving.RemoveListener(ActivateOnNewMobilityMovement);
            IsMobileBaseTeleoperationActive = true;
        }

        private void StopMobileBaseTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: Stop mobile base teleoperation");
            IsMobileBaseTeleoperationActive = false;
            if (!robotStatus.IsMobileBaseOn()) userMobilityInput.event_OnStartMoving.RemoveListener(FailToSendMobilityCommand);
            userMobilityInput.event_OnStartMoving.RemoveListener(ActivateOnNewMobilityMovement);
        }

        private void FailToSendMobilityCommand()
        {
            if (!robotStatus.IsMobileBaseOn())
            {
                event_OnTriedToSendMobilityCommands.Invoke();
            }
        }

        private void ActivateEmotionMode()
        {
            joystickMode = JoystickMode.Emotion;
            userEmotionInput.event_OnEmotionSelected.AddListener(PlayEmotion);
            StopMobileBaseTeleoperation();
        }

        private void PlayEmotion(Emotion emotion)
        {
            if(robotStatus != null && IsRobotTeleoperationActive && !robotStatus.AreRobotMovementsSuspended())
            {
                switch (emotion)
                {
                    case Emotion.Sad:
                        jointsCommands.ReachySad();
                        break;
                    case Emotion.Happy:
                        jointsCommands.ReachyHappy();
                        break;
                    case Emotion.Confused:
                        jointsCommands.ReachyConfused();
                        break;
                    case Emotion.Angry:
                        jointsCommands.ReachyAngry();
                        break;
                }
            }
        }

        private void ActivateMobilityMode()
        {
            joystickMode = JoystickMode.Mobility;
            userEmotionInput.event_OnEmotionSelected.RemoveListener(PlayEmotion);
            StartMobileBaseTeleoperation();
        }

        void StartTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: Start teleoperation");
            EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);
            EventManager.TriggerEvent(EventNames.OnStartTeleoperation);
            if (!robotStatus.IsMobileBaseOn()) userMobilityInput.event_OnStartMoving.AddListener(FailToSendMobilityCommand);
            IsRobotTeleoperationActive = true;
            IsMobileBaseTeleoperationActive = true;
        }
        
        void StopTeleoperation()
        {
            Debug.Log("[TeleoperationManager]: Stop teleoperation");
            IsRobotTeleoperationActive = false;
            IsArmTeleoperationActive = false;
            IsMobileBaseTeleoperationActive = false;
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
            if(IsRobotTeleoperationActive) EventManager.TriggerEvent(EventNames.OnSuspendTeleoperation);
        }

        public void AskForResumingTeleoperation()
        {
            if (IsRobotTeleoperationActive && robotStatus.AreRobotMovementsSuspended())
            {
                EventManager.TriggerEvent(EventNames.OnResumeTeleoperation);
            }
        }

        public void AskForStartingArmTeleoperation()
        {
            if (IsRobotTeleoperationActive && !robotStatus.AreRobotMovementsSuspended())
            {
                EventManager.TriggerEvent(EventNames.OnStartArmTeleoperation);
            }
        }

        public void AskForRobotSmoothlyCompliant()
        {
            EventManager.TriggerEvent(EventNames.OnRobotSmoothlyCompliantRequested);
        }

        void Update()
        {
            if(robotStatus != null && IsRobotTeleoperationActive && !robotStatus.AreRobotMovementsSuspended())
            {
                NeckJointGoal headTarget = userMovementsInput.GetHeadTarget();
                jointsCommands.SendNeckCommands(headTarget);

                if (IsArmTeleoperationActive)
                {
                    ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
                    ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();

                    float pos_left_gripper = userMovementsInput.GetLeftGripperTarget(robotStatus.IsGraspingLockActivated());
                    float pos_right_gripper = userMovementsInput.GetRightGripperTarget(robotStatus.IsGraspingLockActivated());

                    jointsCommands.SendArmsCommands(leftEndEffector, rightEndEffector);
                    jointsCommands.SendGrippersCommands(pos_left_gripper, pos_right_gripper);
                }

                if (IsMobileBaseTeleoperationActive)
                {
                    Vector3 direction = userMobilityInput.GetTargetDirectionCommand();
                    mobilityCommands.SendMobileBaseDirection(direction);
                }
            }
        }
    }
}
