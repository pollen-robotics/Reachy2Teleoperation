using System.Collections;
using System;
using UnityEngine;
using Reachy.Part.Hand;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Kinematics;


namespace TeleopReachy
{
    public class RobotJointCommands : RobotCommands
    {
        private DataMessageManager dataController;
        private ConnectionStatus connectionStatus;

        public Coroutine setSmoothCompliance;
        public Coroutine waitToSetRobotFullSpeed;

        private ArmCartesianGoal lArmZeroPose;
        private ArmCartesianGoal rArmZeroPose;


        // Start is called before the first frame update
        void Start()
        {
            Init();
            dataController = DataMessageManager.Instance;
            connectionStatus = WebRTCManager.Instance.ConnectionStatus;

            robotStatus.event_OnInitializeRobotStateRequested.AddListener(InitializeRobotState);
            robotStatus.event_OnRobotStiffRequested.AddListener(SetRobotStiff);
            robotStatus.event_OnRobotCompliantRequested.AddListener(SetRobotCompliant);
            robotStatus.event_OnRobotSmoothlyCompliantRequested.AddListener(SetRobotSmoothlyCompliant);

            robotStatus.event_OnSuspendTeleoperation.AddListener(SuspendTeleoperation);
            robotStatus.event_OnResumeTeleoperation.AddListener(ResumeTeleoperation);

            robotStatus.event_OnStartArmTeleoperation.AddListener(StartTeleoperation);
            robotStatus.event_OnStopTeleoperation.AddListener(StopTeleoperation);

            robotConfig = RobotDataManager.Instance.RobotConfig;

            setSmoothCompliance = null;
            waitToSetRobotFullSpeed = null;

            Reachy.Kinematics.Matrix4x4 rArmZeroTarget = new Reachy.Kinematics.Matrix4x4
            {
                Data = { 0.966f, 0.198f, -0.166f, 0.048f,
                            -0.135f, 0.934f, 0.330f, -0.356f,
                            0.221f, -0.296f, 0.929f, -0.603f,
                            0, 0, 0, 1 }
            };

            Reachy.Kinematics.Matrix4x4 lArmZeroTarget = new Reachy.Kinematics.Matrix4x4
            {
                Data = { 0.981f, 0.029f, -0.189f, 0.048f,
                            -0.060f, 0.986f, -0.158f, 0.356f,
                            0.181f, 0.166f, 0.969f, -0.603f,
                            0, 0, 0, 1 }
            };

            lArmZeroPose = new ArmCartesianGoal
            {
                GoalPose = lArmZeroTarget
            };
            rArmZeroPose = new ArmCartesianGoal
            {
                GoalPose = rArmZeroTarget
            };
        }

        void OnDestroy()
        {
            robotStatus.SetLeftArmOn(false);
            robotStatus.SetRightArmOn(false);
            robotStatus.SetHeadOn(false);
            if (!robotConfig.IsVirtual())
                SetRobotCompliant();
        }

        protected override void ActualSendGrippersCommands(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand)
        {
            if (robotStatus.IsRobotArmTeleoperationActive())
            {
                if (robotConfig.HasLeftGripper() && robotStatus.IsLeftArmOn()) dataController.SetHandPosition(leftGripperCommand);
                if (robotConfig.HasRightGripper() && robotStatus.IsRightArmOn()) dataController.SetHandPosition(rightGripperCommand);
            }
        }

        protected override void ActualSendBodyCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest, NeckJointGoal neckRequest)
        {
            if (robotStatus.IsRobotArmTeleoperationActive())
            {
                if (robotConfig.HasLeftArm() && robotStatus.IsLeftArmOn()) dataController.SendArmCommand(leftArmRequest);
                if (robotConfig.HasRightArm() && robotStatus.IsRightArmOn()) dataController.SendArmCommand(rightArmRequest);
            }
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) dataController.SendNeckCommand(neckRequest);
        }

        private void SetRobotSmoothlyCompliant()
        {
            Debug.Log("[RobotJointCommands]: SetRobotSmoothlyCompliant");
            setSmoothCompliance = StartCoroutine(SmoothCompliance(2));
        }

        private void SetRobotStiff()
        {
            ToggleStiffness();
            robotStatus.SetRobotCompliant(false);
        }

        private void ToggleStiffness()
        {
            if (robotConfig.HasLeftArm())
            {
                if (robotStatus.IsLeftArmOn())
                    SetRobotStiff("l_arm");
                else
                    SetRobotCompliant("l_arm");
            }
            if (robotConfig.HasRightArm())
            {
                if (robotStatus.IsRightArmOn())
                    SetRobotStiff("r_arm");
                else
                    SetRobotCompliant("r_arm");
            }
            if (robotConfig.HasHead())
            {
                if (robotStatus.IsHeadOn())
                    SetRobotStiff("head");
                else
                    SetRobotCompliant("head");
            }
        }

        //partName should be l_, r_ or neck_
        private void SetRobotStiff(string partName = "")
        {
            if (setSmoothCompliance != null)
            {
                StopCoroutine(setSmoothCompliance);
            }

            Debug.Log("[RobotJointCommands] SetRobotStiff " + partName);
            if (partName == "")
            {
                if (robotConfig.HasLeftArm())
                {
                    dataController.TurnArmOn(robotConfig.partsId["l_arm"]);
                }
                if (robotConfig.HasLeftGripper())
                {
                    dataController.TurnArmOn(robotConfig.partsId["l_hand"]);
                }
                if (robotConfig.HasRightArm())
                {
                    dataController.TurnArmOn(robotConfig.partsId["r_arm"]);
                }
                if (robotConfig.HasRightGripper())
                {
                    dataController.TurnArmOn(robotConfig.partsId["r_hand"]);
                }
                if (robotConfig.HasHead())
                {
                    dataController.TurnHeadOn(robotConfig.partsId["head"]);
                }
            }
            else
            {
                if (partName == "head")
                {
                    dataController.TurnHeadOn(robotConfig.partsId["head"]);
                }
                else
                {
                    if (partName.Contains("arm"))
                    {
                        dataController.TurnArmOn(robotConfig.partsId[partName]);
                    }
                    else if (partName.Contains("hand"))
                    {
                        dataController.TurnHandOn(robotConfig.partsId[partName]);
                    }
                }
            }
        }

        private void SetRobotCompliant()
        {
            ToggleStiffness();
            robotStatus.SetRobotCompliant(true);
        }

        //partName should be l_, r_ or neck_
        private void SetRobotCompliant(string partName = "")
        {
            Debug.Log("[RobotJointCommands] SetRobotCompliant " + partName);
            if (partName == "")
            {
                if (robotConfig.HasLeftArm())
                {
                    dataController.TurnArmOff(robotConfig.partsId["l_arm"]);
                }
                if (robotConfig.HasLeftGripper())
                {
                    dataController.TurnHandOff(robotConfig.partsId["l_hand"]);
                }
                if (robotConfig.HasRightArm())
                {
                    dataController.TurnArmOff(robotConfig.partsId["r_arm"]);
                }
                if (robotConfig.HasRightGripper())
                {
                    dataController.TurnHandOff(robotConfig.partsId["r_hand"]);
                }
                if (robotConfig.HasHead())
                {
                    dataController.TurnHeadOff(robotConfig.partsId["head"]);
                }
            }

            else
            {
                if (partName == "head")
                {
                    dataController.TurnHeadOff(robotConfig.partsId["head"]);
                }
                else
                {
                    if (partName.Contains("arm"))
                    {
                        dataController.TurnArmOff(robotConfig.partsId[partName]);
                    }
                    else if (partName.Contains("hand"))
                    {
                        dataController.TurnHandOff(robotConfig.partsId[partName]);
                    }
                }
            }
        }

        private void StartTeleoperation()
        {
            Debug.Log("[RobotJointCommands]: StartArmTeleoperation");
            waitToSetRobotFullSpeed = StartCoroutine(ResetReachyMotorsFullSpeed());
        }

        private void StopTeleoperation()
        {
            Debug.Log("[RobotJointCommands]: StopTeleoperation");
            if (connectionStatus.IsServerConnected())
            {
                AskForCancellationCurrentMovementsPlaying();
                if (waitToSetRobotFullSpeed != null)
                {
                    StopCoroutine(waitToSetRobotFullSpeed);
                }
                if (!robotStatus.IsRobotPositionLocked) SetRobotSmoothlyCompliant();
                ResetMotorsStartingSpeed();
            }
        }

        private void InitializeRobotState()
        {
            Debug.Log("[RobotJointCommands]: InitializeRobotState");
            StartCoroutine(ResetTorqueMax());
            ResetMotorsStartingSpeed();
        }

        IEnumerator ResetTorqueMax()
        {
            yield return new WaitForSeconds(1);
            if (setSmoothCompliance != null) yield return setSmoothCompliance;

            ModifyArmTorqueLimit(100);
            ModifyHeadTorqueLimit(100);
        }

        private void ResetMotorsStartingSpeed()
        {
            Debug.Log("[RobotJointCommands] ResetMotorsStartingSpeed");
            robotStatus.SetMotorsSpeedLimited(true);
            uint speedLimit = 10;
            ModifyArmSpeedLimit(speedLimit);
            ModifyHeadSpeedLimit(100);
        }

        private void SetHeadLookingStraight()
        {
            Reachy.Kinematics.Quaternion unitQ = new Reachy.Kinematics.Quaternion
            {
                W = 1,
                X = 0,
                Y = 0,
                Z = 0,
            };

            if (robotConfig.HasHead() && robotStatus.IsHeadOn())
            {

                NeckJointGoal neckGoal = new NeckJointGoal { Id = robotConfig.partsId["head"], JointsGoal = new NeckOrientation { Rotation = new Rotation3d { Q = unitQ } } };
                dataController.SendNeckCommand(neckGoal);
            }
        }

        private IEnumerator SmoothCompliance(int duration)
        {
            uint torqueLimitLow = 35;
            uint torqueLimitHigh = 100;

            ModifyArmTorqueLimit(torqueLimitLow);

            SetHeadLookingStraight();
            SendArmsToZeroPose();

            int countingTime = 0;
            while (countingTime <= duration)
            {
                yield return new WaitForSeconds(1);
                torqueLimitLow -= 10;
                ModifyArmTorqueLimit(torqueLimitLow);
                countingTime += 1;
            }

            SetRobotCompliant("head");
            SetRobotCompliant("l_arm");
            SetRobotCompliant("l_hand");
            SetRobotCompliant("r_arm");
            SetRobotCompliant("r_hand");
            robotStatus.SetRobotCompliant(true);

            yield return new WaitForSeconds(0.2f);

            ModifyArmTorqueLimit(torqueLimitHigh);

            yield return new WaitForSeconds(0.1f);
        }

        private void ModifyArmTorqueLimit(uint torqueLimit)
        {
            if (robotConfig.HasLeftArm() && robotStatus.IsLeftArmOn())
            {
                Reachy.Part.Arm.TorqueLimitRequest torqueRequest = new Reachy.Part.Arm.TorqueLimitRequest
                {
                    Id = robotConfig.partsId["l_arm"],
                    Limit = torqueLimit,
                };
                dataController.SetArmTorqueLimit(torqueRequest);
            }
            if (robotConfig.HasRightArm() && robotStatus.IsRightArmOn())
            {
                Reachy.Part.Arm.TorqueLimitRequest torqueRequest = new Reachy.Part.Arm.TorqueLimitRequest
                {
                    Id = robotConfig.partsId["r_arm"],
                    Limit = torqueLimit,
                };
                dataController.SetArmTorqueLimit(torqueRequest);
            }
        }

        private void ModifyArmSpeedLimit(uint speedLimit)
        {
            if (robotConfig.HasLeftArm() && robotStatus.IsLeftArmOn())
            {
                Reachy.Part.Arm.SpeedLimitRequest speedRequest = new Reachy.Part.Arm.SpeedLimitRequest
                {
                    Id = robotConfig.partsId["l_arm"],
                    Limit = speedLimit,
                };
                dataController.SetArmSpeedLimit(speedRequest);
            }
            if (robotConfig.HasRightArm() && robotStatus.IsRightArmOn())
            {
                Reachy.Part.Arm.SpeedLimitRequest speedRequest = new Reachy.Part.Arm.SpeedLimitRequest
                {
                    Id = robotConfig.partsId["r_arm"],
                    Limit = speedLimit,
                };
                dataController.SetArmSpeedLimit(speedRequest);
            }
        }

        private void ModifyHeadSpeedLimit(uint speedLimit)
        {
            if (robotConfig.HasHead() && robotStatus.IsHeadOn())
            {
                Reachy.Part.Head.SpeedLimitRequest headSpeed = new Reachy.Part.Head.SpeedLimitRequest
                {
                    Limit = speedLimit,
                    Id = robotConfig.partsId["head"]
                };
                dataController.SetHeadSpeedLimit(headSpeed);
            }
        }

        private void ModifyHeadTorqueLimit(uint torqueLimit)
        {
            if (robotConfig.HasHead() && robotStatus.IsHeadOn())
            {
                Reachy.Part.Head.TorqueLimitRequest headTorque = new Reachy.Part.Head.TorqueLimitRequest
                {
                    Limit = torqueLimit,
                    Id = robotConfig.partsId["head"]
                };
                dataController.SetHeadTorqueLimit(headTorque);
            }
        }

        private void SendArmsToZeroPose()
        {
            if (robotConfig.HasLeftArm() && robotStatus.IsLeftArmOn())
            {
                lArmZeroPose.Id = robotConfig.partsId["l_arm"];
                dataController.SendArmCommand(lArmZeroPose);
            }
            if (robotConfig.HasRightArm() && robotStatus.IsRightArmOn())
            {
                rArmZeroPose.Id = robotConfig.partsId["r_arm"];
                dataController.SendArmCommand(rArmZeroPose);
            }
        }

        private IEnumerator ResetReachyMotorsFullSpeed()
        {
            Debug.Log("[RobotJointCommands]: ResetReachyMotorsFullSpeed");
            yield return new WaitForSeconds(3);

            uint speedLimit = 100;
            ModifyArmSpeedLimit(speedLimit);
            robotStatus.SetMotorsSpeedLimited(false);
        }

        void SuspendTeleoperation()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                try
                {
                    if (waitToSetRobotFullSpeed != null)
                    {
                        StopCoroutine(waitToSetRobotFullSpeed);
                    }
                    ResetMotorsStartingSpeed();
                    if (setSmoothCompliance != null) StopCoroutine(setSmoothCompliance);
                    setSmoothCompliance = StartCoroutine(SmoothCompliance(5));
                    if (robotStatus.IsHeadOn()) SetHeadLookingStraight();
                }
                catch (Exception exc)
                {
                    Debug.Log($"[RobotJointCommands]: SuspendTeleoperation error: {exc}");
                }
            }
        }

        void ResumeTeleoperation()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                if (!robotStatus.HasMotorsSpeedLimited())
                {
                    ResetMotorsStartingSpeed();
                }
            }
        }
    }
}
