using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Kinematics;
using System;
using System.Threading;

namespace TeleopReachy
{
    public class FakeUserMovementInput : MonoBehaviour
    {
        private RobotJointCommands jointsCommands;
        private RobotStatus robotStatus;
        private HeadTracker headTracker;
        private HandsTracker handsTracker;
        protected RobotConfig robotConfig;

        private bool right_gripper_closed;
        private bool left_gripper_closed;

        private bool reinit_left_gripper;
        private bool reinit_right_gripper;

        private float reachyArmSize = 0.6375f;
        private float reachyShoulderWidth = 0.19f;

        private float radius = 0.2f;
        private float fixed_x = 0.4f;
        private float center_y = 0;
        private float center_z = 0.1f;
        private int num_steps = 200;
        private int step = 0;
        private int circle_period = 3;
        private float t0 = 0;
        private bool need_start_teleop = false;

        private void Init()
        {
            jointsCommands = RobotDataManager.Instance.RobotJointCommands;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            //headTracker = UserTrackerManager.Instance.HeadTracker;
            // handsTracker = UserTrackerManager.Instance.HandsTracker;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(OnConfigChanged);
        }

        void Start()
        {
            Init();

            right_gripper_closed = false;
            left_gripper_closed = false;

            reinit_left_gripper = true;
            reinit_right_gripper = true;

            radius = 0.2f;
            fixed_x = 0.4f;
            center_y = 0;
            center_z = 0.1f;
            num_steps = 200;
            step = 0;
            circle_period = 3;
            t0 = Time.time;

            //StartCoroutine(LateInit());
        }

        private IEnumerator LateInit()
        {
            while (robotStatus.HasMotorsSpeedLimited())
                yield return new WaitForSeconds(0.2f);
            //Debug.Log("Robot config test " + robotConfig + " " + robotStatus);
            /*robotStatus.InitializeRobotState();
            robotStatus.SetHeadOn(true);
            robotStatus.SetRightArmOn(true);
            robotStatus.SetLeftArmOn(true);
            robotStatus.StartRobotTeleoperation();
            robotStatus.StartArmTeleoperation();*/
            yield return new WaitForSeconds(3);
            robotStatus.TurnRobotStiff();
            //jointsCommands.SetRobotStiff();
            //yield return null;

        }

        void OnConfigChanged()
        {
            Debug.Log("Robot config changed " + robotConfig + " " + robotStatus);
            //robotStatus.InitializeRobotState();
            //robotStatus.TurnRobotStiff();
            robotStatus.SetHeadOn(true);
            robotStatus.SetRightArmOn(true);
            robotStatus.SetLeftArmOn(true);
            robotStatus.StartRobotTeleoperation();
            robotStatus.SetMobilityActive(true);
            //robotStatus.StartArmTeleoperation();
            need_start_teleop = true;
        }

        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive() && !robotStatus.IsRobotCompliant() && !robotStatus.AreRobotMovementsSuspended())
            {
                //float angle = 2 * np.pi * (step / num_steps);
                float angle = 2 * Mathf.PI * (Time.time - t0) / circle_period;
                //Debug.Log("Angle " + angle);
                step += 1;
                if (step >= num_steps)
                    step = 0;
                float y = center_y + radius * Mathf.Cos(angle);
                float z = center_z + radius * Mathf.Sin(angle);

                ArmCartesianGoal leftEndEffector = GetLeftEndEffectorTarget(fixed_x, y + 0.2f, z);
                ArmCartesianGoal rightEndEffector = GetRightEndEffectorTarget(fixed_x, y - 0.2f, z);

                UnityEngine.Quaternion headQuat = UnityEngine.Quaternion.Euler(0, angle * 2, 0);
                NeckJointGoal headTarget = new NeckJointGoal
                {
                    JointsGoal = new NeckOrientation
                    {
                        Rotation = new Rotation3d
                        {
                            Q = new Reachy.Kinematics.Quaternion
                            {
                                W = headQuat.w,
                                X = -headQuat.z,
                                Y = headQuat.x,
                                Z = -headQuat.y,
                                /*W = 1,
                                X = 0,
                                Y = 0,
                                Z = 0,*/
                            }
                        }
                    }
                };

                //NeckJointGoal headTarget = headTracker.GetHeadTarget();

                //float pos_left_gripper = GetLeftGripperTarget();
                //float pos_right_gripper = GetRightGripperTarget();

                //jointsCommands.SendFullBodyCommands(leftEndEffector, rightEndEffector, headTarget);
                //jointsCommands.SendGrippersCommands(pos_left_gripper, pos_right_gripper);
                //robotStatus.LeftGripperClosed(left_gripper_closed);
                //robotStatus.RightGripperClosed(right_gripper_closed);
            }
            if (need_start_teleop)
            {
                Debug.Log("Fabien config");
                robotStatus.InitializeRobotState();
                //robotStatus.TurnRobotStiff();
                robotStatus.StartArmTeleoperation();

                StartCoroutine(LateInit());
                need_start_teleop = false;
                Debug.Log("Robot config changed 2 " + robotConfig + " " + robotStatus);
            }
        }

        public ArmCartesianGoal GetRightEndEffectorTarget(float x, float y, float z)
        {
            ArmCartesianGoal rightEndEffector;
            Reachy.Kinematics.Matrix4x4 rArmTarget = new Reachy.Kinematics.Matrix4x4
            {
                Data = { 0, 0, 1, x,
                         0, 1, 0, y,
                         1, 0, 0, z,
                         0, 0, 0, 1 }
            };

            rightEndEffector = new ArmCartesianGoal { GoalPose = rArmTarget };

            return rightEndEffector;
        }

        public ArmCartesianGoal GetLeftEndEffectorTarget(float x, float y, float z)
        {
            ArmCartesianGoal leftEndEffector;
            Reachy.Kinematics.Matrix4x4 lArmTarget = new Reachy.Kinematics.Matrix4x4
            {
                Data = { 0, 0, 1, x,
                         0, 1, 0, y,
                         1, 0, 0, z,
                         0, 0, 0, 1 }
            };
            leftEndEffector = new ArmCartesianGoal { GoalPose = lArmTarget };


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
