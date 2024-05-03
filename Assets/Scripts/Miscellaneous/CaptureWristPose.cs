using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TeleopReachy
{
    [System.Serializable]
    public struct LinearParameters
    {
        public float A;
        public float b;

        public LinearParameters(float A, float b)
        {
            this.A = A;
            this.b = b;
        }
    }
    public class CaptureWristPose : Singleton<CaptureWristPose>
    {
        public float recordInterval = 1f;
        private float timer = 0f;
        private Transform leftController;
        private Transform rightController;
        private Transform userTrackerTransform;
        public Vector3 rightMinAngles = new Vector3(-100f, -100f, -100f);
        public Vector3 rightMaxAngles = new Vector3(100f, 100f, 100f);
        public Vector3 leftMinAngles = new Vector3(-100f, -100f, -100f);
        public Vector3 leftMaxAngles = new Vector3(100f, 100f, 100f);
        public Quaternion rightNeutralOrientation;
        public Quaternion leftNeutralOrientation;
        public List<LinearParameters> rightLinearParameters = new List<LinearParameters>();
        public List<LinearParameters> leftLinearParameters = new List<LinearParameters>();
        private List<string> capturedData = new List<string>();
        private bool buttonX;
        private int nbPosition = 0;
        public UnityEvent event_onStartWristCalib;
        private ControllersManager controllers;
        public UnityEvent event_NeutralPoseCaptured = new UnityEvent();
        public UnityEvent event_WristPoseCaptured = new UnityEvent();


        public void Start()
        {
            Debug.Log("[Wrist Calibration version User] Start");
            leftController = GameObject.Find("LeftHand Controller").transform;
            rightController = GameObject.Find("RightHand Controller").transform;
            event_onStartWristCalib = new UnityEvent();
            event_onStartWristCalib.AddListener(StartWristCalibration);
            controllers = ActiveControllerManager.Instance.ControllersManager;
            
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= recordInterval && nbPosition < 8)
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX)
                {
                    event_onStartWristCalib.Invoke();
                }
            }

            if (nbPosition == 7)
            {
                SavePoseData();
                GetRescalingParameters();
                event_WristPoseCaptured.Invoke();
                nbPosition ++;
                capturedData.Clear();
            }
        }

        public void StartWristCalibration()
        {

            userTrackerTransform = GameObject.Find("UserTracker").transform;
            Debug.Log("[Wrist Calibration] Start Calibration");
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;

            Vector3 leftHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(leftController.position);
            Quaternion leftHandRotation = Quaternion.Inverse(userTrackerTransform.rotation) * leftController.rotation;
            Vector3 leftHandEulerAngles = leftHandRotation.eulerAngles;
            leftHandEulerAngles.x = NormalizeAngle(leftHandEulerAngles.x);
            leftHandEulerAngles.y = NormalizeAngle(leftHandEulerAngles.y);
            leftHandEulerAngles.z = NormalizeAngle(leftHandEulerAngles.z);

            Vector3 rightHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(rightController.position);
            Quaternion rightHandRotation = Quaternion.Inverse(userTrackerTransform.rotation) * rightController.rotation;
            Vector3 rightHandEulerAngles = rightHandRotation.eulerAngles;
            rightHandEulerAngles.x = NormalizeAngle(rightHandEulerAngles.x);
            rightHandEulerAngles.y = NormalizeAngle(rightHandEulerAngles.y);
            rightHandEulerAngles.z = NormalizeAngle(rightHandEulerAngles.z);

            nbPosition++;
            buttonX = false;
            timer = 0f;

            switch (nbPosition)
            {
                case 1:
                    rightNeutralOrientation = rightHandRotation;
                    leftNeutralOrientation = leftHandRotation;
                    event_NeutralPoseCaptured.Invoke();
                    Debug.Log("Position 1");
                    break;
                case 2:
                    rightMaxAngles.z = rightHandEulerAngles.z;
                    leftMinAngles.z = leftHandEulerAngles.z;
                    Debug.Log("Position 2");
                    break;
                case 3:
                    rightMinAngles.z = rightHandEulerAngles.z;
                    leftMaxAngles.z = leftHandEulerAngles.z;
                    Debug.Log("Position 3");
                    break;
                case 4:
                    rightMinAngles.x = rightHandEulerAngles.x;
                    leftMinAngles.x = leftHandEulerAngles.x;
                    Debug.Log("Position 4");
                    break;
                case 5:
                    rightMaxAngles.x = rightHandEulerAngles.x;
                    leftMaxAngles.x = leftHandEulerAngles.x;
                    Debug.Log("Position 5");
                    break;
                case 6:
                    rightMaxAngles.y = rightHandEulerAngles.y;
                    leftMinAngles.y = leftHandEulerAngles.y;
                    Debug.Log("Position 6");
                    break;
                case 7:
                    rightMinAngles.y = rightHandEulerAngles.y;
                    leftMaxAngles.y = leftHandEulerAngles.y;
                    Debug.Log("Position 7");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    break;
            }
        }


        public void SavePoseData()
        {
            Debug.Log("[Wrist Calibration] Saving Data");
            string path = "C:/Users/robot/Dev/WristCalibrationData.csv";
            string dataToAppend = string.Join("\n", capturedData);
            dataToAppend += "\n";
            
            File.AppendAllText(path, dataToAppend);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f)
            {
                angle -= 360f;
            }
            while (angle < -180f)
            {
                angle += 360f;
            }
            return angle;
        }

        public void GetRescalingParameters()
        {
            rightLinearParameters.Add(LinearCoefficient(rightMinAngles.x, rightNeutralOrientation.eulerAngles.x, -100f, 0f));
            rightLinearParameters.Add(LinearCoefficient(rightMinAngles.y, rightNeutralOrientation.eulerAngles.y, -40f, 0f));
            rightLinearParameters.Add(LinearCoefficient(rightMinAngles.z, rightNeutralOrientation.eulerAngles.z, -50f, 0f));
            rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.x, rightMaxAngles.x, 0f, 100f));
            rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.y, rightMaxAngles.y, 0f, 20f));
            rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.z, rightMaxAngles.z, 0f, 50f));

            leftLinearParameters.Add(LinearCoefficient(leftMinAngles.x, leftNeutralOrientation.eulerAngles.x, -100f, 0f));
            leftLinearParameters.Add(LinearCoefficient(leftMinAngles.y, leftNeutralOrientation.eulerAngles.y, -40f, 0f));
            leftLinearParameters.Add(LinearCoefficient(leftMinAngles.z, leftNeutralOrientation.eulerAngles.z, -50f, 0f));
            leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.x, leftMaxAngles.x, 0f, 100f));
            leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.y, leftMaxAngles.y, 0f, 20f));
            leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.z, leftMaxAngles.z, 0f, 50f));
            
        }

        public LinearParameters LinearCoefficient(float x1, float x2, float y1, float y2)
        {
            float A = (y2 - y1) / (x2 - x1);
            float B = y1 - A * x1;
            LinearParameters linearParameters = new LinearParameters(A, B);
            return linearParameters;
        }


    }
}

