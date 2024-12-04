using System;
using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;

namespace TeleopReachy
{
    public class UserMovementsInput : MonoBehaviour
    {
        private RobotStatus robotStatus;
        private HeadTracker headTracker;
        private HandsTracker handsTracker;

        private bool right_gripper_closed;
        private bool left_gripper_closed;

        private bool reinit_left_gripper;
        private bool reinit_right_gripper;

        private float reachyArmSize = 0.6375f;
        private float reachyShoulderWidth = 0.19f;

        private bool reducedLeftTorque = false;
        private bool reducedRightTorque = false;

        private void OnEnable()
        {
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

        public NeckJointGoal GetHeadTarget()
        {
            if(headTracker != null) return headTracker.GetHeadTarget();
            else return new NeckJointGoal();
        }

        public ArmCartesianGoal GetRightEndEffectorTarget()
        {
            if(handsTracker != null)
            {
                ArmCartesianGoal rightEndEffector;
                if (UserSize.Instance.UserArmSize == 0)
                {
                    Reachy.Kinematics.Matrix4x4 right_target_pos_calibrated = handsTracker.rightHand.target_pos;
                    if (right_target_pos_calibrated.Data[11] < -TableHeight.Instance.Height / 100 && !reducedRightTorque)
                    {
                        reducedRightTorque = true;
                        RobotDataManager.Instance.RobotJointCommands.ModifyRightArmTorqueLimit(30);
                    }
                    if (right_target_pos_calibrated.Data[11] > -TableHeight.Instance.Height / 100 && reducedRightTorque)
                    {
                        reducedRightTorque = false;
                        RobotDataManager.Instance.RobotJointCommands.ModifyRightArmTorqueLimit(100);
                    }
                    if (TableHeight.Instance.SafetyActivated)
                    {
                        right_target_pos_calibrated.Data[11] = Math.Max(right_target_pos_calibrated.Data[11], -TableHeight.Instance.Height / 100);
                    }
                    rightEndEffector = new ArmCartesianGoal { GoalPose = right_target_pos_calibrated };
                }
                else
                {
                    Reachy.Kinematics.Matrix4x4 right_target_pos_calibrated = handsTracker.rightHand.target_pos;
                    right_target_pos_calibrated.Data[3] = right_target_pos_calibrated.Data[3] * reachyArmSize / UserSize.Instance.UserArmSize;
                    right_target_pos_calibrated.Data[7] = (right_target_pos_calibrated.Data[7] + UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize - reachyShoulderWidth;
                    right_target_pos_calibrated.Data[11] = Math.Max(right_target_pos_calibrated.Data[11] * reachyArmSize / UserSize.Instance.UserArmSize, -TableHeight.Instance.Height / 100);

                    rightEndEffector = new ArmCartesianGoal { GoalPose = right_target_pos_calibrated };
                }

                return rightEndEffector;
            }
            else
            {
                return new ArmCartesianGoal();
            }
        }

        public ArmCartesianGoal GetLeftEndEffectorTarget()
        {
            if(handsTracker != null)
            {
                ArmCartesianGoal leftEndEffector;
                if (UserSize.Instance.UserArmSize == 0)
                {
                    Reachy.Kinematics.Matrix4x4 left_target_pos_calibrated = handsTracker.leftHand.target_pos;
                    if (left_target_pos_calibrated.Data[11] < -TableHeight.Instance.Height / 100 && !reducedLeftTorque)
                    {
                        reducedLeftTorque = true;
                        RobotDataManager.Instance.RobotJointCommands.ModifyLeftArmTorqueLimit(30);
                    }
                    if (left_target_pos_calibrated.Data[11] > -TableHeight.Instance.Height / 100 && reducedLeftTorque)
                    {
                        reducedLeftTorque = false;
                        RobotDataManager.Instance.RobotJointCommands.ModifyLeftArmTorqueLimit(100);
                    }
                    if (TableHeight.Instance.SafetyActivated)
                    {
                        left_target_pos_calibrated.Data[11] = Math.Max(left_target_pos_calibrated.Data[11], -TableHeight.Instance.Height / 100);
                    }
                    leftEndEffector = new ArmCartesianGoal { GoalPose = left_target_pos_calibrated };
                }
                else
                {
                    Reachy.Kinematics.Matrix4x4 left_target_pos_calibrated = handsTracker.leftHand.target_pos;
                    left_target_pos_calibrated.Data[3] = left_target_pos_calibrated.Data[3] * reachyArmSize / UserSize.Instance.UserArmSize;
                    left_target_pos_calibrated.Data[7] = (left_target_pos_calibrated.Data[7] - UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize + reachyShoulderWidth;
                    left_target_pos_calibrated.Data[11] = Math.Max(left_target_pos_calibrated.Data[11] * reachyArmSize / UserSize.Instance.UserArmSize, -TableHeight.Instance.Height / 100);

                    leftEndEffector = new ArmCartesianGoal { GoalPose = left_target_pos_calibrated };
                }

                return leftEndEffector;
            }
            else
            {
                return new ArmCartesianGoal();
            }
        }

        void Update()
        {
            if(handsTracker != null)
            {
                // Check Right Gripper State
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

                // Check Left Gripper State
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
            }
        }

        public float GetRightGripperTarget(bool openCloseAnswer=false)
        {
            float pos_right_gripper;
            if (openCloseAnswer)
            {
                if (right_gripper_closed) pos_right_gripper = 0;
                else pos_right_gripper = 1;
            }
            else
            {
                pos_right_gripper = 1 - handsTracker.rightHand.trigger;
            }
            return pos_right_gripper;
        }

        public float GetLeftGripperTarget(bool openCloseAnswer=false)
        {
            float pos_left_gripper;
            if (openCloseAnswer)
            {
                if (left_gripper_closed) pos_left_gripper = 0;
                else pos_left_gripper = 1;
            }
            else
            {
                pos_left_gripper = 1 - handsTracker.leftHand.trigger;
            }
            return pos_left_gripper;
        }
    }
}