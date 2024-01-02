using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Grpc.Core;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy.Part.Hand;


namespace TeleopReachy
{
    public abstract class RobotCommands : MonoBehaviour
    {
        public UnityEvent<Emotion> event_OnEmotionOver;

        // Token to cancel emotions
        protected CancellationTokenSource askForCancellation = new CancellationTokenSource();

        protected RobotConfig robotConfig;
        protected RobotStatus robotStatus;

        protected void Init()
        {
            robotConfig = RobotDataManager.Instance.RobotConfig;

            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        protected void AskForCancellationCurrentMovementsPlaying()
        {
            askForCancellation.Cancel();
            StartCoroutine(DisposeToken());
        }

        IEnumerator DisposeToken()
        {
            yield return new WaitForSeconds(0.1f);

            askForCancellation.Dispose();
            askForCancellation = new CancellationTokenSource();
        }

        // protected abstract void SendJointsCommands(JointsCommand jointsCommand);
        protected abstract void ActualSendGrippersCommands(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand);
        protected abstract void ActualSendBodyCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest, NeckJointGoal neckRequest);

        public void SendFullBodyCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest, NeckJointGoal neckRequest)
        {
            leftArmRequest.Id = robotConfig.partsId["l_arm"];
            rightArmRequest.Id = robotConfig.partsId["r_arm"];
            neckRequest.Id = robotConfig.partsId["head"];
            // FullBodyCartesianCommand bodyCommand = new FullBodyCartesianCommand();
            // if (robotConfig.IsVirtual() || (robotConfig.HasLeftArm() && robotStatus.IsLeftArmOn()))
            // {
            //     bodyCommand.LeftArm = leftArmRequest;
            // }
            // if (robotConfig.IsVirtual() || (robotConfig.HasRightArm() && robotStatus.IsRightArmOn()))
            // {
            //     bodyCommand.RightArm = rightArmRequest;
            // }
            // if (robotConfig.IsVirtual() || robotConfig.HasHead() && robotStatus.IsHeadOn())
            // {
            //     bodyCommand.Head = neckRequest;
            // }

            ActualSendBodyCommands(leftArmRequest, rightArmRequest, neckRequest);
        }

        public void SendGrippersCommands(float leftGripperOpening, float rightGripperOpening)
        {
            // List<JointCommand> grippersCommand = new List<JointCommand>();

            // if (robotConfig.IsVirtual() || robotConfig.HasLeftGripper() && robotStatus.IsLeftArmOn())
            // {
            //     var jointCom = new JointCommand();
            //     jointCom.Id = new JointId { Name = "l_gripper" };
            //     jointCom.GoalPosition = leftGripperOpening;

            //     grippersCommand.Add(jointCom);
            // }

            // if (robotConfig.IsVirtual() || robotConfig.HasRightGripper() && robotStatus.IsRightArmOn())
            // {
            //     var jointCom = new JointCommand();
            //     jointCom.Id = new JointId { Name = "r_gripper" };
            //     jointCom.GoalPosition = rightGripperOpening;

            //     grippersCommand.Add(jointCom);
            // }

            // JointsCommand gripperCommand = new JointsCommand
            // {
            //     Commands = { grippersCommand },
            // };

            HandPositionRequest leftHandPositionRequest = new HandPositionRequest();
            leftHandPositionRequest.Id = robotConfig.partsId["l_hand"];
            leftHandPositionRequest.Position = new HandPosition { ParallelGripper=new ParallelGripperPosition { Position=leftGripperOpening }};

            HandPositionRequest rightHandPositionRequest = new HandPositionRequest();
            rightHandPositionRequest.Id = robotConfig.partsId["r_hand"];
            rightHandPositionRequest.Position = new HandPosition { ParallelGripper=new ParallelGripperPosition { Position=rightGripperOpening }};

            ActualSendGrippersCommands(leftHandPositionRequest, rightHandPositionRequest);
        }

        public async void ReachySad()
        {
            Debug.Log("Simulated Reachy is sad");
            CancellationToken cancellationToken = askForCancellation.Token;
            await Task.Delay(100);


            // JointsCommand antennasSpeedLimit = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 1.5f},
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 1.5f },
            //         }
            // };
            // JointsCommand antennasSpeedLimit2 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 0.7f},
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 0.7f },
            //         }
            // };
            // JointsCommand antennasCommand1 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(140) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-140) },
            //         }
            // };
            // JointsCommand antennasCommand2 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(120) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-120) },
            //         }
            // };

            // JointsCommand antennasCommandBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         }
            // };

            // try
            // {
            //     SendJointsCommands(antennasSpeedLimit);
            //     SendJointsCommands(antennasCommand1);
            //     await Task.Delay(2000);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     SendJointsCommands(antennasSpeedLimit2);
            //     SendJointsCommands(antennasCommand2);
            //     await Task.Delay(600);
            //     SendJointsCommands(antennasCommand1);
            //     await Task.Delay(600);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     SendJointsCommands(antennasCommand2);
            //     await Task.Delay(600);
            //     SendJointsCommands(antennasCommand1);
            //     await Task.Delay(1000);
            //     SendJointsCommands(antennasSpeedLimit);
            //     SendJointsCommands(antennasCommandBack);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     event_OnEmotionOver.Invoke(Emotion.Sad);
            // }
            // catch (OperationCanceledException e)
            // {
            //     Debug.Log("Reachy sad has been canceled: " + e);
            //     event_OnEmotionOver.Invoke(Emotion.Sad);
            // }
        }
        public async void ReachyHappy()
        {
            Debug.Log("Simulated Reachy is happy");
            CancellationToken cancellationToken = askForCancellation.Token;

            await Task.Delay(100);

            // JointsCommand antennasCommand1 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(10) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-10) },
            //         }
            // };
            // JointsCommand antennasCommand2 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(-10) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(10) },
            //         }
            // };

            // JointsCommand antennasCommandBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         }
            // };
            // JointsCommand antennasSpeedBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 0 },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 0 },
            //         }
            // };

            // try
            // {
            //     SendJointsCommands(antennasSpeedBack);
            //     for (int i = 0; i < 9; i++)
            //     {
            //         SendJointsCommands(antennasCommand1);
            //         await Task.Delay(100);
            //         SendJointsCommands(antennasCommand2);
            //         await Task.Delay(100);
            //         cancellationToken.ThrowIfCancellationRequested();
            //     }

            //     await Task.Delay(200);
            //     SendJointsCommands(antennasCommandBack);
            //     SendJointsCommands(antennasSpeedBack);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     event_OnEmotionOver.Invoke(Emotion.Happy);
            // }
            // catch (OperationCanceledException e)
            // {
            //     Debug.Log("Reachy happy has been canceled: " + e);
            //     event_OnEmotionOver.Invoke(Emotion.Happy);
            // }
        }
        public async void ReachyAngry()
        {
            Debug.Log("Reachy is angry");
            CancellationToken cancellationToken = askForCancellation.Token;
            await Task.Delay(100);

            // JointsCommand antennasSpeedLimit1 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 5f},
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 5f },
            //         }
            // };
            // JointsCommand antennasSpeedLimit2 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna"  }, SpeedLimit = 2.3f},
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 2.3f },
            //         }
            // };
            // JointsCommand antennasCommand1 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(80) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-80) },
            //         }
            // };
            // JointsCommand antennasCommand2 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(40) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-40) },
            //         }
            // };
            // JointsCommand antennasCommandBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         }
            // };
            // JointsCommand antennasSpeedBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 0 },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 0 },
            //         }
            // };

            // try
            // {
            //     for (int i = 0; i < 2; i++)
            //     {
            //         SendJointsCommands(antennasSpeedBack);
            //         SendJointsCommands(antennasCommand1);
            //         await Task.Delay(1000);
            //         cancellationToken.ThrowIfCancellationRequested();
            //         SendJointsCommands(antennasSpeedLimit2);
            //         SendJointsCommands(antennasCommand2);
            //         await Task.Delay(500);
            //         cancellationToken.ThrowIfCancellationRequested();
            //     }

            //     SendJointsCommands(antennasSpeedBack);
            //     SendJointsCommands(antennasCommand1);
            //     await Task.Delay(1500);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     SendJointsCommands(antennasSpeedLimit2);

            //     SendJointsCommands(antennasCommandBack);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     event_OnEmotionOver.Invoke(Emotion.Angry);
            // }
            // catch (OperationCanceledException e)
            // {
            //     Debug.Log("Reachy angry has been canceled: " + e);
            //     event_OnEmotionOver.Invoke(Emotion.Angry);
            // }
        }
        public async void ReachyConfused()
        {
            Debug.Log("Reachy is confused");
            CancellationToken cancellationToken = askForCancellation.Token;
            await Task.Delay(100);


            // JointsCommand antennasSpeedLimit = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 2.3f},
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 2.3f },
            //         }
            // };
            // JointsCommand antennasCommand1 = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(-20) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(-80) },
            //         }
            // };
            // JointsCommand antennasCommandBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, GoalPosition=Mathf.Deg2Rad*(0) },
            //         }
            // };
            // JointsCommand antennasSpeedBack = new JointsCommand
            // {
            //     Commands = {
            //         new JointCommand { Id=new JointId { Name = "l_antenna" }, SpeedLimit = 0 },
            //         new JointCommand { Id=new JointId { Name = "r_antenna" }, SpeedLimit = 0 },
            //         }
            // };

            // try
            // {
            //     SendJointsCommands(antennasSpeedLimit);
            //     SendJointsCommands(antennasCommand1);
            //     await Task.Delay(2000);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     SendJointsCommands(antennasCommandBack);
            //     cancellationToken.ThrowIfCancellationRequested();
            //     event_OnEmotionOver.Invoke(Emotion.Confused);
            // }
            // catch (OperationCanceledException e)
            // {
            //     Debug.Log("Reachy confused has been canceled: " + e);
            //     event_OnEmotionOver.Invoke(Emotion.Confused);
            // }
        }
    }
}