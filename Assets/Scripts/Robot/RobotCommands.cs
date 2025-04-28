using System;
using System.Linq;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy.Part.Hand;
using Component.DynamixelMotor;
using Component;


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
            Debug.Log("Reachy is sad");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;
            
            try
            {
                float origin = 0;
                float target1 = 130;

                foreach (var t in Enumerable.Range(0, 60).Select(i => (float)(i / 59.0)))
                {
                    float interpolated = (1 - t) * origin + t * target1;
                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolated)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(-interpolated)},
                        }
                    };
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                float duration = 1.0f;
                int sampleRate = 200;
                int totalSteps = (int)(duration * sampleRate);
                float[] tValues = Enumerable.Range(0, totalSteps)
                                            .Select(i => (float)(i / (float)sampleRate))
                                            .ToArray();

                float[] positions = tValues.Select(t => (float)(10 * Math.Sin(2 * Math.PI * t) + target1)).ToArray();

                foreach (var p in positions)
                {
                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(p)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(-p)},
                        }
                    };
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Delay(500);

                foreach (var t in Enumerable.Range(0, 60).Select(i => (float)(i / 59.0)))
                {
                    float interpolated = (1 - t) * target1 + t * origin;
                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolated)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(-interpolated)},
                        }
                    };
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Reachy sad has been canceled: " + e);
            }

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }

        public async void ReachyHappy()
        {
            Debug.Log("Reachy is happy");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;

            float duration = 2.0f;
            int sampleRate = 60; // 100 samples per second
            int totalSteps = (int)(duration * sampleRate);
            float[] t = Enumerable.Range(0, totalSteps)
                                .Select(i => i / (float)sampleRate)
                                .ToArray();
            float[] positions = t.Select(time => (float)(10 * Math.Sin(2 * Math.PI * 5 * time))).ToArray();

            try
            {
                foreach (var p in positions)
                {
                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(p)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(-p)},
                        }
                    };
                    
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Reachy happy has been canceled: " + e);
            }

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }

        public async void ReachyConfused()
        {
            Debug.Log("Reachy is confused");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                float origin = 0;
                float targetLeft = -20;
                float targetRight = -70;

                foreach (var t in Enumerable.Range(0, 30).Select(i => (float)(i / 29.0)))
                {
                    float interpolatedLeft = (1 - t) * origin + t * targetLeft;
                    float interpolatedRight = (1 - t) * origin + t * targetRight;

                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolatedLeft)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolatedRight)},
                        }
                    };
                    
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Delay(2000);

                foreach (var t in Enumerable.Range(0, 30).Select(i => (float)(i / 29.0)))
                {
                    float interpolatedLeft = (1 - t) * targetLeft + t * origin;
                    float interpolatedRight = (1 - t) * targetRight + t * origin;

                    DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                        Cmd = {
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolatedLeft)},
                            new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                    GoalPosition = Mathf.Deg2Rad*(interpolatedRight)},
                        }
                    };
                    
                    ActualSendAntennasCommands(currentCommands);
                    await Task.Delay(15);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Reachy confused has been canceled: " + e);
            }

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }

        public async void ReachyAngry()
        {
            Debug.Log("Reachy is angry");
            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(true);
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                float origin = 0;
                float targetLeft = 80;
                float targetRight = -80;

                for (int nb_repetition = 0; nb_repetition < 2; nb_repetition++)
                {
                    foreach (var t in Enumerable.Range(0, 10).Select(i => (float)(i / 9.0)))
                    {
                        float interpolatedLeft = (1 - t) * origin + t * targetLeft;
                        float interpolatedRight = (1 - t) * origin + t * targetRight;

                        DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                            Cmd = {
                                new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                        GoalPosition = Mathf.Deg2Rad*(interpolatedLeft)},
                                new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                        GoalPosition = Mathf.Deg2Rad*(interpolatedRight)},
                            }
                        };
                        
                        ActualSendAntennasCommands(currentCommands);
                        await Task.Delay(15);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    foreach (var t in Enumerable.Range(0, 30).Select(i => (float)(i / 29.0)))
                    {
                        float interpolatedLeft = (1 - t) * targetLeft + t * origin;
                        float interpolatedRight = (1 - t) * targetRight + t * origin;

                        DynamixelMotorsCommand currentCommands = new DynamixelMotorsCommand {
                            Cmd = {
                                new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_left" },
                                                        GoalPosition = Mathf.Deg2Rad*(interpolatedLeft)},
                                new DynamixelMotorCommand { Id=new ComponentId { Name = "antenna_right" },
                                                        GoalPosition = Mathf.Deg2Rad*(interpolatedRight)},
                            }
                        };
                        
                        ActualSendAntennasCommands(currentCommands);
                        await Task.Delay(15);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Reachy angry has been canceled: " + e);
            }

            if (robotConfig.HasHead() && robotStatus.IsHeadOn()) robotStatus.SetEmotionPlaying(false);
        }
    }
}