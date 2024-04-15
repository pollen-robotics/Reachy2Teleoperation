using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Collections;


namespace TeleopReachy
{
    public class VRPoseRecorder : MonoBehaviour
    {
        public float recordInterval = 0.2f;
        private float timer = 0f;
        private RobotStatus robotStatus ;
        private Transform humanBase;
        private Transform headset;
        private Transform leftController;
        private Transform rightController;

        private List<VRPoseData> poseDataList = new List<VRPoseData>();

        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            headset = GameObject.Find("Main Camera").transform;
            leftController = GameObject.Find("LeftHand Controller").transform;
            rightController = GameObject.Find("RightHand Controller").transform;
            humanBase = GameObject.Find("Floor").transform;
            robotStatus.event_OnStopTeleoperation.AddListener(SavePoseData);
        }

        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                timer += Time.deltaTime;

                if (timer >= recordInterval)
                {
                    timer = 0f;
                    RecordPose();
                }
            }
        }

        void RecordPose()
        {
            Matrix4x4 humanBaseInverse = humanBase.worldToLocalMatrix;

            Vector3 headPosition = ConvertPosition(humanBaseInverse.MultiplyPoint(headset.position));
            Quaternion headRotation = ConvertRotation(headset.rotation);

            Vector3 leftControllerPosition = ConvertPosition(humanBaseInverse.MultiplyPoint(leftController.position));
            Quaternion leftControllerRotation = ConvertRotation(leftController.rotation);

            Vector3 rightControllerPosition = ConvertPosition(humanBaseInverse.MultiplyPoint(rightController.position));
            Quaternion rightControllerRotation = ConvertRotation(rightController.rotation);

            VRPoseData poseData = new VRPoseData(Time.time, headPosition, headRotation, leftControllerPosition, leftControllerRotation, rightControllerPosition, rightControllerRotation);
            poseDataList.Add(poseData);
        }

        void SavePoseData()
        {
            string directoryPath = @"C:\Users\robot\Dev\test_calib\";
            string timeStamp = DateTime.Now.ToString("MMdd_HHmm");
            string jsonFileName = Path.Combine(directoryPath, timeStamp + "_VRPoseData.json");
            string json = JsonConvert.SerializeObject(poseDataList.ToArray(), Formatting.Indented);
            File.WriteAllText(jsonFileName, json);
            Debug.Log("Pose data saved to " + jsonFileName);
        }

        Vector3 ConvertPosition(Vector3 position)
        {
            return new Vector3(position.z, -position.x, position.y);
        }

        Quaternion ConvertRotation(Quaternion rotation)
        {
            return new Quaternion(-rotation.z, rotation.x, -rotation.y, rotation.w);
        }
    }

    [System.Serializable]
    public class VRPoseData
    {
        public float timestamp;
        public float[] headPosition;
        public float[] headRotation;
        public float[] leftControllerPosition;
        public float[] leftControllerRotation;
        public float[] rightControllerPosition;
        public float[] rightControllerRotation;

        public VRPoseData(float timestamp, Vector3 headPosition, Quaternion headRotation, Vector3 leftControllerPosition, Quaternion leftControllerRotation, Vector3 rightControllerPosition, Quaternion rightControllerRotation)
        {
            this.timestamp = timestamp;
            this.headPosition = new float[] { headPosition.x, headPosition.y, headPosition.z };
            this.headRotation = new float[] { headRotation.x, headRotation.y, headRotation.z, headRotation.w };
            this.leftControllerPosition = new float[] { leftControllerPosition.x, leftControllerPosition.y, leftControllerPosition.z };
            this.leftControllerRotation = new float[] { leftControllerRotation.x, leftControllerRotation.y, leftControllerRotation.z, leftControllerRotation.w };
            this.rightControllerPosition = new float[] { rightControllerPosition.x, rightControllerPosition.y, rightControllerPosition.z };
            this.rightControllerRotation = new float[] { rightControllerRotation.x, rightControllerRotation.y, rightControllerRotation.z, rightControllerRotation.w };
        }
    }

}

