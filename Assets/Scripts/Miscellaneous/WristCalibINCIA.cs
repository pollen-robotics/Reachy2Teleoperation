
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
        public Vector3 rightWristCenter;
        public Vector3 leftWristCenter;
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

                        Quaternion rightControllerQuat = rightControllerTransform.rotation;
                        Quaternion HMDQuat = HMDTransform.rotation;
                        Matrix<double> combinedMatrix = CombineRotationMatrices(rightControllerQuat, HMDQuat);
                        rightControllerRotations.SetSubMatrix((rightDistances.Count-1)*3, 3, 0, 6, combinedMatrix);
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

                        Quaternion leftControllerQuat = leftControllerTransform.rotation;
                        Quaternion HMDQuat = HMDTransform.rotation;
                        Matrix<double> combinedMatrix = CombineRotationMatrices(leftControllerQuat, HMDQuat);
                        leftControllerRotations.SetSubMatrix((leftDistances.Count-1)*3, 3, 0, 6, combinedMatrix);



                        timer = 0f;
                    }
                }
                else
                {
                    calib_left_side = true;
                }
            }
        }

        private Matrix<double> CombineRotationMatrices(Quaternion controllerRotationQuat, Quaternion HMDRotationQuat)
        {
            // Convertir les quaternions en matrices de rotation
            Matrix4x4 controllerRotationMatrix = Matrix4x4.Rotate(controllerRotationQuat);
            Matrix4x4 HMDRotationMatrix = Matrix4x4.Rotate(HMDRotationQuat);

            // Extraire les parties 3x3 des matrices de rotation
            Matrix<double> controllerRotation3x3 = ExtractMatrix3x3(controllerRotationMatrix);
            Matrix<double> HMDRotation3x3 = ExtractMatrix3x3(HMDRotationMatrix);

            // Créer une nouvelle matrice 3x6
            Matrix<double> combinedMatrix = Matrix<double>.Build.Dense(3, 6);

            // Remplir la nouvelle matrice avec les parties 3x3
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Partie 3x3 de controllerRotationMatrix
                    combinedMatrix[i, j] = controllerRotation3x3[i, j];

                    // Partie 3x3 de -HMDRotationMatrix
                    combinedMatrix[i, j + 3] = -HMDRotation3x3[i, j];
                }
            }

            return combinedMatrix;
        }

        private Matrix<double> ExtractMatrix3x3(Matrix4x4 matrix)
        {
            // Extraction des parties 3x3 de la matrice 4x4
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { matrix.m00, matrix.m01, matrix.m02 },
                { matrix.m10, matrix.m11, matrix.m12 },
                { matrix.m20, matrix.m21, matrix.m22 }
            });
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

            rightWristCenter = new Vector3((float)rightSolution[0], (float)rightSolution[1], (float)rightSolution[2]);
            leftWristCenter = new Vector3((float)leftSolution[0], (float)leftSolution[1], (float)leftSolution[2]);
            
        }

    }
}
