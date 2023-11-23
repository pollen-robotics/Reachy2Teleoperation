using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Grpc.Core;
using System.Threading.Tasks;

using Reachy;
using Reachy.Part;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Component;


namespace TeleopReachy
{
    public class gRPCDataController : gRPCBase
    {
        private ReachyService.ReachyServiceClient reachyClient = null;
        private ArmService.ArmServiceClient armClient = null;
        private HeadService.HeadServiceClient headClient = null;
        private HandService.HandServiceClient handClient = null;

        private bool needUpdateCommandBody;
        private bool needUpdateCommandGripper;
        // private bool needUpdateState;

        private Reachy.Reachy reachy;

        public UnityEvent<bool> event_DataControllerStatusHasChanged;

        public UnityEvent<Reachy.Reachy> event_OnRobotReceived;

        // public UnityEvent<Dictionary<ComponentId, float>> event_OnStateUpdateTemperature;

        // public UnityEvent<Dictionary<ComponentId, float>> event_OnStateUpdatePresentPositions;

        void Start()
        {
            needUpdateCommandBody = false;
            needUpdateCommandGripper = false;
            // needUpdateState = false;

            InitChannel("server_data_port");
            if (channel != null)
            {
                reachyClient = new ReachyService.ReachyServiceClient(channel);
                armClient = new ArmService.ArmServiceClient(channel);
                headClient = new HeadService.HeadServiceClient(channel);
                handClient = new HandService.HandServiceClient(channel);

                Task.Run(() => GetReachyId());
            }
        }

        protected override void RecoverFromNetWorkIssue()
        {
            Task.Run(() => GetReachyId());
            gRPCManager.Instance.gRPCMobileBaseController.AskForMobilityReset();
        }

        protected override void NotifyDisconnection()
        {
            Debug.LogWarning("GRPC DataController disconnected");
            isRobotInRoom = false;
            event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
        }

        private void GetReachyId()
        {
            try
            {
                Debug.Log(reachyClient);
                reachy = reachyClient.GetReachy(new Google.Protobuf.WellKnownTypes.Empty());
                event_OnRobotReceived.Invoke(reachy);
                isRobotInRoom = true;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
                needUpdateCommandBody = true;
                needUpdateCommandGripper = true;
                // needUpdateState = true;
            }
            catch (RpcException e)
            {
                Debug.LogWarning("RPC failed: " + e);
                rpcException = "Error in GetReachyId():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }


        // public async void SendJointsCommand(JointsCommand jointsCommand)
        // {
        //     try
        //     {
        //         await client.SendJointsCommandsAsync(jointsCommand);
        //     }
        //     catch (RpcException e)
        //     {
        //         Debug.LogWarning("Communication RPC failed: in SendJointsPositions():" + e);
        //         rpcException = "Error in SendJointsPositions():\n" + e.ToString();
        //         isRobotInRoom = false;
        //         event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
        //     }
        // }

        public async void SetHandPosition(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand)
        {
            try
            {
                if (needUpdateCommandGripper)
                {
                    needUpdateCommandGripper = false;
                    await handClient.SetHandPositionAsync(leftGripperCommand);
                    await handClient.SetHandPositionAsync(rightGripperCommand);
                    needUpdateCommandGripper = true;
                }
            }
            catch (RpcException e)
            {
                Debug.LogWarning("Communication RPC failed: in SendJointsPositions():" + e);
                rpcException = "Error in SendJointsPositions():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        public async void SendBodyCommand(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest, NeckGoal neckRequest)
        {
            try
            {
                if (needUpdateCommandBody)
                {
                    needUpdateCommandBody = false;
                    await armClient.GoToCartesianPositionAsync(leftArmRequest);
                    await armClient.GoToCartesianPositionAsync(rightArmRequest);
                    await headClient.GoToOrientationAsync(neckRequest);
                    needUpdateCommandBody = true;
                }
            }
            catch (RpcException e)
            {
                Debug.LogWarning("GRPC failed: in SendBodyCommand():" + e);
                rpcException = "Error in SendBodyCommand():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        public async void SendHeadCommand(NeckGoal neckRequest)
        {
            try
            {
                if (needUpdateCommandBody)
                {
                    needUpdateCommandBody = false;
                    await headClient.GoToOrientationAsync(neckRequest);
                    needUpdateCommandBody = true;
                }
            }
            catch (RpcException e)
            {
                Debug.LogWarning("GRPC failed: in SendBodyCommand():" + e);
                rpcException = "Error in SendBodyCommand():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        public async void TurnArmOff(PartId id)
        {
            await armClient.TurnOffAsync(id);
        }

        public async void TurnHeadOff(PartId id)
        {
            await headClient.TurnOffAsync(id);
        }

        public async void TurnArmOn(PartId id)
        {
            await armClient.TurnOnAsync(id);
        }

         public async void TurnHeadOn(PartId id)
        {
            await headClient.TurnOnAsync(id);
        }

        // public async void GetJointsState()
        // {
        //     try
        //     {
        //         if (needUpdateState)
        //         {
        //             needUpdateState = false;

        //             List<JointId> ids = new List<JointId>();
        //             foreach (var item in allJointsId.Names)
        //             {
        //                 var joint = new JointId();
        //                 joint.Name = item;

        //                 ids.Add(joint);
        //             };

        //             JointsStateRequest jointsRequest = new JointsStateRequest
        //             {
        //                 Ids = { ids },
        //                 RequestedFields = { JointField.Name, JointField.PresentPosition, JointField.GoalPosition, JointField.Temperature },
        //             };

        //             var reply = await client.GetJointsStateAsync(jointsRequest);

        //             Dictionary<JointId, float> present_positions = new Dictionary<JointId, float>();
        //             //Dictionary<JointId, float> goal_positions = new Dictionary<JointId, float>();
        //             Dictionary<JointId, float> temperatures = new Dictionary<JointId, float>();

        //             for (int i = 0; i < reply.States.Count; i++)
        //             {
        //                 float command = Mathf.Rad2Deg * (float)reply.States[i].PresentPosition;
        //                 present_positions.Add(reply.Ids[i], command);
        //                 //float expectedcommand = Mathf.Rad2Deg * (float)reply.States[i].GoalPosition;
        //                 //goal_positions.Add(reply.Ids[i], expectedcommand);
        //                 float temperature = (float)reply.States[i].Temperature;
        //                 temperatures.Add(reply.Ids[i], temperature);
        //             }
        //             //OnJointsStateReceivedEvent(new StateUpdateEventArgs(present_positions, goal_positions, temperatures));
        //             event_OnStateUpdateTemperature.Invoke(temperatures);
        //             event_OnStateUpdatePresentPositions.Invoke(present_positions);
        //             needUpdateState = true;
        //         }
        //     }
        //     catch (RpcException e)
        //     {
        //         Debug.LogWarning("RPC failed: " + e);
        //         rpcException = "Error in GetJointsState():\n" + e.ToString();
        //         isRobotInRoom = false;
        //         event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
        //     }
        // }
    }
}