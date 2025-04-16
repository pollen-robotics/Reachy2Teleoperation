using System.Collections.Generic;
using System.Collections;
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
using Reachy.Part.Mobile.Base.Mobility;
using Reachy.Part.Mobile.Base.Utility;
using Reachy.Part.Mobile.Base.Lidar;
using Bridge;
using GstreamerWebRTC;


namespace TeleopReachy
{
    public class DataMessageManager : Singleton<DataMessageManager>
    {
        public UnityEvent<Reachy.Reachy> event_OnRobotReceived;

        public UnityEvent<Dictionary<string, float>> event_OnStateUpdateTemperature;
        public UnityEvent<Dictionary<string, float>> event_OnStateUpdatePresentPositions;
        public UnityEvent<Dictionary<int, List<ReachabilityAnswer>>> event_OnStateUpdateReachability;
        public UnityEvent<Dictionary<string, string>> event_OnAuditUpdate;
        public UnityEvent<float> event_OnBatteryUpdate;
        public UnityEvent<LidarObstacleDetectionEnum> event_OnLidarDetectionUpdate;

        protected GStreamerPluginCustom webRTCController;
        protected AnyCommands commands = new AnyCommands { };

        protected virtual void Start()
        {
            webRTCController = WebRTCManager.Instance.gstreamerPlugin;
        }

        protected void Update()
        {
            if (commands.Commands.Count != 0)
            {
                webRTCController.SendCommandMessageLossy(commands);
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
            Dictionary<int, List<ReachabilityAnswer>> reachability = new Dictionary<int, List<ReachabilityAnswer>>();
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
                        PartId partId = new PartId();
                        var armId = partState.Descriptor.FindFieldByName("id");
                        if (armId != null)
                        {
                            partId = (PartId)armId.Accessor.GetValue(partState);
                        }
                        var reachabilityAnswer = partState.Descriptor.FindFieldByName("reachability");
                        if (reachabilityAnswer != null)
                        {
                            var reachabilityObject = reachabilityAnswer.Accessor.GetValue(partState);
                            IEnumerable reachabilityValues = reachabilityObject as IEnumerable;
                            List<ReachabilityAnswer> answers = new List<ReachabilityAnswer>();
                            if (reachabilityValues != null)
                            {
                                foreach (var reachabilityValue in reachabilityValues)
                                {
                                    ReachabilityAnswer reachable = (ReachabilityAnswer)reachabilityValue;
                                    answers.Add(reachable);
                                }
                            }
                            reachability.Add((int)partId.Id, answers);
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
            event_OnStateUpdateReachability.Invoke(reachability);
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
                                if (componentStatus is Orbita2dStatus status2d)
                                {
                                    errorDetails = status2d.Errors.Select(e => e.Details).ToArray();
                                }
                                if (componentStatus is Orbita3dStatus status3d)
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
                                if (componentStatus is Orbita3dStatus status3d)
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

        public virtual void SetHandPosition(HandPositionRequest gripperPosition)
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

        public virtual void SendArmCommand(ArmCartesianGoal armGoal)
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

        public virtual void SendNeckCommand(NeckJointGoal neckGoal)
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

        public virtual void SendMobileBaseCommand(TargetDirectionCommand direction)
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
            webRTCController.SendCommandMessageReliable(armCommand);
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
            webRTCController.SendCommandMessageReliable(neckCommand);
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
            webRTCController.SendCommandMessageReliable(handCommand);
        }

        public void TurnMobileBaseOff(PartId id)
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand
            {
                Id = id,
                Mode = ZuuuModePossiblities.FreeWheel
            };

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
            webRTCController.SendCommandMessageReliable(mobileBaseCommand);
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
            webRTCController.SendCommandMessageReliable(armCommand);
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
            webRTCController.SendCommandMessageReliable(neckCommand);
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
            webRTCController.SendCommandMessageReliable(handCommand);
        }

        public void TurnMobileBaseOn(PartId id)
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand
            {
                Id = id,
                Mode = ZuuuModePossiblities.CmdVel
            };

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
            webRTCController.SendCommandMessageReliable(mobileBaseCommand);
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
            webRTCController.SendCommandMessageReliable(armCommand);
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
            webRTCController.SendCommandMessageReliable(armCommand);
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
            webRTCController.SendCommandMessageReliable(neckCommand);
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
            webRTCController.SendCommandMessageReliable(neckCommand);
        }

        protected void GetOrbita3D_PresentPosition(
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

        protected void GetOrbita2D_PresentPosition(
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

        protected void GetOrbita2D_Temperature(
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

        protected void GetOrbita3D_Temperature(
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

        protected void GetParallelGripper_PresentPosition(
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

        protected void GetParallelGripper_Temperature(
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
