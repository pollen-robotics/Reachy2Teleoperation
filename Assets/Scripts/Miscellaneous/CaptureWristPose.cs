using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TeleopReachy
{
    public class CaptureWristPose : MonoBehaviour
    {
        public float recordInterval = 1f;
        private float timer = 0f;
        private Transform leftController;
        private Transform rightController;
        private Transform userTrackerTransform;
        private List<string> capturedData = new List<string>();
        private bool buttonX;
        public UnityEvent event_onStartWristCalib;
        private ControllersManager controllers;
        private RobotStatus robotStatus ;


        public void Start()
        {
            Debug.Log("[Wrist Calibration] Start");
            robotStatus = RobotDataManager.Instance.RobotStatus;
            leftController = GameObject.Find("LeftHand Controller").transform;
            rightController = GameObject.Find("RightHand Controller").transform;
            userTrackerTransform = GameObject.Find("UserTracker").transform;
            event_onStartWristCalib = new UnityEvent();
            event_onStartWristCalib.AddListener(StartWristCalibration);
            robotStatus.event_OnStopTeleoperation.AddListener(SavePoseData);
            controllers = ActiveControllerManager.Instance.ControllersManager;
            
        }

        public void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            timer += Time.deltaTime;
            if (timer >= recordInterval)
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX)
                {
                    event_onStartWristCalib.Invoke();
                }
            }
                
        }

        public void StartWristCalibration()
        {
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

            // Ajouter les données de position, rotation et rotation en Euler sur la même ligne
            capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");

            // Réinitialiser le bouton et le minuteur
            buttonX = false;
            timer = 0f;
        }


        public void SavePoseData()
        {
            Debug.Log("[Wrist Calibration] Saving Data");
            // dans le dossier Dev de l'ordi
            string path = "C:/Users/robot/Dev/WristCalibrationData.csv";
            File.WriteAllLines(path, capturedData);
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

            
    }



}

