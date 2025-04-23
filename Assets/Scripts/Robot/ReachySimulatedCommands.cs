using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Reachy.Part;
using Reachy2Controller;
using Component.DynamixelMotor;


namespace TeleopReachy
{
    public class ReachySimulatedCommands : RobotCommands
    {
        private UserMovementsInput userMovementsInput;

        [SerializeField]
        private ReachySimulatedServer reachyFakeServer;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachyReal;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachySimulated;


        // Start is called before the first frame update
        void Start()
        {
            Init();
            userMovementsInput = UserInputManager.Instance.UserMovementsInput;
        }

        // Update is called once per frame
        void Update()
        {
            ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();
            ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
            NeckJointGoal headTarget = userMovementsInput.GetHeadTarget();

            // if (robotConfig.IsVirtual() || !robotStatus.IsLeftArmOn())
            //     SetLeftArmToModelPose();

            // if (robotConfig.IsVirtual() || !robotStatus.IsRightArmOn())
            //     SetRightArmToModelPose();

            // if (robotConfig.IsVirtual() || !robotStatus.IsHeadOn())
            //     SetHeadToModelPose();
            SendArmsCommands(leftEndEffector, rightEndEffector);
            SendNeckCommands(headTarget);

            float pos_right_gripper = userMovementsInput.GetRightGripperTarget(robotStatus.IsGraspingLockActivated());
            float pos_left_gripper = userMovementsInput.GetLeftGripperTarget(robotStatus.IsGraspingLockActivated());
            SendGrippersCommands(pos_left_gripper, pos_right_gripper);
        }

        protected override void ActualSendArmsCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest)
        {
            rightArmRequest.Id = new PartId { Name = "r_arm" };
            leftArmRequest.Id = new PartId { Name = "l_arm" };

            reachyFakeServer.SendArmCommand(leftArmRequest);
            reachyFakeServer.SendArmCommand(rightArmRequest);
        }

        protected override void ActualSendNeckCommands(NeckJointGoal neckRequest)
        {
            neckRequest.Id = new PartId { Name = "head" };

            reachyFakeServer.SendNeckCommand(neckRequest);
        }

        protected override void ActualSendGrippersCommands(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand)
        {
            leftGripperCommand.Id = new PartId { Name = "l_hand" };
            rightGripperCommand.Id = new PartId { Name = "r_hand" };

            if(leftGripperCommand.Id != null) reachyFakeServer.SetHandPosition(leftGripperCommand);
            if(rightGripperCommand.Id != null) reachyFakeServer.SetHandPosition(rightGripperCommand);
        }

        protected override void ActualSendAntennasCommands(DynamixelMotorsCommand antennasRequest)
        {
            foreach (DynamixelMotorCommand cmd in antennasRequest.Cmd)
            {
                if (cmd.Id.Name == "antenna_left")
                {
                    cmd.Id.Name = "head_l_antenna";
                    reachyFakeServer.SetAntennaPosition(cmd);
                }
                if (cmd.Id.Name == "antenna_right")
                {
                    cmd.Id.Name = "head_r_antenna";
                    reachyFakeServer.SetAntennaPosition(cmd);
                }
            }
        }

        //     void SetHeadToModelPose()
        //     {
        //         Dictionary<JointId, JointField> headJoints = new Dictionary<JointId, JointField>();
        //         JointField field = JointField.GoalPosition;
        //         var joint = new JointId();
        //         joint.Name = "neck_pitch";
        //         headJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "neck_roll";
        //         headJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "neck_yaw";
        //         headJoints.Add(joint, field);

        //         List<SerializableMotor> headMotors = reachySimulated.GetCurrentMotorsState(headJoints);

        //         Dictionary<JointId, float> headTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < headMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = headMotors[i].name;
        //             float goal = Mathf.Rad2Deg * headMotors[i].goal_position;
        //             headTarget.Add(joint, goal);
        //         }

        //         reachyReal.HandleCommand(headTarget);
        //     }

        //     void SetLeftArmToModelPose()
        //     {
        //         Dictionary<JointId, JointField> leftJoints = new Dictionary<JointId, JointField>();
        //         JointField field = JointField.GoalPosition;
        //         var joint = new JointId();
        //         joint.Name = "l_shoulder_pitch";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_shoulder_roll";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_arm_yaw";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_elbow_pitch";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_forearm_yaw";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_wrist_pitch";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_wrist_roll";
        //         leftJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "l_gripper";
        //         leftJoints.Add(joint, field);

        //         List<SerializableMotor> leftArmMotors = reachySimulated.GetCurrentMotorsState(leftJoints);

        //         Dictionary<JointId, float> leftArmTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < leftArmMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = leftArmMotors[i].name;
        //             float goal = Mathf.Rad2Deg * leftArmMotors[i].goal_position;
        //             leftArmTarget.Add(joint, goal);
        //         }

        //         reachyReal.HandleCommand(leftArmTarget);
        //     }

        //     void SetRightArmToModelPose()
        //     {
        //         Dictionary<JointId, JointField> rightJoints = new Dictionary<JointId, JointField>();
        //         JointField field = JointField.GoalPosition;
        //         var joint = new JointId();
        //         joint.Name = "r_shoulder_pitch";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_shoulder_roll";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_arm_yaw";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_elbow_pitch";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_forearm_yaw";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_wrist_pitch";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_wrist_roll";
        //         rightJoints.Add(joint, field);
        //         joint = new JointId();
        //         joint.Name = "r_gripper";
        //         rightJoints.Add(joint, field);

        //         List<SerializableMotor> rightArmMotors = reachySimulated.GetCurrentMotorsState(rightJoints);

        //         Dictionary<JointId, float> rightArmTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < rightArmMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = rightArmMotors[i].name;
        //             float goal = Mathf.Rad2Deg * rightArmMotors[i].goal_position;
        //             rightArmTarget.Add(joint, goal);
        //         }

        //         reachyReal.HandleCommand(rightArmTarget);
        //     }
    }
}
