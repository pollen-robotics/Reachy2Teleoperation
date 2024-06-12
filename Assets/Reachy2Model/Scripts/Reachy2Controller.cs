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
        public bool isCompliant;
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
            // baseHeadRot = head.transform.localRotation;
        }

        void Update()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];

                // if (!m.name.StartsWith("head_neck"))
                // {
                    JointController joint = m.gameObject.GetComponent<JointController>();
                    joint.RotateTo(m.targetPosition);

                    m.presentPosition = joint.GetPresentPosition();
                // }
                // else
                // {
                //     m.presentPosition = m.targetPosition;
                // }
            }

            // UpdateHeadOrientation();
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

            // if (containNeckCommand)
            // {
            //     //UnityEngine.Quaternion.Euler(); not properly working. Manually creating rotation
            //     UnityEngine.Quaternion euler_request = UnityEngine.Quaternion.Euler(Vector3.forward * headOrientation[2]) * UnityEngine.Quaternion.Euler(Vector3.up * -headOrientation[0]) * UnityEngine.Quaternion.Euler(Vector3.right * headOrientation[1]);
            //     HandleHeadOrientation(euler_request);
            // }
        }

        //     public void HandleCompliancy(Dictionary<JointId, bool> commands)
        //     {
        //         foreach (KeyValuePair<JointId, bool> kvp in commands)
        //         {
        //             string motorName;
        //             switch (kvp.Key.IdCase)
        //             {
        //                 case JointId.IdOneofCase.Name:
        //                     motorName = kvp.Key.Name;
        //                     break;
        //                 case JointId.IdOneofCase.Uid:
        //                     motorName = motors[kvp.Key.Uid].name;
        //                     break;
        //                 default:
        //                     motorName = kvp.Key.Name;
        //                     break;
        //             }
        //             SetMotorCompliancy(motorName, kvp.Value);
        //         }
        //     }

        //     public List<SerializableMotor> GetCurrentMotorsState(Dictionary<JointId, JointField> request)
        //     {
        //         List<SerializableMotor> motorsList = new List<SerializableMotor>();
        //         foreach (KeyValuePair<JointId, JointField> kvp in request)
        //         {
        //             Motor m;
        //             float position;
        //             float target_position;
        //             bool compliancy;
        //             switch (kvp.Key.IdCase)
        //             {
        //                 case JointId.IdOneofCase.Name:
        //                     m = name2motor[kvp.Key.Name];
        //                     position = m.presentPosition;
        //                     target_position = m.targetPosition;
        //                     compliancy = m.isCompliant;
        //                     if (!name2motor[kvp.Key.Name].isDirect)
        //                     {
        //                         position *= -1;
        //                         target_position *= -1;
        //                     }
        //                     position -= name2motor[kvp.Key.Name].offset;
        //                     target_position -= name2motor[kvp.Key.Name].offset;
        //                     break;
        //                 case JointId.IdOneofCase.Uid:
        //                     m = motors[kvp.Key.Uid];
        //                     position = m.presentPosition;
        //                     target_position = m.targetPosition;
        //                     compliancy = m.isCompliant;
        //                     if (!motors[kvp.Key.Uid].isDirect)
        //                     {
        //                         position *= -1;
        //                         target_position *= -1;
        //                     }
        //                     position -= motors[kvp.Key.Uid].offset;
        //                     target_position -= motors[kvp.Key.Uid].offset;
        //                     break;
        //                 default:
        //                     m = name2motor[kvp.Key.Name];
        //                     position = m.presentPosition;
        //                     target_position = m.targetPosition;
        //                     compliancy = m.isCompliant;
        //                     if (!name2motor[kvp.Key.Name].isDirect)
        //                     {
        //                         position *= -1;
        //                         target_position *= -1;
        //                     }
        //                     position -= name2motor[kvp.Key.Name].offset;
        //                     target_position -= name2motor[kvp.Key.Name].offset;
        //                     break;
        //             }
        //             motorsList.Add(new SerializableMotor() { name = m.name, uid = m.uid, present_position = Mathf.Deg2Rad * position, goal_position = Mathf.Deg2Rad * target_position, isCompliant = compliancy });
        //         }
        //         return motorsList;
        //     }

        // public void HandleHeadOrientation(UnityEngine.Quaternion q)
        // {
        //     targetHeadRot = q;
        // }

        // void UpdateHeadOrientation()
        // {
        //     head.transform.localRotation = targetHeadRot;
        // }
    }
}