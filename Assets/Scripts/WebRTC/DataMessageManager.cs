using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Google.Protobuf;
using Reachy;
using Reachy.Part;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Reachy.Kinematics;
using Component.Orbita2D;
using Component.Orbita3D;
using Mobile.Base.Mobility;
using Mobile.Base.Utility;
using Mobile.Base.Lidar;
using Bridge;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class DataMessageManager : Singleton<DataMessageManager>
    {
        public UnityEvent<Reachy.Reachy> event_OnRobotReceived;

        public UnityEvent<Dictionary<string, float>> event_OnStateUpdateTemperature;
        public UnityEvent<Dictionary<string, float>> event_OnStateUpdatePresentPositions;
        public UnityEvent<Dictionary<string, string>> event_OnAuditUpdate;
        public UnityEvent<float> event_OnBatteryUpdate;
        public UnityEvent<LidarObstacleDetectionEnum> event_OnLidarDetectionUpdate;

        //private WebRTCData webRTCDataController;
        private GStreamerPluginCustom webRTCController;

        //private HandCommand lastRightHandCommand;
        //private HandCommand lastLeftHandCommand;
        //private ArmCommand lastRightArmCommand;
        //private ArmCommand lastLeftArmCommand;
        //private MobileBaseCommand lastMobileBaseCommand;

        private AnyCommands commands = new AnyCommands { };

        void Start()
        {
            //webRTCDataController = WebRTCManager.Instance.webRTCDataController;
            webRTCController = WebRTCManager.Instance.webRTCController;
        }

        void Update()
        {
            //if (commands.Commands.Count != 0) webRTCDataController.SendCommandMessage(commands);
            if (commands.Commands.Count != 0)
            {
                webRTCController.SendCommandMessage(commands);
            }
            commands = new AnyCommands { };
        }

        public void GetReachyId(Reachy.Reachy reachy)
        {
            event_OnRobotReceived.Invoke(reachy);
        }

        public void StreamReachyState(ReachyState reachyState)
        {
            var reachyDescriptor = ReachyState.Descriptor;
            var armDescriptor = ArmState.Descriptor;
            var headDescriptor = HeadState.Descriptor;
            var handDescriptor = HandState.Descriptor;

            Dictionary<string, float> present_position = new Dictionary<string, float>();
            Dictionary<string, float> temperatures = new Dictionary<string, float>();
            float batteryLevel;
            LidarObstacleDetectionEnum obstacleDetection;

            foreach (var partField in reachyDescriptor.Fields.InDeclarationOrder())
            {
                var partState = partField.Accessor.GetValue(reachyState) as IMessage;
                if (partState != null)
                {
                    if (partState is ArmState)
                    {
                        foreach (var componentField in armDescriptor.Fields.InDeclarationOrder())
                        {
                            var componentState = componentField.Accessor.GetValue(partState) as IMessage;
                            if (componentState is Orbita2dState)
                            {
                                GetOrbita2D_PresentPosition(present_position, componentState, partField, componentField);
                                GetOrbita2D_Temperature(temperatures, componentState, partField, componentField);
                            }
                            if (componentState is Orbita3dState)
                            {
                                GetOrbita3D_PresentPosition(present_position, componentState, partField, componentField);
                                GetOrbita3D_Temperature(temperatures, componentState, partField, componentField);
                            }
                        }
                    }
                    if (partState is HeadState)
                    {
                        foreach (var componentField in headDescriptor.Fields.InDeclarationOrder())
                        {
                            var componentState = componentField.Accessor.GetValue(partState) as IMessage;
                            if (componentState is Orbita3dState)
                            {
                                GetOrbita3D_PresentPosition(present_position, componentState, partField, componentField);
                                GetOrbita3D_Temperature(temperatures, componentState, partField, componentField);
                            }
                        }
                    }
                    if (partState is HandState)
                    {
                        GetParallelGripper_PresentPosition(present_position, partState, partField);
                        GetParallelGripper_Temperature(temperatures, partState, partField);
                    }
                    if (partState is MobileBaseState)
                    {
                        var batteryField = partState.Descriptor.FindFieldByName("battery_level");
                        var batteryValue = batteryField.Accessor.GetValue(partState);
                        if (batteryValue != null)
                        {
                            BatteryLevel battery = (BatteryLevel)batteryValue;
                            batteryLevel = (float)battery.Level;
                            event_OnBatteryUpdate.Invoke(batteryLevel);
                        }

                        var lidarDetectionField = partState.Descriptor.FindFieldByName("lidar_safety");
                        var lidarDetectionValue = lidarDetectionField.Accessor.GetValue(partState);
                        if (lidarDetectionValue != null)
                        {
                            LidarSafety lidarSafety = (LidarSafety)lidarDetectionValue;
                            LidarObstacleDetectionStatus lidarDetectionStatus = lidarSafety.ObstacleDetectionStatus;
                            obstacleDetection = lidarDetectionStatus.Status;
                            if (obstacleDetection == LidarObstacleDetectionEnum.ObjectDetectedSlowdown || obstacleDetection == LidarObstacleDetectionEnum.ObjectDetectedStop)
                            {
                                event_OnLidarDetectionUpdate.Invoke(obstacleDetection);
                            }
                        }
                    }
                }
            }

            event_OnStateUpdatePresentPositions.Invoke(present_position);
            event_OnStateUpdateTemperature.Invoke(temperatures);
        }

        public void StreamReachyStatus(ReachyStatus reachyStatus)
        {
            var auditDescriptor = ReachyStatus.Descriptor;
            var armDescriptor = ArmStatus.Descriptor;
            var headDescriptor = HeadStatus.Descriptor;
            var handDescriptor = HandStatus.Descriptor;

            Dictionary<string, string> components_status = new Dictionary<string, string>();

            foreach (var partField in auditDescriptor.Fields.InDeclarationOrder())
            {
                var partStatus = partField.Accessor.GetValue(reachyStatus) as IMessage;
                if (partStatus != null)
                {
                    if (partStatus is ArmStatus)
                    {
                        foreach (var componentField in armDescriptor.Fields.InDeclarationOrder())
                        {
                            var componentStatus = componentField.Accessor.GetValue(partStatus) as IMessage;
                            if (componentStatus != null)  
                            {
                                string[] errorDetails = new string[0];
                                if(componentStatus is Orbita2dStatus status2d)
                                {
                                    errorDetails = status2d.Errors.Select(e => e.Details).ToArray();
                                }
                                if(componentStatus is Orbita3dStatus status3d)
                                {
                                    errorDetails = status3d.Errors.Select(e => e.Details).ToArray();
                                }
                                string[] side = partField.Name.Split("status");
                                string[] component = componentField.Name.Split("_status");
                                string component_name = side[0] + component[0];
                                components_status.Add(component_name, errorDetails[0]);
                            }
                        }
                    }
                    if (partStatus is HeadStatus)
                    {
                        foreach (var componentField in headDescriptor.Fields.InDeclarationOrder())
                        {
                            var componentStatus = componentField.Accessor.GetValue(partStatus) as IMessage;
                            if (componentStatus != null) 
                            {
                                if(componentStatus is Orbita3dStatus status3d)
                                {
                                    string[] errorDetails = status3d.Errors.Select(e => e.Details).ToArray();
                                    string[] side = partField.Name.Split("status");
                                    string[] component = componentField.Name.Split("_status");
                                    string component_name = side[0] + component[0];
                                    components_status.Add(component_name, errorDetails[0]);
                                }
                                
                            }
                        }
                    }
                }
            }
            event_OnAuditUpdate.Invoke(components_status);
        }

        public void SetHandPosition(HandPositionRequest gripperPosition)
        {
            Bridge.AnyCommand handCommand = new Bridge.AnyCommand
            {
                HandCommand = new Bridge.HandCommand
                {
                    HandGoal = gripperPosition
                }
            };
            commands.Commands.Add(handCommand);
        }

        public void SendArmCommand(ArmCartesianGoal armGoal)
        {
            Bridge.AnyCommand armCommand = new Bridge.AnyCommand
            {
                ArmCommand = new Bridge.ArmCommand
                {
                    ArmCartesianGoal = armGoal
                }
            };
            commands.Commands.Add(armCommand);
        }

        public void SendNeckCommand(NeckJointGoal neckGoal)
        {
            Bridge.AnyCommand neckCommand = new Bridge.AnyCommand
            {
                NeckCommand = new Bridge.NeckCommand
                {
                    NeckGoal = neckGoal
                }
            };
            commands.Commands.Add(neckCommand);
        }

        public void SendMobileBaseCommand(TargetDirectionCommand direction)
        {
            Bridge.AnyCommand mobileBaseCommand = new Bridge.AnyCommand
            {
                MobileBaseCommand = new Bridge.MobileBaseCommand
                {
                    TargetDirection = direction
                }
            };
            commands.Commands.Add(mobileBaseCommand);
        }

        public void TurnArmOff(PartId id)
        {
            Bridge.AnyCommands armCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        ArmCommand = new Bridge.ArmCommand{
                            TurnOff = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(armCommand);
        }

        public void TurnHeadOff(PartId id)
        {
            Bridge.AnyCommands neckCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        NeckCommand = new Bridge.NeckCommand{
                            TurnOff = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(neckCommand);
        }

        public void TurnHandOff(PartId id)
        {
            Bridge.AnyCommands handCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        HandCommand = new Bridge.HandCommand{
                            TurnOff = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(handCommand);
        }

        public void TurnMobileBaseOff()
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand { Mode = ZuuuModePossiblities.FreeWheel };

            Bridge.AnyCommands mobileBaseCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        MobileBaseCommand = new Bridge.MobileBaseCommand{
                            MobileBaseMode = zuuuMode
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(mobileBaseCommand);
        }

        public void TurnArmOn(PartId id)
        {
            Bridge.AnyCommands armCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        ArmCommand = new Bridge.ArmCommand{
                            TurnOn = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(armCommand);
        }

        public void TurnHeadOn(PartId id)
        {
            Bridge.AnyCommands neckCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        NeckCommand = new Bridge.NeckCommand{
                            TurnOn = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(neckCommand);
        }

        public void TurnHandOn(PartId id)
        {
            Bridge.AnyCommands handCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        HandCommand = new Bridge.HandCommand{
                            TurnOn = id
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(handCommand);
        }

        public void TurnMobileBaseOn()
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand { Mode = ZuuuModePossiblities.CmdVel };

            Bridge.AnyCommands mobileBaseCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        MobileBaseCommand = new Bridge.MobileBaseCommand{
                            MobileBaseMode = zuuuMode
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(mobileBaseCommand);
        }

        public void SetArmTorqueLimit(Reachy.Part.Arm.TorqueLimitRequest request)
        {
            Bridge.AnyCommands armCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        ArmCommand = new Bridge.ArmCommand{
                            TorqueLimit = request
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(armCommand);
        }

        public void SetArmSpeedLimit(Reachy.Part.Arm.SpeedLimitRequest request)
        {
            Bridge.AnyCommands armCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        ArmCommand = new Bridge.ArmCommand{
                            SpeedLimit = request
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(armCommand);
        }

        public void SetHeadSpeedLimit(Reachy.Part.Head.SpeedLimitRequest request)
        {
            Bridge.AnyCommands neckCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        NeckCommand = new Bridge.NeckCommand{
                            SpeedLimit = request
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(neckCommand);
        }

        public void SetHeadTorqueLimit(Reachy.Part.Head.TorqueLimitRequest request)
        {
            Bridge.AnyCommands neckCommand = new Bridge.AnyCommands
            {
                Commands = {
                    new Bridge.AnyCommand
                    {
                        NeckCommand = new Bridge.NeckCommand{
                            TorqueLimit = request
                        }
                    }
                }
            };
            webRTCController.SendCommandMessage(neckCommand);
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

                    float value = (float)axisField.Accessor.GetValue(eulerAngles);
                    dict.Add(joint_name, Mathf.Rad2Deg * value);
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
