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
        private List<Vector3> leftHandEulerAnglesList = new List<Vector3>();
        private List<Vector3> rightHandEulerAnglesList = new List<Vector3>();
        public Vector3 rightMinAngles;
        public Vector3 rightMaxAngles;
        public Vector3 leftMinAngles;
        public Vector3 leftMaxAngles;
        public Quaternion rightNeutralOrientation;
        public Quaternion leftNeutralOrientation;
        public List<LinearParameters> rightLinearParameters = new List<LinearParameters>();
        public List<LinearParameters> leftLinearParameters = new List<LinearParameters>();
        private List<string> capturedData = new List<string>();
        private bool buttonX;
        private int nbPosition = 0;
        public UnityEvent event_onStartWristCalib;
        private ControllersManager controllers;
        private RobotStatus robotStatus ;
        public UnityEvent event_NeutralPoseCaptured = new UnityEvent();
        public UnityEvent event_WristPoseCaptured = new UnityEvent();


        public void Start()
        {
            Debug.Log("[Wrist Calibration version User] Start");
            leftController = GameObject.Find("LeftHand Controller").transform;
            rightController = GameObject.Find("RightHand Controller").transform;
            //leftController = GameObject.Find("TrackedLeftHand").transform;
            //rightController = GameObject.Find("TrackedRightHand").transform;
            event_onStartWristCalib = new UnityEvent();
            event_onStartWristCalib.AddListener(StartWristCalibration);
            //robotStatus.event_OnStopTeleoperation.AddListener(SavePoseData);
            controllers = ActiveControllerManager.Instance.ControllersManager;
            
        }

        public void Update()
        {
            //if (robotStatus.IsRobotTeleoperationActive())
            timer += Time.deltaTime;
            if (timer >= recordInterval)
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX)
                {
                    event_onStartWristCalib.Invoke();
                }
            }

            if (nbPosition ==7)
            {
                (leftMinAngles, leftMaxAngles) = GetMinAndMax(leftHandEulerAnglesList);
                (rightMinAngles, rightMaxAngles) = GetMinAndMax(rightHandEulerAnglesList);
                Debug.Log("Left Min Angles: " + leftMinAngles + " Left Max Angles: " + leftMaxAngles);
                Debug.Log("Right Min Angles: " + rightMinAngles + " Right Max Angles: " + rightMaxAngles);
                SavePoseData();
                GetRescalingParameters();
                event_WristPoseCaptured.Invoke();
                nbPosition = 0;
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
            Debug.Log("Nb position: " + nbPosition);
            buttonX = false;

            leftHandEulerAnglesList.Add(leftHandEulerAngles);
            rightHandEulerAnglesList.Add(rightHandEulerAngles);

            if (nbPosition == 1)
            {
                rightNeutralOrientation = rightHandRotation;
                leftNeutralOrientation = leftHandRotation;
                event_NeutralPoseCaptured.Invoke();

            }

            // Ajouter les données de position, rotation et rotation en Euler sur la même ligne
            capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");

            // Réinitialiser le bouton et le minuteur
            timer = 0f;
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

        public (Vector3, Vector3) GetMinAndMax (List<Vector3> list)
        {

            float maxX = list.Max(angle => angle.x);
            float minX = list.Min(angle => angle.x);
            float maxY = list.Max(angle => angle.y);
            float minY = list.Min(angle => angle.y);
            float maxZ = list.Max(angle => angle.z);
            float minZ = list.Min(angle => angle.z);

            Vector3 min = new Vector3(minX, minY, minZ);
            Vector3 max = new Vector3(maxX, maxY, maxZ);

            return (min, max);
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

