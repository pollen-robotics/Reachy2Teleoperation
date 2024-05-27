using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TeleopReachy
{

    [System.Serializable]
    public struct CubicParameters
    {
        public float a;
        public float b;
        public float c;
        public float d;


        public CubicParameters(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

    }
    
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
        public float recordInterval = 0.5f;
        private float timer = 0f;
        private Transform leftController;
        private Transform rightController;
        private Transform userTrackerTransform;
        public Quaternion rightNeutralOrientation;
        public Quaternion leftNeutralOrientation;
        public UnityEvent event_onWristCalib = new UnityEvent();
        private ControllersManager controllers;
        public UnityEvent event_NeutralPoseCaptured = new UnityEvent();
        public UnityEvent event_WristPoseCaptured = new UnityEvent();
        private List<string> capturedData = new List<string>();
        private bool buttonX;
        private int nbPosition ;

        
        //version dans la référence de la pose neutre 
        
        private Quaternion neutralOrientation;
        public List<Vector3> leftUserEulerAngles_neutralframe = new List<Vector3>();
        public List<Vector3> rightUserEulerAngles_neutralframe = new List<Vector3>();
        public List<Vector3> leftTargetEulerAngles_neutralframe = new List<Vector3>
        {

            new Vector3(0f,0f,0f),
            new Vector3(0f,-70f,0f),
            new Vector3(0f,90f,0f),
            new Vector3(25f,0f,0f),
            new Vector3(-30f,0f,0f),
            new Vector3(0f,0f,50f),
            new Vector3(0f,0f,-40f)

        };

        public List<Vector3> rightTargetEulerAngles_neutralframe = new List<Vector3>
        {
            new Vector3(0f,0f,0f),
            new Vector3(0f,70f,0f),
            new Vector3(0f,-90f,0f),
            new Vector3(25f,0f,0f),
            new Vector3(-30f,0f,0f),
            new Vector3(0f,0f,-50f),
            new Vector3(0f,0f,40f)

        };

        public List<Vector3> fakeLeftTargetEulerAngles_neutralframe = new List<Vector3>
        {
            new Vector3(0f,0f,0f),
            new Vector3(0f,-40f,0f),
            new Vector3(0f,40f,0f),
            new Vector3(10f,0f,0f),
            new Vector3(-10f,0f,0f),
            new Vector3(0f,0f,-30f),
            new Vector3(0f,0f,-30f)

        };

        public List<Vector3> fakeRightTargetEulerAngles_neutralframe = new List<Vector3>
        {
            new Vector3(0f,0f,0f),
            new Vector3(0f,40f,0f),
            new Vector3(0f,-40f,0f),
            new Vector3(10f,0f,0f),
            new Vector3(-10f,0f,0f),
            new Vector3(0f,0f,-30f),
            new Vector3(0f,0f,30f)

        };

        public List<LinearParameters> rightLinearParameters ;
        public List<LinearParameters> leftLinearParameters ;
        public List<LinearParameters> fakeRightLinearParameters ;
        public List<LinearParameters> fakeLeftLinearParameters ;
        private TrackedHandManager trackedHandManager;


        

        public void Start()
        {
            Debug.Log("[Wrist Calibration version User] Start");
            leftController = GameObject.Find("TrackedLeftHand").transform;
            rightController = GameObject.Find("TrackedRightHand").transform;
            nbPosition = 0;

            event_onWristCalib.AddListener(StartWristCalibration);
            controllers = ActiveControllerManager.Instance.ControllersManager;
            trackedHandManager= FindObjectOfType<TrackedHandManager>();
            neutralOrientation = trackedHandManager.neutralOrientation;
        }


        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= recordInterval && nbPosition < 8)
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX)
                {
                    event_onWristCalib.Invoke();
                }
            }

            if (nbPosition == 7)
            {
                
                leftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, leftTargetEulerAngles_neutralframe, 'x'));
                leftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, leftTargetEulerAngles_neutralframe, 'y'));
                leftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, leftTargetEulerAngles_neutralframe, 'z'));
                rightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, rightTargetEulerAngles_neutralframe, 'x'));
                rightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, rightTargetEulerAngles_neutralframe, 'y'));
                rightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, rightTargetEulerAngles_neutralframe, 'z'));

                fakeLeftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, fakeLeftTargetEulerAngles_neutralframe, 'x'));
                fakeLeftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, fakeLeftTargetEulerAngles_neutralframe, 'y'));
                fakeLeftLinearParameters.Add(LinearCoefficient_3points(leftUserEulerAngles_neutralframe, fakeLeftTargetEulerAngles_neutralframe, 'z'));
                fakeRightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, fakeRightTargetEulerAngles_neutralframe, 'x'));
                fakeRightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, fakeRightTargetEulerAngles_neutralframe, 'y'));
                fakeRightLinearParameters.Add(LinearCoefficient_3points(rightUserEulerAngles_neutralframe, fakeRightTargetEulerAngles_neutralframe, 'z'));
                
                for (int i = 0; i < rightLinearParameters.Count; i++)
                {
                    Debug.Log("Linear parameters left: " + leftLinearParameters[i].A + "=a "+ leftLinearParameters[i].b+" =b ");
                    Debug.Log("Linear parameters right: " + rightLinearParameters[i].A + "=a "+ rightLinearParameters[i].b+" =b " );
                }
               
                SavePoseData(); 
                event_WristPoseCaptured.Invoke();
                nbPosition ++;
                capturedData.Clear();
            }
        }

        public void StartWristCalibration()
        {
            Debug.Log("[Wrist Calibration] Start Calibration");
            userTrackerTransform = GameObject.Find("UserTracker").transform;
        
            if (nbPosition == 0)
            {
                rightNeutralOrientation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * rightController.rotation;
                leftNeutralOrientation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * leftController.rotation;
                event_NeutralPoseCaptured.Invoke();
                Debug.Log("[Wrist Calibration] Neutral Pose Captured");
            }

            
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;
            Vector3 leftHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(leftController.position);
            Debug.Log("[Wrist Calibration] Capturing Pose");
            Quaternion leftHandRotation = UnityEngine.Quaternion.Inverse(neutralOrientation)*UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * leftController.rotation;
            Vector3 leftHandEulerAngles = leftHandRotation.eulerAngles;

            Vector3 rightHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(rightController.position);
            Quaternion rightHandRotation = UnityEngine.Quaternion.Inverse(neutralOrientation)* UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * rightController.rotation;
            Vector3 rightHandEulerAngles = rightHandRotation.eulerAngles;

            nbPosition++;
            buttonX = false;
            timer = 0f;

            Debug.Log("[Wrist Calibration] Position " + nbPosition);
            capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
            leftUserEulerAngles_neutralframe.Add(leftHandEulerAngles);
            rightUserEulerAngles_neutralframe.Add(rightHandEulerAngles);
        }


        public void SavePoseData()
        {
            Debug.Log("[Wrist Calibration] Saving Data");
            string path = "C:/Users/robot/Dev/WristCalibrationData_userframe.csv";
            string dataToAppend = string.Join("\n", capturedData) + "\n";

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.Write(dataToAppend);
            }
        }

        public LinearParameters LinearCoefficient_2points(List<Vector3> xValues, List<Vector3> yValues, char component)
        {
            float x1=0, x2=0, y1= 0, y2=0 ;
            switch (component)
            {
                case 'x':
                    x1 = NormalizeAngle(xValues[3].x);
                    x2 = NormalizeAngle(xValues[4].x);
                    y1 = NormalizeAngle(yValues[3].x);
                    y2 = NormalizeAngle(yValues[4].x);
                    break;
                case 'y':
                    x1 = NormalizeAngle(xValues[1].y);
                    x2 = NormalizeAngle(xValues[2].y);
                    y1 = NormalizeAngle(yValues[1].y);
                    y2 = NormalizeAngle(yValues[2].y);
                    break;
                case 'z':
                    x1 = NormalizeAngle(xValues[5].z);
                    x2 = NormalizeAngle(xValues[6].z);
                    y1 = NormalizeAngle(yValues[5].z);
                    y1 = NormalizeAngle(yValues[6].z);
                    break;
            }
            
            float A = Math.Abs((y2 - y1) / (x2 - x1));
            float B = y1 - A * x1;
            LinearParameters linearParameters = new LinearParameters(A, B);
            return linearParameters;
        }
    
        public static LinearParameters LinearCoefficient_3points(List<Vector3> xValues, List<Vector3> yValues, char component)
        {
            float x1=0, x2=0, x3=0, y1= 0, y2=0 , y3 = 0;
            switch (component)
            {
                case 'x':
                    x1 = NormalizeAngle(xValues[0].x);
                    x2 = NormalizeAngle(xValues[3].x);
                    x3 = NormalizeAngle(xValues[4].x);
                    y1 = NormalizeAngle(yValues[0].x);
                    y2 = NormalizeAngle(yValues[3].x);
                    y3 = NormalizeAngle(yValues[4].x);
                    break;
                case 'y':
                    x1 = NormalizeAngle(xValues[0].y);
                    x2 = NormalizeAngle(xValues[1].y);
                    x3 = NormalizeAngle(xValues[2].y);
                    y1 = NormalizeAngle(yValues[0].y);
                    y2 = NormalizeAngle(yValues[1].y);
                    y3 = NormalizeAngle(yValues[2].y);
                    break;
                case 'z':
                    x1 = NormalizeAngle(xValues[0].z);
                    x2 = NormalizeAngle(xValues[5].z);
                    x3 = NormalizeAngle(xValues[6].z);
                    y1 = NormalizeAngle(yValues[0].z);
                    y2 = NormalizeAngle(yValues[5].z);
                    y3 = NormalizeAngle(yValues[6].z);
                    break;
            }
            // Calcul des coefficients pour les deux premiers points
            float A1 = (y2 - y1) / (x2 - x1);
            float B1 = y1 - A1 * x1;

            // Calcul des coefficients pour les deux derniers points
            float A2 = (y3 - y2) / (x3 - x2);
            float B2 = y2 - A2 * x2;

            // Moyenne des deux valeurs pour A et B
            float A = (A1 + A2) / 2;
            float B = (B1 + B2) / 2;

            LinearParameters linearParameters = new LinearParameters(A, B);
            return linearParameters;
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle < -180) angle += 360;
            else if (angle > 180) angle -= 360;
            return angle;
        }


    }
}

