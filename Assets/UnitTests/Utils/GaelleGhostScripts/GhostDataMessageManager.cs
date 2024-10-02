using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

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
    public class GhostDataMessageManager : Singleton<DataMessageManager>
    {
        public GhostApplicationManager ghostApplicationManager;

        public UnityEvent<Reachy.Reachy> event_OnRobotReceived;

        public UnityEvent<Dictionary<string, float>> event_OnStateUpdateTemperature;
        public UnityEvent<Dictionary<string, float>> event_OnStateUpdatePresentPositions;
        public UnityEvent<Dictionary<string, string>> event_OnAuditUpdate;
        public UnityEvent<float> event_OnBatteryUpdate;
        public UnityEvent<LidarObstacleDetectionEnum> event_OnLidarDetectionUpdate;

        private GStreamerPluginCustom webRTCController;

        private RobotStatus robotStatus;

        public TextAsset RightArmTextFile;
        public TextAsset LeftArmTextFile;
        public TextAsset RightGripperTextFile;
        public TextAsset LeftGripperTextFile;
        public TextAsset NeckTextFile;
        public TextAsset MobileBaseTextFile;

        string[] RightArm;
        int right_arm_inc = 0;
        string[] RightGripper;
        int right_gripper_inc = 0;
        string[] LeftArm;
        int left_arm_inc = 0;
        string[] LeftGripper;
        int left_gripper_inc = 0;
        string[] Neck;
        int neck_inc = 0;
        string[] MobileBase;
        int mobile_base_inc = 0;

        private bool isReady = false;
        private bool isFirst = true;

        private AnyCommands commands = new AnyCommands { };

        void Start()
        {
            RightArm = RightArmTextFile.text.Split('\n');
            RightGripper = RightGripperTextFile.text.Split('\n');
            LeftArm = LeftArmTextFile.text.Split('\n');
            LeftGripper = LeftGripperTextFile.text.Split('\n');
            Neck = NeckTextFile.text.Split('\n');
            MobileBase = MobileBaseTextFile.text.Split('\n');

            ghostApplicationManager.event_BaseSceneLoaded.AddListener(OnBaseSceneLoaded);
        }

        void OnBaseSceneLoaded()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, GetRobotScripts);
        }

        void GetRobotScripts()
        {
            webRTCController = WebRTCManager.Instance.webRTCController;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(InitBackInc);
        }

        void InitBackInc()
        {
            isReady = false;
            right_arm_inc = 0;
            right_gripper_inc = 0;
            left_arm_inc = 0;
            left_gripper_inc = 0;
            neck_inc = 0;
            mobile_base_inc = 0;
            isFirst = true;
        }

        void Update()
        {
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

        public void SetHandPosition()
        {
            Bridge.AnyCommand r_handCommand;
            try
            {
                string r_line = RightGripper[right_gripper_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var r_json = JObject.Parse(r_line);
                r_handCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(r_json.ToString());
                right_gripper_inc++;
            }
            catch (Exception e)
            {
                right_gripper_inc = 0;
                string r_line = RightGripper[right_gripper_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var r_json = JObject.Parse(r_line);
                r_handCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(r_json.ToString());
                right_gripper_inc++;
            }

            Bridge.AnyCommand l_handCommand;
            try
            {
                string l_line = LeftGripper[left_gripper_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var l_json = JObject.Parse(l_line);
                l_handCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(l_json.ToString());
                left_gripper_inc++;
            }
            catch (Exception e)
            {
                left_gripper_inc = 0;
                string l_line = LeftGripper[left_gripper_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var l_json = JObject.Parse(l_line);
                l_handCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(l_json.ToString());
                left_gripper_inc++;
            }

            
            if(robotStatus.IsLeftGripperOn()) commands.Commands.Add(l_handCommand);
            if(robotStatus.IsRightGripperOn()) commands.Commands.Add(r_handCommand);
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

        IEnumerator WaitFor3Seconds()
        {
            yield return new WaitForSeconds(3);
            isReady = true;
        }

        public void SendArmCommand()
        {
            if(isFirst || isReady)
            {
                Bridge.AnyCommand r_armCommand;
                try
                {
                    string r_line = RightArm[right_arm_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                    var r_json = JObject.Parse(r_line);
                    r_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(r_json.ToString());
                    right_arm_inc++;
                }
                catch (Exception e)
                {
                    right_arm_inc = 0;
                    string r_line = RightArm[right_arm_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                    var r_json = JObject.Parse(r_line);
                    r_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(r_json.ToString());
                    right_arm_inc++;
                }
                
                Bridge.AnyCommand l_armCommand;
                try
                {
                    string l_line = LeftArm[left_arm_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                    var l_json = JObject.Parse(l_line);
                    l_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(l_json.ToString());
                    left_arm_inc++;
                }
                catch (Exception e)
                {
                    left_arm_inc = 0;
                    string l_line = LeftArm[left_arm_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                    var l_json = JObject.Parse(l_line);
                    l_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(l_json.ToString());
                    left_arm_inc++;
                }

                r_armCommand.ArmCommand.ArmCartesianGoal.ConstrainedMode = robotStatus.GetIKMode();
                l_armCommand.ArmCommand.ArmCartesianGoal.ConstrainedMode = robotStatus.GetIKMode();

                if(robotStatus.IsRightArmOn()) commands.Commands.Add(r_armCommand);
                if(robotStatus.IsLeftArmOn()) commands.Commands.Add(l_armCommand);
            }
            if(isFirst && !isReady)
            {
                isFirst = false;
                StartCoroutine(WaitFor3Seconds());
            }
            if(!isFirst && !isReady)
            {
                Bridge.AnyCommand r_armCommand;
                string r_line = RightArm[0].Trim('[', ']', ' ', '\t', '\n', '\r');

                var r_json = JObject.Parse(r_line);
                r_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(r_json.ToString());
                
                Bridge.AnyCommand l_armCommand;
                string l_line = LeftArm[0].Trim('[', ']', ' ', '\t', '\n', '\r');

                var l_json = JObject.Parse(l_line);
                l_armCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(l_json.ToString());

                r_armCommand.ArmCommand.ArmCartesianGoal.ConstrainedMode = robotStatus.GetIKMode();
                l_armCommand.ArmCommand.ArmCartesianGoal.ConstrainedMode = robotStatus.GetIKMode();

                if(robotStatus.IsRightArmOn()) commands.Commands.Add(r_armCommand);
                if(robotStatus.IsLeftArmOn()) commands.Commands.Add(l_armCommand);
            }
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

        public void SendNeckCommand()
        {
            Bridge.AnyCommand neckCommand;
            try
            {
                string n_line = Neck[neck_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var n_json = JObject.Parse(n_line);
                neckCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(n_json.ToString());
                neck_inc++;
            }
            catch (Exception e)
            {
                neck_inc = 0;
                string n_line = Neck[neck_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var n_json = JObject.Parse(n_line);
                neckCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(n_json.ToString());
                neck_inc++;
            }
            commands.Commands.Add(neckCommand);
        }

        public void SendMobileBaseCommand()
        {
           Bridge.AnyCommand mobileBaseCommand;
            try
            {
                string mb_line = MobileBase[mobile_base_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var mb_json = JObject.Parse(mb_line);
                mobileBaseCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(mb_json.ToString());
                mobile_base_inc++;
            }
            catch (Exception e)
            {
                mobile_base_inc = 0;
                string mb_line = MobileBase[mobile_base_inc].Trim('[', ']', ' ', '\t', '\n', '\r');

                var mb_json = JObject.Parse(mb_line);
                mobileBaseCommand = JsonParser.Default.Parse<Bridge.AnyCommand>(mb_json.ToString());
                mobile_base_inc++;
            }
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

        public void TurnMobileBaseOff(PartId id)
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand { 
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

        public void TurnMobileBaseOn(PartId id)
        {
            ZuuuModeCommand zuuuMode = new ZuuuModeCommand { 
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
