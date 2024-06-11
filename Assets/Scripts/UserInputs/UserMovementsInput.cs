using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;

namespace TeleopReachy
{
    public class UserMovementsInput : MonoBehaviour
    {
        private RobotJointCommands jointsCommands;
        private RobotStatus robotStatus;
        private HeadTracker headTracker;
        private HandsTracker handsTracker;

        private bool right_gripper_closed;
        private bool left_gripper_closed;

        private bool reinit_left_gripper;
        private bool reinit_right_gripper;

        private float reachyArmSize = 0.6375f;
        private float reachyShoulderWidth = 0.19f;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        private void OnDisable()
        {
            EventManager.StopListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        private void Init()
        {
            jointsCommands = RobotDataManager.Instance.RobotJointCommands;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            headTracker = UserTrackerManager.Instance.HeadTracker;
            handsTracker = UserTrackerManager.Instance.HandsTracker;
        }

        void Start()
        {
            right_gripper_closed = false;
            left_gripper_closed = false;

            reinit_left_gripper = true;
            reinit_right_gripper = true;
        }

        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive() && !robotStatus.IsRobotCompliant() && !robotStatus.AreRobotMovementsSuspended())
            {
                ArmCartesianGoal leftEndEffector = GetLeftEndEffectorTarget();
                ArmCartesianGoal rightEndEffector = GetRightEndEffectorTarget();

                NeckJointGoal headTarget = headTracker.GetHeadTarget();

                float pos_left_gripper = GetLeftGripperTarget();
                float pos_right_gripper = GetRightGripperTarget();

                jointsCommands.SendFullBodyCommands(leftEndEffector, rightEndEffector, headTarget);
                jointsCommands.SendGrippersCommands(pos_left_gripper, pos_right_gripper);
                robotStatus.LeftGripperClosed(left_gripper_closed);
                robotStatus.RightGripperClosed(right_gripper_closed);
            }
        }

        public ArmCartesianGoal GetRightEndEffectorTarget()
        {
            ArmCartesianGoal rightEndEffector;
            if (UserSize.Instance.UserArmSize == 0)
            {
                rightEndEffector = new ArmCartesianGoal
                {
                    GoalPose = handsTracker.rightHand.target_pos,
                };
            }
            else
            {
                Reachy.Kinematics.Matrix4x4 right_target_pos_calibrated = handsTracker.rightHand.target_pos;
                right_target_pos_calibrated.Data[3] = right_target_pos_calibrated.Data[3] * reachyArmSize / UserSize.Instance.UserArmSize;
                right_target_pos_calibrated.Data[7] = (right_target_pos_calibrated.Data[7] + UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize - reachyShoulderWidth;
                right_target_pos_calibrated.Data[11] = right_target_pos_calibrated.Data[11] * reachyArmSize / UserSize.Instance.UserArmSize;

                rightEndEffector = new ArmCartesianGoal { GoalPose = right_target_pos_calibrated };
            }

            return rightEndEffector;
        }

        public ArmCartesianGoal GetLeftEndEffectorTarget()
        {
            ArmCartesianGoal leftEndEffector;
            if (UserSize.Instance.UserArmSize == 0)
            {
                leftEndEffector = new ArmCartesianGoal
                {
                    GoalPose = handsTracker.leftHand.target_pos,
                };
            }
            else
            {
                Reachy.Kinematics.Matrix4x4 left_target_pos_calibrated = handsTracker.leftHand.target_pos;
                left_target_pos_calibrated.Data[3] = left_target_pos_calibrated.Data[3] * reachyArmSize / UserSize.Instance.UserArmSize;
                left_target_pos_calibrated.Data[7] = (left_target_pos_calibrated.Data[7] - UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize + reachyShoulderWidth;
                left_target_pos_calibrated.Data[11] = left_target_pos_calibrated.Data[11] * reachyArmSize / UserSize.Instance.UserArmSize;

                leftEndEffector = new ArmCartesianGoal { GoalPose = left_target_pos_calibrated };
            }

            return leftEndEffector;
        }

        public float GetRightGripperTarget()
        {
            float pos_right_gripper;

            if (!robotStatus.IsGraspingLockActivated())
            {
                pos_right_gripper = 1 - handsTracker.rightHand.trigger;
                //set correct gripper status 
                if (handsTracker.rightHand.trigger > 0.5)
                    right_gripper_closed = true;
                else
                    right_gripper_closed = false;
            }
            else
            {
                if (handsTracker.rightHand.trigger > 0.9 && reinit_right_gripper)
                {
                    reinit_right_gripper = false;
                    right_gripper_closed = !right_gripper_closed;
                }
                else
                {
                    if (handsTracker.rightHand.trigger < 0.5)
                    {
                        reinit_right_gripper = true;
                    }
                }

                if (right_gripper_closed)
                {
                    pos_right_gripper = 0;
                }
                else
                {
                    pos_right_gripper = 1;
                }
            }

            return pos_right_gripper;
        }

        public float GetLeftGripperTarget()
        {
            float pos_left_gripper;

            if (!robotStatus.IsGraspingLockActivated())
            {
                pos_left_gripper = 1 - handsTracker.leftHand.trigger;
                //set correct gripper status 
                if (handsTracker.leftHand.trigger > 0.5)
                    left_gripper_closed = true;
                else
                    left_gripper_closed = false;
            }
            else
            {
                if (handsTracker.leftHand.trigger > 0.9 && reinit_left_gripper)
                {
                    reinit_left_gripper = false;
                    left_gripper_closed = !left_gripper_closed;
                }
                else
                {
                    if (handsTracker.leftHand.trigger < 0.5)
                    {
                        reinit_left_gripper = true;
                    }
                }

                if (left_gripper_closed)
                {
                    pos_left_gripper = 0;
                }
                else
                {
                    pos_left_gripper = 1;
                }
            }

            return pos_left_gripper;
        }

        public void ForceLeftGripperStatus(bool closed)
        {
            if (handsTracker.leftHand.trigger < 0.5)
                left_gripper_closed = closed;
            reinit_left_gripper = false;
        }

        public void ForceRightGripperStatus(bool closed)
        {
            if (handsTracker.rightHand.trigger < 0.5)
                right_gripper_closed = closed;
            reinit_right_gripper = false;
        }
    }
}