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
    public class GhostDataMessageManager : DataMessageManager
    {
        public GhostApplicationManager ghostApplicationManager;

        private TeleoperationManager teleoperationManager;

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

        private bool keep_gripper_command = true;
        private bool keep_arm_command = true;

        protected override void Start()
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
            teleoperationManager = TeleoperationManager.Instance;
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, GetRobotScripts);
            EventManager.StartListening(EventNames.OnStopTeleoperation, InitBackInc);
        }

        void GetRobotScripts()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            webRTCController = WebRTCManager.Instance.gstreamerPlugin;
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

        public override void SetHandPosition(HandPositionRequest gripperPosition)
        {
            SetHandPosition();
        }

        public void SetHandPosition()
        {
            if(robotStatus.IsRightGripperOn() && robotStatus.IsLeftGripperOn())
            {
                keep_gripper_command = !keep_gripper_command;
            }
            if(keep_gripper_command)
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
        }

        public override void SendArmCommand(ArmCartesianGoal armGoal)
        {
            if(!teleoperationManager.IsArmTeleoperationActive)
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
            else
            {
                SendArmCommand();
            }
        }

        IEnumerator WaitFor3Seconds()
        {
            yield return new WaitForSeconds(3);
            isReady = true;
        }

        public void SendArmCommand()
        {
            if(robotStatus.IsLeftArmOn() && robotStatus.IsRightArmOn())
            {
                keep_arm_command = !keep_arm_command;
            }
            if(keep_arm_command)
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
        }

        public override void SendNeckCommand(NeckJointGoal neckGoal)
        {
            if(!teleoperationManager.IsRobotTeleoperationActive)
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
            else
            {
                SendNeckCommand();
            }
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

        public override void SendMobileBaseCommand(TargetDirectionCommand direction)
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
    }
}
