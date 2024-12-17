using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy.Part.Hand;
using Component.DynamixelMotor;


namespace TeleopReachy
{
    public abstract class RobotCommands : MonoBehaviour
    {
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

        protected abstract void ActualSendGrippersCommands(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand);
        protected abstract void ActualSendArmsCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest);
        protected abstract void ActualSendNeckCommands(NeckJointGoal neckRequest);
        protected abstract void ActualSendAntennasCommands(DynamixelMotorsCommand antennasRequest);
        
        public void SendArmsCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest)
        {
            if (robotConfig.HasLeftArm())
            {
                leftArmRequest.Id = robotConfig.partsId["l_arm"];
                leftArmRequest.ConstrainedMode = robotStatus.GetIKMode();
            }
            if (robotConfig.HasRightArm())
            {
                rightArmRequest.Id = robotConfig.partsId["r_arm"];
                rightArmRequest.ConstrainedMode = robotStatus.GetIKMode();
            }
            ActualSendArmsCommands(leftArmRequest, rightArmRequest);
        }

        public void SendNeckCommands(NeckJointGoal neckRequest)
        {
            if (robotConfig.HasHead()) neckRequest.Id = robotConfig.partsId["head"];
            ActualSendNeckCommands(neckRequest);
        }

        public void SendGrippersCommands(float leftGripperOpening, float rightGripperOpening)
        {
            HandPositionRequest leftHandPositionRequest = new HandPositionRequest();
            if (robotConfig.HasLeftGripper()) leftHandPositionRequest.Id = robotConfig.partsId["l_hand"];
            leftHandPositionRequest.Position = new HandPosition { ParallelGripper = new ParallelGripperPosition { OpeningPercentage = leftGripperOpening } };

            HandPositionRequest rightHandPositionRequest = new HandPositionRequest();
            if (robotConfig.HasRightGripper()) rightHandPositionRequest.Id = robotConfig.partsId["r_hand"];
            rightHandPositionRequest.Position = new HandPosition { ParallelGripper = new ParallelGripperPosition { OpeningPercentage = rightGripperOpening } };

            ActualSendGrippersCommands(leftHandPositionRequest, rightHandPositionRequest);
        }

        public async void ReachySad()
        {
            Debug.LogError("Simulated Reachy is sad");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);

            CancellationToken cancellationToken = askForCancellation.Token;
            await Task.Delay(5000);

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

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }

        public async void ReachyHappy()
        {
            Debug.LogError("Simulated Reachy is happy");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;

            await Task.Delay(5000);

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
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }

        public async void ReachyConfused()
        {
            Debug.LogError("Reachy is confused");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;
            await Task.Delay(5000);


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

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }
    }
}