using System.Collections.Generic;
using UnityEngine;

namespace Reachy2Controller
{
    [System.Serializable]
    public class Motor
    {
        public string name;
        public int uid;
        public GameObject gameObject;
        public float targetPosition;
        public float presentPosition;
    }

    public class Reachy2Controller : MonoBehaviour
    {
        public Motor[] motors;
        private Dictionary<string, Motor> name2motor;

        UnityEngine.Quaternion baseHeadRot;
        UnityEngine.Quaternion targetHeadRot;
        public Vector3 headOrientation;
        float headRotDuration;

        public GameObject l_arm { get; private set; }
        public GameObject r_arm { get; private set; }
        public GameObject head { get; private set; }
        public GameObject mobile_base { get; private set; }

        void Awake()
        {
            name2motor = new Dictionary<string, Motor>();

            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];
                m.uid = i;
                name2motor[m.name] = m;
            }

            headOrientation = new Vector3(0, 0, 0);

            l_arm = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(1).GetChild(1).gameObject;
            r_arm = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(1).GetChild(2).gameObject;
            head = transform.GetChild(0).GetChild(0).GetChild(3).GetChild(1).GetChild(3).gameObject;
            mobile_base = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        }

        void Update()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];

                JointController joint = m.gameObject.GetComponent<JointController>();
                joint.RotateTo(m.targetPosition);

                m.presentPosition = joint.GetPresentPosition();
            }
        }

        void SetMotorTargetPosition(string motorName, float targetPosition)
        {
            Motor motor = null;
            if (motorName.EndsWith("hand"))
            {
                float open_gripper = -135;
                float closed_gripper = 3;
                float low_range = -30;
                float high_range = 32;

                targetPosition = low_range + ((high_range - low_range) / (closed_gripper - open_gripper)) * (-targetPosition - open_gripper);

                string mimicName = motorName + "_mimic";
                string distalName = motorName + "_distal";
                string distalMimicName = distalName + "_mimic";
                SetMotorTargetPosition(mimicName, targetPosition);
                SetMotorTargetPosition(distalName, -targetPosition);
                SetMotorTargetPosition(distalMimicName, -targetPosition);
            }
            if (name2motor.TryGetValue(motorName, out motor))
            {
                motor.targetPosition = targetPosition;
            }
        }

        public void HandleCommand(Dictionary<string, float> commands)
        {
            bool containNeckCommand = false;
            foreach (KeyValuePair<string, float> kvp in commands)
            {
                string motorName;
                motorName = kvp.Key;
                SetMotorTargetPosition(motorName, kvp.Value);

                if (motorName == "head_neck_roll")
                {
                    containNeckCommand = true;
                    headOrientation[0] = kvp.Value;
                }
                if (motorName == "head_neck_pitch")
                {
                    containNeckCommand = true;
                    headOrientation[1] = kvp.Value;
                }
                if (motorName == "head_neck_yaw")
                {
                    containNeckCommand = true;
                    headOrientation[2] = -kvp.Value;
                }
            }
        }
    }
}