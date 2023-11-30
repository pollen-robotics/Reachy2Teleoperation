using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using Reachy.Kinematics;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Component.Orbita2D;


namespace TeleopReachy
{
    public class UserMovementsInput : MonoBehaviour
    {
        private RobotJointCommands jointsCommands;
        private RobotStatus robotStatus;
        private HeadTracker headTracker;
        private HandsTracker handsTracker;

        private ArmPosition q0_left;
        private ArmPosition q0_right;

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
            q0_left = new ArmPosition
            {
                ShoulderPosition = new Pose2d { Axis1 = (float?)0, Axis2 = (float?)0},
                ElbowPosition = new Pose2d { Axis1 = (float?)0, Axis2 = (float?)-Mathf.PI / 2},
                WristPosition = new Rotation3d { Rpy = new ExtEulerAngles { Roll = 0, Pitch = 0 , Yaw = 0} },
            };

            q0_right = new ArmPosition
            {
                ShoulderPosition = new Pose2d { Axis1 = (float?)0, Axis2 = (float?)0},
                ElbowPosition = new Pose2d { Axis1 = (float?)0, Axis2 = (float?)-Mathf.PI / 2},
                WristPosition = new Rotation3d { Rpy = new ExtEulerAngles { Roll = 0, Pitch = 0 , Yaw = 0} },
            };

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

                // leftEndEffector.Q0 = q0_left;
                // rightEndEffector.Q0 = q0_right;

                NeckGoal headTarget = headTracker.GetHeadTarget();

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
                rightEndEffector = new ArmCartesianGoal { 
                    TargetPosition = handsTracker.rightHand.target_position,
                    TargetOrientation = new Rotation3d { Matrix = handsTracker.rightHand.target_rotation }
                };
            }
            else
            {
                Point rightTargetPos_calibrated = handsTracker.rightHand.target_position;
                rightTargetPos_calibrated.X = rightTargetPos_calibrated.X * reachyArmSize / UserSize.Instance.UserArmSize;
                rightTargetPos_calibrated.Y = (rightTargetPos_calibrated.Y + UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize - reachyShoulderWidth;
                rightTargetPos_calibrated.Z = rightTargetPos_calibrated.Z * reachyArmSize / UserSize.Instance.UserArmSize;

                rightEndEffector = new ArmCartesianGoal { 
                    TargetPosition = rightTargetPos_calibrated,
                    TargetOrientation = new Rotation3d { Matrix = handsTracker.rightHand.target_rotation }
                };
            }

            return rightEndEffector;
        }

        public ArmCartesianGoal GetLeftEndEffectorTarget()
        {
            ArmCartesianGoal leftEndEffector;
            if (UserSize.Instance.UserArmSize == 0)
            {
                leftEndEffector = new ArmCartesianGoal { 
                    TargetPosition = handsTracker.leftHand.target_position,
                    TargetOrientation = new Rotation3d { Matrix = handsTracker.leftHand.target_rotation }
                };
            }
            else
            {
                Point leftTargetPos_calibrated = handsTracker.leftHand.target_position;
                leftTargetPos_calibrated.X = leftTargetPos_calibrated.X * reachyArmSize / UserSize.Instance.UserArmSize;
                leftTargetPos_calibrated.Y = (leftTargetPos_calibrated.Y + UserSize.Instance.UserShoulderWidth) * reachyArmSize / UserSize.Instance.UserArmSize - reachyShoulderWidth;
                leftTargetPos_calibrated.Z = leftTargetPos_calibrated.Z * reachyArmSize / UserSize.Instance.UserArmSize;

                leftEndEffector = new ArmCartesianGoal { 
                    TargetPosition = leftTargetPos_calibrated,
                    TargetOrientation = new Rotation3d { Matrix = handsTracker.leftHand.target_rotation }
                };
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
                    pos_right_gripper = Mathf.Deg2Rad * 20;
                }
                else
                {
                    pos_right_gripper = Mathf.Deg2Rad * -50;
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
                    pos_left_gripper = Mathf.Deg2Rad * -20;
                }
                else
                {
                    pos_left_gripper = Mathf.Deg2Rad * 50;
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