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
        public List<List<LinearParameters>> rightLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> leftLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> fakeRightLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> fakeLeftLinearParameters = new List<List<LinearParameters>>();

        public List<List<float>> rightLimitValues = new List<List<float>>();
        public List<List<float>> leftLimitValues = new List<List<float>>();
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
            leftController = GameObject.Find("TrackedLeftHand").transform;
            rightController = GameObject.Find("TrackedRightHand").transform;
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
                GetFakeRescalingParameters(); // à enlever si on garde la calibration
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
            Quaternion leftHandRotation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * leftController.rotation;
            Vector3 leftHandEulerAngles = leftHandRotation.eulerAngles;

            Vector3 rightHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(rightController.position);
            Quaternion rightHandRotation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * rightController.rotation;
            Vector3 rightHandEulerAngles = rightHandRotation.eulerAngles;


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
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    break;
                case 2:
                    rightMaxAngles.z = rightHandEulerAngles.z;
                    leftMinAngles.z = leftHandEulerAngles.z;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 2");
                    break;
                case 3:
                    rightMinAngles.z = rightHandEulerAngles.z;
                    leftMaxAngles.z = leftHandEulerAngles.z;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 3");
                    break;
                case 4:
                    rightMinAngles.x = rightHandEulerAngles.x;
                    leftMinAngles.x = leftHandEulerAngles.x;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 4");
                    break;
                case 5:
                    rightMaxAngles.x = rightHandEulerAngles.x;
                    leftMaxAngles.x = leftHandEulerAngles.x;
                    Debug.Log("Position 5");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    break;
                case 6:
                    rightMaxAngles.y = rightHandEulerAngles.y;
                    leftMinAngles.y = leftHandEulerAngles.y;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 6");
                    break;
                case 7:
                    rightMinAngles.y = rightHandEulerAngles.y;
                    leftMaxAngles.y = leftHandEulerAngles.y;
                    Debug.Log("Position 7");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("rightminangles ="+ rightMinAngles+ "rightmaxangles ="+ rightMaxAngles+ "leftminangles ="+ leftMinAngles+ "leftmaxangles ="+ leftMaxAngles);
                    break;
            }
        }


        public void SavePoseData()
        {
            Debug.Log("[Wrist Calibration] Saving Data");
            string path = "C:/Users/robot/Dev/WristCalibrationData.csv";
            string dataToAppend = string.Join("\n", capturedData) + "\n";

            // Utilisation d'un bloc using pour garantir que le StreamWriter est correctement fermé
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.Write(dataToAppend);
            }
        }

        public void GetRescalingParameters()
        {
            rightLinearParameters.Add(Get3LinearParameters(rightMinAngles.x, rightMaxAngles.x, 290f, 310f, 'x'));
            rightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f, 'y'));
            rightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f, 'z'));

            rightLimitValues.Add(GetLimitValues(rightMinAngles.x, rightMaxAngles.x));
            rightLimitValues.Add(GetLimitValues(rightMaxAngles.y - 360, rightMinAngles.y));
            rightLimitValues.Add(GetLimitValues(rightMaxAngles.z - 360, rightMinAngles.z));

            Debug.Log("rightlimitvalues ="+ rightLimitValues[0] + rightLimitValues[1] + rightLimitValues[2]);


            //rightLinearParameters.Add(LinearCoefficient(rightMinAngles.x, rightMaxAngles.x, 290f, 310f));
            //rightLinearParameters.Add(LinearCoefficient(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f));
            //rightLinearParameters.Add(LinearCoefficient(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f));


            // rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.x, rightMaxAngles.x, 0f, 20f));
            // rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.y, rightMaxAngles.y, 0f, 50f));
            // float x0, x360, y0, y360, z0, z360;
            // (z0, z360) = GetExtremum(rightLinearParameters[2]);
            // rightLinearParameters.Add(LinearCoefficient(z0, z360, 0f, 360f));

            leftLinearParameters.Add(Get3LinearParameters(leftMinAngles.x, leftMaxAngles.x, 290f, 310f, 'x'));
            leftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.y - 360, leftMinAngles.y, -60f, 60f, 'y'));
            leftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.z - 360, leftMinAngles.z, -90f, 90f, 'z'));

            leftLimitValues.Add(GetLimitValues(leftMinAngles.x, leftMaxAngles.x));
            leftLimitValues.Add(GetLimitValues(leftMaxAngles.y - 360, leftMinAngles.y));
            leftLimitValues.Add(GetLimitValues(leftMaxAngles.z - 360, leftMinAngles.z));

            Debug.Log("leftlimitvalues ="+ leftLimitValues[0] + leftLimitValues[1] + leftLimitValues[2]);

            // leftLinearParameters.Add(LinearCoefficient(leftMinAngles.x, leftMaxAngles.x, 290f, 310f));
            // leftLinearParameters.Add(LinearCoefficient(leftMaxAngles.y - 360 , leftMinAngles.y, -60f, 60f));
            // leftLinearParameters.Add(LinearCoefficient(leftMinAngles.z, leftMaxAngles.z, -90f, 90f));
            
            // leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.x, leftMaxAngles.x, 0f, 20f));
            // leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.y, leftMaxAngles.y, 0f, 50f));
            // (z0, z360) = GetExtremum(leftLinearParameters[2]);
            // leftLinearParameters.Add(LinearCoefficient(z0, z360, 0f, 360f));
            
        }

        public void GetFakeRescalingParameters() // à enlever si on garde la calibration
        {
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMinAngles.x, rightMaxAngles.x, 290f, 310f, 'x'));
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f, 'y'));
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f, 'z'));

            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMinAngles.x, leftMaxAngles.x, 290f, 310f, 'x'));
            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.y - 360, leftMinAngles.y, -60f, 60f, 'y'));
            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.z - 360, leftMinAngles.z, -90f, 90f, 'z'));

            
        }

        public LinearParameters LinearCoefficient(float x1, float x2, float y1, float y2)
        {
            float A = (y2 - y1) / (x2 - x1);
            float B = y1 - A * x1;
            LinearParameters linearParameters = new LinearParameters(A, B);
            return linearParameters;
        }

        public List<LinearParameters> Get3LinearParameters(float x1, float x2, float y1, float y2, char mode)
        {
            List<LinearParameters> parameters = new List<LinearParameters>();

            LinearParameters intervalParameters = LinearCoefficient(x1, x2, y1, y2);
            parameters.Add(intervalParameters);

            LinearParameters negParameters = new LinearParameters(0, 0);
            LinearParameters posParameters = new LinearParameters(0, 0);

            if (mode == 'x') 
            {
                negParameters = LinearCoefficient(0, x1, 0, y1);
                posParameters = LinearCoefficient(x2, 360, y2, 360);
            } 
            else 
            {
                float rangeX = 360 - (x2 - x1);
                float medianX = (x1 + x2) / 2;
                float rangeY = 360 - (y2 - y1);
                float medianY = (y1 + y2) / 2;
                negParameters = LinearCoefficient(x1 - rangeX/2, x1, y1 - rangeY/2, y1);
                posParameters = LinearCoefficient(x2, x2 + rangeX/2, y2, y2 + rangeY/2);
                
            }

            parameters.Add(negParameters);
            parameters.Add(posParameters);

            Debug.Log("Parameters: " + parameters[0].A + " " + parameters[0].b + " " + parameters[1].A + " " + parameters[1].b + " " + parameters[2].A + " " + parameters[2].b);

            return parameters;

        }

        public List<float> GetLimitValues (float min, float max) 
        {
            List<float> limitValues = new List<float>();
            float rangeX = 360 - (max - min);
            float limitDown = min - rangeX/2;
            float limitUp = max + rangeX/2;
            limitValues.Add(limitDown);
            limitValues.Add(min);
            limitValues.Add(max);
            limitValues.Add(limitUp);
            return limitValues;
        }


        public (float, float) GetExtremum(LinearParameters linearParameters)
        {

            float x0 = -linearParameters.b / linearParameters.A;
            float x360 = (360 - linearParameters.b) / linearParameters.A;
            return (x0, x360);
        }


    }
}

