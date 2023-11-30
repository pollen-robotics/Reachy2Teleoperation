using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Grpc.Core;
using System.Threading.Tasks;

using Google.Protobuf;
using Reachy;
using Reachy.Part;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Reachy.Kinematics;
using Component;
using Component.Orbita2D;
using Component.Orbita3D;


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
        private bool needUpdateState;

        private Reachy.Reachy reachy;

        public UnityEvent<bool> event_DataControllerStatusHasChanged;

        public UnityEvent<Reachy.Reachy> event_OnRobotReceived;

        public UnityEvent<Dictionary<string, float>> event_OnStateUpdateTemperature;
        public UnityEvent<Dictionary<string, float>> event_OnStateUpdatePresentPositions;

        void Start()
        {
            needUpdateCommandBody = false;
            needUpdateCommandGripper = false;
            needUpdateState = false;

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
                needUpdateState = true;
                StreamReachyState();
            }
            catch (RpcException e)
            {
                Debug.LogWarning("RPC failed: " + e);
                rpcException = "Error in GetReachyId():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        private async void StreamReachyState()
        {
            try 
            {
                ReachyStreamStateRequest request = new ReachyStreamStateRequest { Id = reachy.Id, PublishFrequency = 60 };

                using var reachyState = reachyClient.StreamReachyState(request);

                var reachyDescriptor = ReachyState.Descriptor;
                var armDescriptor = ArmState.Descriptor;
                var headDescriptor = HeadState.Descriptor;
                var handDescriptor = HandState.Descriptor;

                while (await reachyState.ResponseStream.MoveNext())
                {
                    if(!needUpdateState)
                    {
                        break;
                    }

                    Dictionary<string, float> present_position = new Dictionary<string, float>();
                    Dictionary<string, float> temperatures = new Dictionary<string, float>();

                    foreach (var partField in reachyDescriptor.Fields.InDeclarationOrder())
                    {
                        var partState = partField.Accessor.GetValue(reachyState.ResponseStream.Current) as IMessage;
                        if (partState != null)
                        {
                            if(partState is ArmState)
                            {
                                foreach (var componentField in armDescriptor.Fields.InDeclarationOrder())
                                {
                                    var componentState = componentField.Accessor.GetValue(partState) as IMessage;
                                    if(componentState is Orbita2dState)
                                    {
                                        GetOrbita2D_PresentPosition(present_position, componentState, partField, componentField);
                                        GetOrbita2D_Temperature(temperatures, componentState, partField, componentField);
                                    }
                                    if(componentState is Orbita3dState)
                                    {
                                        GetOrbita3D_PresentPosition(present_position, componentState, partField, componentField);
                                        GetOrbita3D_Temperature(temperatures, componentState, partField, componentField);
                                    }
                                }
                            }
                            if(partState is HeadState)
                            {
                                foreach (var componentField in headDescriptor.Fields.InDeclarationOrder())
                                {
                                    var componentState = componentField.Accessor.GetValue(partState) as IMessage;
                                    if(componentState is Orbita3dState)
                                    {
                                        GetOrbita3D_PresentPosition(present_position, componentState, partField, componentField);
                                        GetOrbita3D_Temperature(temperatures, componentState, partField, componentField);
                                    }
                                }
                            }
                            if(partState is HandState)
                            {
                                GetParallelGripper_PresentPosition(present_position, partState, partField);
                                GetParallelGripper_Temperature(temperatures, partState, partField);
                            }
                        }
                    }
                    event_OnStateUpdatePresentPositions.Invoke(present_position);
                    event_OnStateUpdateTemperature.Invoke(temperatures);
                }
            }
            catch (RpcException e)
            {
                Debug.LogWarning("RPC failed: " + e);
                rpcException = "Error in GetJointsState():\n" + e.ToString();
                isRobotInRoom = false;
                event_DataControllerStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        void OnDestroy()
        {
            needUpdateState = false;
        }

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

        private void GetOrbita3D_PresentPosition(
            Dictionary<string, float> dict,
            IMessage componentState, 
            Google.Protobuf.Reflection.FieldDescriptor partField,
            Google.Protobuf.Reflection.FieldDescriptor componentField
            )
        {
            var eulerAnglesDescriptor = ExtEulerAngles.Descriptor;

            var pres_pos = componentState.Descriptor.FindFieldByName("present_position");
            if (pres_pos != null)
            {
                Rotation3d pp = (Rotation3d)pres_pos.Accessor.GetValue(componentState);
                ExtEulerAngles eulerAngles = pp.Rpy;

                foreach (var axisField in eulerAnglesDescriptor.Fields.InDeclarationOrder())
                {
                    string[] side = partField.Name.Split("state");
                    string[] component = componentField.Name.Split("state");
                    string joint_name = side[0] + component[0] + axisField.Name;

                    double value = (double)axisField.Accessor.GetValue(eulerAngles);
                    dict.Add(joint_name, Mathf.Rad2Deg * (float)value);
                }
            }
        }

        private void GetOrbita2D_PresentPosition(
            Dictionary<string, float> dict,
            IMessage componentState, 
            Google.Protobuf.Reflection.FieldDescriptor partField,
            Google.Protobuf.Reflection.FieldDescriptor componentField
            )
        {
            var pose2dDescriptor = Pose2d.Descriptor;

            var pres_pos = componentState.Descriptor.FindFieldByName("present_position");
            if (pres_pos != null)
            {
                Pose2d pose = (Pose2d)pres_pos.Accessor.GetValue(componentState);

                foreach (var axisField in pose2dDescriptor.Fields.InDeclarationOrder())
                {
                    string[] side = partField.Name.Split("state");
                    string[] component = componentField.Name.Split("state");
                    string joint_name = side[0] + component[0] + axisField.Name;

                    float value = (float)axisField.Accessor.GetValue(pose);
                    dict.Add(joint_name, Mathf.Rad2Deg * value);
                }
            }
        }

        private void GetOrbita2D_Temperature(
            Dictionary<string, float> dict,
            IMessage componentState, 
            Google.Protobuf.Reflection.FieldDescriptor partField,
            Google.Protobuf.Reflection.FieldDescriptor componentField
            )
        {
            var float2dDescriptor = Float2d.Descriptor;

            var temp = componentState.Descriptor.FindFieldByName("temperature");
            if (temp != null)
            {
                Float2d temperature = (Float2d)temp.Accessor.GetValue(componentState);

                foreach (var motorField in float2dDescriptor.Fields.InDeclarationOrder())
                {
                    string[] side = partField.Name.Split("state");
                    string[] component = componentField.Name.Split("state");
                    string motor_name = side[0] + component[0] + motorField.Name;

                    float value = (float)motorField.Accessor.GetValue(temperature);
                    dict.Add(motor_name, value);
                }
            }
        }

        private void GetOrbita3D_Temperature(
            Dictionary<string, float> dict,
            IMessage componentState, 
            Google.Protobuf.Reflection.FieldDescriptor partField,
            Google.Protobuf.Reflection.FieldDescriptor componentField
            )
        {
            var float3dDescriptor = Float3d.Descriptor;

            var temp = componentState.Descriptor.FindFieldByName("temperature");
            if (temp != null)
            {
                Float3d temperature = (Float3d)temp.Accessor.GetValue(componentState);

                foreach (var motorField in float3dDescriptor.Fields.InDeclarationOrder())
                {
                    string[] side = partField.Name.Split("state");
                    string[] component = componentField.Name.Split("state");
                    string motor_name = side[0] + component[0] + motorField.Name;

                    float value = (float)motorField.Accessor.GetValue(temperature);
                    dict.Add(motor_name, value);
                }
            }
        }

        private void GetParallelGripper_PresentPosition(
            Dictionary<string, float> dict,
            IMessage partState, 
            Google.Protobuf.Reflection.FieldDescriptor partField
            )
        {
            var pres_pos = partState.Descriptor.FindFieldByName("present_position");
            if (pres_pos != null)
            {
                HandPosition pp = (HandPosition)pres_pos.Accessor.GetValue(partState);

                string[] side = partField.Name.Split("_state");
                string joint_name = side[0];

                float value = (float)pp.ParallelGripper.Position;
                dict.Add(joint_name, Mathf.Rad2Deg * value);
            }
        }

        private void GetParallelGripper_Temperature(
            Dictionary<string, float> dict,
            IMessage partState, 
            Google.Protobuf.Reflection.FieldDescriptor partField
            )
        {
            var temperaturesDescriptor = Temperatures.Descriptor;

            var temp = partState.Descriptor.FindFieldByName("temperature");
            if (temp != null)
            {
                HandTemperatures temperature = (HandTemperatures)temp.Accessor.GetValue(partState);

                foreach (var motorField in temperaturesDescriptor.Fields.InDeclarationOrder())
                {
                    string[] side = partField.Name.Split("state");
                    string element_name = side[0] + motorField.Name;

                    float value = (float)motorField.Accessor.GetValue(temperature.ParallelGripper);
                    dict.Add(element_name, value);
                }
            }
        }
    }
}