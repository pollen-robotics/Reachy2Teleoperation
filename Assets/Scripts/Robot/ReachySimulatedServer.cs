using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Component.Orbita2D;
using Reachy.Kinematics;
using Component.DynamixelMotor;


namespace Reachy2Controller
{
    public class ReachySimulatedServer : MonoBehaviour
    {
        public static Reachy2Controller reachy;

        private Dictionary<string, float> present_position;

        enum ArmSide
        {
            Left,
            Right,
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            [DllImport("Arm_kinematics.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        [DllImport("libArm_kinematics.so", CallingConvention = CallingConvention.Cdecl)]
        // #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        //     [DllImport("libArm_kinematics.dylib", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_ANDROID
            [DllImport("libArm_kinematics.android.so", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void setup();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            [DllImport("Arm_kinematics.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        [DllImport("libArm_kinematics.so", CallingConvention = CallingConvention.Cdecl)]
        // #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        //     [DllImport("libArm_kinematics.dylib", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_ANDROID
            [DllImport("libArm_kinematics.android.so", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void inverse(ArmSide side, double[] M, double[] q);

        void Start()
        {
            reachy = GameObject.Find("Reachy2Simulated").GetComponent<Reachy2Controller>();
            setup(); // Setup Arm_kinematics

            present_position = new Dictionary<string, float>();
        }

        void Update()
        {
            reachy.HandleCommand(present_position);
        }

        public void SendArmCommand(ArmCartesianGoal armGoal)
        {
            ArmIKRequest ikRequest = new ArmIKRequest
            {
                Id = armGoal.Id,
                Target = new ArmEndEffector
                {
                    Pose = armGoal.GoalPose,
                }
            };
            List<double> armSolution = ComputeArmIK(ikRequest);

            if (armGoal.Id.Name == "l_arm")
            {
                present_position["l_arm_shoulder_axis_1"] = Mathf.Rad2Deg * (float)armSolution[0];
                present_position["l_arm_shoulder_axis_2"] = Mathf.Rad2Deg * (float)armSolution[1];
                present_position["l_arm_elbow_axis_1"] = Mathf.Rad2Deg * (float)armSolution[2];
                present_position["l_arm_elbow_axis_2"] = Mathf.Rad2Deg * (float)armSolution[3];
                present_position["l_arm_wrist_roll"] = Mathf.Rad2Deg * (float)armSolution[4];
                present_position["l_arm_wrist_pitch"] = Mathf.Rad2Deg * (float)armSolution[5];
                present_position["l_arm_wrist_yaw"] = Mathf.Rad2Deg * (float)armSolution[6];
            }
            else
            {
                present_position["r_arm_shoulder_axis_1"] = Mathf.Rad2Deg * (float)armSolution[0];
                present_position["r_arm_shoulder_axis_2"] = Mathf.Rad2Deg * (float)armSolution[1];
                present_position["r_arm_elbow_axis_1"] = Mathf.Rad2Deg * (float)armSolution[2];
                present_position["r_arm_elbow_axis_2"] = Mathf.Rad2Deg * (float)armSolution[3];
                present_position["r_arm_wrist_roll"] = Mathf.Rad2Deg * (float)armSolution[4];
                present_position["r_arm_wrist_pitch"] = Mathf.Rad2Deg * (float)armSolution[5];
                present_position["r_arm_wrist_yaw"] = Mathf.Rad2Deg * (float)armSolution[6];
            }
        }

        public void SendNeckCommand(NeckJointGoal neckGoal)
        {
            UnityEngine.Quaternion headRotation = new UnityEngine.Quaternion(
                (float)neckGoal.JointsGoal.Rotation.Q.Y,
                -(float)neckGoal.JointsGoal.Rotation.Q.Z,
                -(float)neckGoal.JointsGoal.Rotation.Q.X,
                (float)neckGoal.JointsGoal.Rotation.Q.W);

            Vector3 neck_commands = headRotation.eulerAngles;

            if (neck_commands[2] > 180) neck_commands[2] = neck_commands[2] - 360;
            if (neck_commands[0] > 180) neck_commands[0] = neck_commands[0] - 360;
            if (neck_commands[1] > 180) neck_commands[1] = neck_commands[1] - 360;

            present_position["head_neck_roll"] = -neck_commands[2];
            present_position["head_neck_pitch"] = neck_commands[0];
            present_position["head_neck_yaw"] = -neck_commands[1];
        }

        public void SetHandPosition(HandPositionRequest gripperPosition)
        {
            float opening = (float)gripperPosition.Position.ParallelGripper.OpeningPercentage;
            float open_gripper = 135;
            float closed_gripper = -3;

            float targetPosition = (1 - opening) * closed_gripper + opening * open_gripper;
            present_position[gripperPosition.Id.Name] = targetPosition;
        }

        public void SetAntennaPosition(DynamixelMotorCommand antennaCommand)
        {
            present_position[antennaCommand.Id.Name] = Mathf.Rad2Deg * (float)antennaCommand.GoalPosition;
        }

        private List<double> ComputeArmIK(ArmIKRequest ikRequest)
        {
            double[] M = new double[16];
            if (ikRequest.Target.Pose.Data.Count != 16)
            {
                return new List<double> { 0, 0, 0, 0, 0, 0, 0 };
            }

            for (int i = 0; i < 16; i++)
            {
                M[i] = ikRequest.Target.Pose.Data[i];
            }
            double[] q = new double[7];

            ArmSide side;
            if (ikRequest.Id.Name == "l_arm")
            {
                side = ArmSide.Left;
            }
            else { side = ArmSide.Right; }

            inverse(side, M, q);

            List<double> listq = new List<double>(q);

            return listq;
        }
    }
}