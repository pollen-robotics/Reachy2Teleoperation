using UnityEngine;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Reachy.Part;
using Reachy2Controller;

namespace TeleopReachy
{
    public class ReachySimulatedCommands : RobotCommands
    {
        private HeadTracker headTracker;
        private HandsTracker handsTracker;
        private UserMovementsInput userMovementsInput;

        [SerializeField]
        private ReachySimulatedServer reachyFakeServer;

        private Reachy2Controller.Reachy2Controller reachy;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachyModel;


        // Start is called before the first frame update
        void Start()
        {
            Init();
            headTracker = UserTrackerManager.Instance.HeadTracker;
            handsTracker = UserTrackerManager.Instance.HandsTracker;
            userMovementsInput = UserInputManager.Instance.UserMovementsInput;

            reachy = GameObject.Find("Reachy2").transform.GetComponent<Reachy2Controller.Reachy2Controller>();
        }

        // Update is called once per frame
        void Update()
        {
            ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();
            ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
            NeckJointGoal headTarget = headTracker.GetHeadTarget();

            // if (robotConfig.IsVirtual() || !robotStatus.IsLeftArmOn())
            //     SetLeftArmToModelPose();

            // if (robotConfig.IsVirtual() || !robotStatus.IsRightArmOn())
            //     SetRightArmToModelPose();

            // if (robotConfig.IsVirtual() || !robotStatus.IsHeadOn())
            //     SetHeadToModelPose();
            SendFullBodyCommands(leftEndEffector, rightEndEffector, headTarget);

            float pos_right_gripper = userMovementsInput.GetRightGripperTarget();
            float pos_left_gripper = userMovementsInput.GetLeftGripperTarget();
            SendGrippersCommands(pos_left_gripper, pos_right_gripper);
        }

        protected override void ActualSendBodyCommands(ArmCartesianGoal leftArmRequest, ArmCartesianGoal rightArmRequest, NeckJointGoal neckRequest)
        {
            rightArmRequest.Id = new PartId { Name = "r_arm" };
            leftArmRequest.Id = new PartId { Name = "l_arm" };
            neckRequest.Id = new PartId { Name = "head" };

            reachyFakeServer.SendArmCommand(leftArmRequest);
            reachyFakeServer.SendArmCommand(rightArmRequest);
            reachyFakeServer.SendNeckCommand(neckRequest);
        }

        protected override void ActualSendGrippersCommands(HandPositionRequest leftGripperCommand, HandPositionRequest rightGripperCommand)
        {
            leftGripperCommand.Id = new PartId { Name = "l_hand" };
            rightGripperCommand.Id = new PartId { Name = "r_hand" };

            if(leftGripperCommand.Id != null) reachyFakeServer.SetHandPosition(leftGripperCommand);
            if(rightGripperCommand.Id != null) reachyFakeServer.SetHandPosition(rightGripperCommand);
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

        //         List<SerializableMotor> headMotors = reachyModel.GetCurrentMotorsState(headJoints);

        //         Dictionary<JointId, float> headTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < headMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = headMotors[i].name;
        //             float goal = Mathf.Rad2Deg * headMotors[i].goal_position;
        //             headTarget.Add(joint, goal);
        //         }

        //         reachy.HandleCommand(headTarget);
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

        //         List<SerializableMotor> leftArmMotors = reachyModel.GetCurrentMotorsState(leftJoints);

        //         Dictionary<JointId, float> leftArmTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < leftArmMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = leftArmMotors[i].name;
        //             float goal = Mathf.Rad2Deg * leftArmMotors[i].goal_position;
        //             leftArmTarget.Add(joint, goal);
        //         }

        //         reachy.HandleCommand(leftArmTarget);
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

        //         List<SerializableMotor> rightArmMotors = reachyModel.GetCurrentMotorsState(rightJoints);

        //         Dictionary<JointId, float> rightArmTarget = new Dictionary<JointId, float>();
        //         for (int i = 0; i < rightArmMotors.Count; i++)
        //         {
        //             joint = new JointId();
        //             joint.Name = rightArmMotors[i].name;
        //             float goal = Mathf.Rad2Deg * rightArmMotors[i].goal_position;
        //             rightArmTarget.Add(joint, goal);
        //         }

        //         reachy.HandleCommand(rightArmTarget);
        //     }

        //     protected override void SendJointsCommands(JointsCommand jointsCommand)
        //     {
        //         reachyFakeServer.SendJointsCommands(jointsCommand);
        //     }

    }
}

