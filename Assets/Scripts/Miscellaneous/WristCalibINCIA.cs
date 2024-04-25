using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

namespace TeleopReachy
{
    public class WristCalibINCIA : Singleton<WristCalibINCIA>
    {
        private Transform leftControllerTransform;
        private Transform rightControllerTransform;
        private Transform HMDTransform;
        public int numberOfPoints = 50;

        private List<Vector3> leftDistances;
        private List<Vector3> rightDistances;
        private Matrix<double> leftControllerRotations;
        private Matrix<double> rightControllerRotations;
        public float recordInterval = 0.1f;
        private float timer = 0f;

        private bool calib_right_side = false;
        private bool calib_left_side = false;
        private bool calibration_done = false;
        private ControllersManager controllers;
        private bool start_calib_keyboard = false;
        private bool buttonX = false;
        public UnityEvent event_OnWristCalibChanged;
        public UnityEvent event_WaitForWristCalib;
        public UnityEvent event_StartRightWristCalib;
        public UnityEvent event_StartLeftWristCalib;

        private static WristCalibINCIA instance;

        public new static WristCalibINCIA Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<WristCalibINCIA>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(WristCalibINCIA).Name;
                        instance = obj.AddComponent<WristCalibINCIA>();
                    }
                }
                return instance;
            }
        }

        public void Start()
        {
            // Initialisation des listes et matrices
            leftDistances = new List<Vector3>();
            rightDistances = new List<Vector3>();
            leftControllerRotations = Matrix<double>.Build.Dense(3 * numberOfPoints, 6);
            rightControllerRotations = Matrix<double>.Build.Dense(3 * numberOfPoints, 6);

            // Récupération des transformées des contrôleurs
            leftControllerTransform = GameObject.Find("LeftHand Controller").transform;
            rightControllerTransform = GameObject.Find("RightHand Controller").transform;
            HMDTransform = GameObject.Find("Main Camera").transform;
            controllers = ActiveControllerManager.Instance.ControllersManager;

            if (leftControllerTransform == null || rightControllerTransform == null)
            {
                Debug.Log("Manettes non trouvées.");
                return;
            }

            event_OnWristCalibChanged = new UnityEvent();
            event_StartLeftWristCalib = new UnityEvent();
            event_StartRightWristCalib = new UnityEvent();
            event_WaitForWristCalib = new UnityEvent();
        }


        public void Update()
        { 
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
            if (buttonX)
            {
                start_calib_keyboard = true;

            }

            if (!start_calib_keyboard && !calibration_done) 
            {
                event_WaitForWristCalib.Invoke();
                Debug.Log("Attente de la calibration");
            }

            //capture des points du côté droit, puis du côté gauche
            if (!calib_right_side && start_calib_keyboard) {
                timer += Time.deltaTime;
                event_StartRightWristCalib.Invoke();
                CaptureData("right");
            }

            else if (!calib_left_side && start_calib_keyboard){
                timer += Time.deltaTime;
                event_StartLeftWristCalib.Invoke();
                CaptureData("left");
            }

            else if (calib_left_side && !calibration_done)  
            {
                Debug.Log("Fin de la capture");
                EstimateWristCenter();
                calibration_done = true;
                event_OnWristCalibChanged.Invoke();
 
            }  
        }

        private void CaptureData(string side)
        {
            if (side == "right")
            {
                if (rightDistances.Count < numberOfPoints)
                {
                    if (timer >= recordInterval)
                    {
                        Debug.Log("capture droite " + rightDistances.Count);
                    
                        double[] diffVector = new double[] {
                            -(rightControllerTransform.position.x - HMDTransform.position.x),
                            -(rightControllerTransform.position.y - HMDTransform.position.y),
                            -(rightControllerTransform.position.z - HMDTransform.position.z)
                        };
                        rightDistances.Add(new Vector3((float)diffVector[0], (float)diffVector[1], (float)diffVector[2]));

                        Quaternion rightControllerRotation = rightControllerTransform.rotation;
                        Matrix<double> controllerRotationMatrix = DenseMatrix.OfArray(new double[,]
                        {
                            { rightControllerRotation.x, rightControllerRotation.y, rightControllerRotation.z },
                            { -rightControllerRotation.y, rightControllerRotation.x, -rightControllerRotation.w },
                            { -rightControllerRotation.z, rightControllerRotation.w, rightControllerRotation.x }
                        });
                        
                        rightControllerRotations.SetSubMatrix(3 * rightDistances.Count - 3, 3, 0, 3, controllerRotationMatrix);
                        timer = 0f;
                    }
                }
                else
                {
                    Debug.Log("fin de la calib droite");
                    calib_right_side = true;
                    start_calib_keyboard = false;
                    timer = 0f;
                }
            }
            else if (side == "left")
            {
                if (leftDistances.Count < numberOfPoints)
                {
                    if (timer >= recordInterval)
                    {
                        Debug.Log("capture gauche " + leftDistances.Count);
                        double[] diffVector = new double[] {
                            -(leftControllerTransform.position.x - HMDTransform.position.x),
                            -(leftControllerTransform.position.y - HMDTransform.position.y),
                            -(leftControllerTransform.position.z - HMDTransform.position.z)
                        };
                        leftDistances.Add(new Vector3((float)diffVector[0], (float)diffVector[1], (float)diffVector[2]));

                        Quaternion leftControllerRotation = leftControllerTransform.rotation;
                        Matrix<double> controllerRotationMatrix = DenseMatrix.OfArray(new double[,]
                        {
                            { leftControllerRotation.x, leftControllerRotation.y, leftControllerRotation.z },
                            { -leftControllerRotation.y, leftControllerRotation.x, -leftControllerRotation.w },
                            { -leftControllerRotation.z, leftControllerRotation.w, leftControllerRotation.x }
                        });
                        
                        leftControllerRotations.SetSubMatrix(3 * leftDistances.Count - 3, 3, 0, 3, controllerRotationMatrix);
                        timer = 0f;
                    }
                }
                else
                {
                    calib_left_side = true;
                }
            }
        }


        public void EstimateWristCenter()
        {
            Matrix<double> rightQinv = rightControllerRotations.PseudoInverse();
            Matrix<double> leftQinv = leftControllerRotations.PseudoInverse();
            Vector<double> rightDistancesVector = DenseVector.OfEnumerable(rightDistances.SelectMany(v => new double[] { (double)v.x, (double)v.y, (double)v.z }));
            Vector<double> leftDistancesVector = DenseVector.OfEnumerable(leftDistances.SelectMany(v => new double[] { (double)v.x, (double)v.y, (double)v.z }));
            Vector<double> rightSolution = rightQinv * rightDistancesVector;
            Vector<double> leftSolution = leftQinv * leftDistancesVector;
            Debug.Log("rightSolution = " + rightSolution);
            Debug.Log("leftSolution = " + leftSolution);
        }

    }
}
