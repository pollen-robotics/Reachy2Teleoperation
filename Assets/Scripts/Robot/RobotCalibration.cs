using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace TeleopReachy
{
    public class RobotCalibration : Singleton<RobotCalibration>
    {
        private Transform trackedLeftHand;
        private Transform trackedRightHand;

        private Vector3 lastPointLeft;
        private Vector3 lastPointRight;

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Vector3> rightCoordinates = new List<Vector3>();
        private float intervalTime=1/100 ; //fréquence de 100Hz
        private float actualTime=0f;
        private Transform newUserCenter;
        private bool calib_right_side = false;
        private bool calib_left_side = false;
        private bool calibration_done = false;
        private ControllersManager controllers;
        private bool start_calib_keyboard = false;
        private bool buttonX = false;
        public UnityEvent event_OnCalibChanged;
        public UnityEvent event_WaitForCalib;
        public UnityEvent event_StartRightCalib;
        public UnityEvent event_StartLeftCalib;

        private  InstructionsTextUIManager instructionsTextUIManager;


        private static RobotCalibration instance;

        public new static RobotCalibration Instance // ajout du new, à voir si erreur 
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RobotCalibration>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(RobotCalibration).Name;
                        instance = obj.AddComponent<RobotCalibration>();
                    }
                }
                return instance;
            }
        }

     

        public void Start()
        {
            Debug.Log("Start of RobotCalibration");
            trackedLeftHand = GameObject.Find("TrackedLeftHand").transform;
            trackedRightHand = GameObject.Find("TrackedRightHand").transform;
            newUserCenter = GameObject.Find("NewUserCenter").transform;
            controllers = ActiveControllerManager.Instance.ControllersManager;

            if (trackedLeftHand == null || trackedRightHand == null) {
                Debug.Log("Manettes non trouvées."); 
                return;
            } 

            lastPointLeft = trackedLeftHand.position;
            lastPointRight = trackedRightHand.position;
            event_OnCalibChanged = new UnityEvent();
            event_StartLeftCalib = new UnityEvent();
            event_StartRightCalib = new UnityEvent();
            event_WaitForCalib = new UnityEvent();            
            }

        public void Update()
        { 
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
            if (buttonX)
                start_calib_keyboard = true;

            if (!start_calib_keyboard && !calibration_done)
            {
                Debug.Log("calib : press X");
                event_WaitForCalib.Invoke();
                actualTime = 0f;
            }

            // capturing points from right side, then left side
            // if (!calib_right_side && start_calib_keyboard) {
            //     Debug.Log("calib droite");
            //     event_StartRightCalib.Invoke();
            //     CapturePoints("right");
            //     actualTime += Time.deltaTime;}

            // else if (!calib_left_side && start_calib_keyboard){
            //     Debug.Log("calib gauche");
            //     event_StartLeftCalib.Invoke();
            //     CapturePoints("left");
            //     actualTime += Time.deltaTime;}

            else if (!calibration_done && start_calib_keyboard)
            {
                Debug.Log("calib simultanée");
                event_StartLeftCalib.Invoke();
                CapturePoints();
                actualTime += Time.deltaTime;


            }

            else if (calib_left_side && !calibration_done) 
            {
                Debug.Log("calcul de calib");
                UpperBodyFeatures();
                TransitionRoomManager.Instance.FixNewPosition();
                // ajout du game object au centre de l'utilisateur 
                newUserCenter.localPosition = TransitionRoomManager.Instance.midShoulderPoint;
                ExportCoordinatesToCSV("C:/Users/User/Documents"); // à modifier
                calibration_done = true;
                event_OnCalibChanged.Invoke();
                Debug.Log("calib finie");
            }  
        }

        // private void CapturePoints (string side)
        // {
        //     if (side == "right"){
        //         if (rightCoordinates.Count < 200){
        //             Debug.Log("last point" + lastPointRight);
                    
        //             //if (Vector3.Distance(lastPointRight, trackedRightHand.position)> 0.07f)
        //             if (actualTime % intervalTime == 0)
        //             {
        //                 rightCoordinates.Add(trackedRightHand.position);
        //                 lastPointRight = trackedRightHand.position;
        //                 Debug.Log(rightCoordinates.Count);}
        //         } else {
        //             calib_right_side = true;
        //             start_calib_keyboard = false;}
        //     } else if (side == "left"){
        //         if (leftCoordinates.Count < 200)
        //         {
        //             Debug.Log("last point" + lastPointLeft);
        //             //if (Vector3.Distance(lastPointLeft, trackedLeftHand.position)> 0.07f)
        //             if (actualTime % intervalTime == 0 )
        //             {
        //                 leftCoordinates.Add(trackedLeftHand.position);
        //                 lastPointLeft = trackedLeftHand.position;
        //                 Debug.Log(leftCoordinates.Count);}
        //         } else  {
        //             calib_left_side = true;
        //             start_calib_keyboard = false;}
        //     }
            
        // }

        private void CapturePoints () // version simultanée 
        {
                if (rightCoordinates.Count < 200 || leftCoordinates.Count < 200){
                    if (actualTime % intervalTime == 0)
                    {
                        rightCoordinates.Add(trackedRightHand.position);
                        lastPointRight = trackedRightHand.position;
                        Debug.Log("droit:" + rightCoordinates.Count);
                        
                        leftCoordinates.Add(trackedLeftHand.position);
                        lastPointLeft = trackedLeftHand.position;
                        Debug.Log("gauche :" + leftCoordinates.Count);}
                
                } else {
                    calib_left_side = true;
                    calib_right_side = true;
                    start_calib_keyboard = false;
                }
            }
            
    


        public void UpperBodyFeatures()
        {
 
            (double leftArmSize, Vector3 leftShoulderCenter) = CenterRotationLSM(leftCoordinates);
            (double rightArmSize, Vector3 rightShoulderCenter) = CenterRotationLSM(rightCoordinates);
            Debug.Log("LSM des deux côtés ok");
            
            double meanArmSize = (leftArmSize + rightArmSize) / 2f;
            Vector3 midShoulderPoint = (leftShoulderCenter + rightShoulderCenter) / 2f;

            Debug.Log("Epaule G : " + leftShoulderCenter +"/ Epaule D : " + rightShoulderCenter + "/Milieu Epaule  : " + midShoulderPoint +", Taille moyenne des bras : " + meanArmSize);
            TransitionRoomManager.Instance.meanArmSize = meanArmSize;
            TransitionRoomManager.Instance.midShoulderPoint = midShoulderPoint;
            TransitionRoomManager.Instance.shoulderWidth = Vector3.Distance(leftShoulderCenter, rightShoulderCenter)/2f;
            Debug.Log("largeur épaule =" + Vector3.Distance(leftShoulderCenter, rightShoulderCenter)/2f);
        }

        private (double radius, Vector3 rotationCenterPosition) CenterRotationLSM(List<Vector3> sideCoordinates)
        {
            Debug.Log("Debut de la LSM");
            int numberOfPoints = sideCoordinates.Count;
            Debug.Log(numberOfPoints);
            
            double[,] A = new double[numberOfPoints, 4];
            double[,] f = new double[numberOfPoints, 1];

            for (int i = 0; i < numberOfPoints; i++)
            {
                A[i, 0] = sideCoordinates[i].x * 2;
                A[i, 1] = sideCoordinates[i].y * 2;
                A[i, 2] = sideCoordinates[i].z * 2;
                A[i, 3] = 1.0f;
                f[i, 0] = sideCoordinates[i].x * sideCoordinates[i].x + sideCoordinates[i].y * sideCoordinates[i].y + sideCoordinates[i].z * sideCoordinates[i].z;
            }
            Debug.Log("Modif de A et f faites");

            var aMatrix = Matrix<double>.Build.DenseOfArray(A);
		    var fMatrix = Matrix<double>.Build.DenseOfArray(f);
            var rotCenter = MultipleRegression.NormalEquations(aMatrix, fMatrix);
            Debug.Log("rotCenter = " + rotCenter);

            double t = (rotCenter[0, 0] * rotCenter[0, 0]) + (rotCenter[1, 0] * rotCenter[1, 0]) + (rotCenter[2, 0] * rotCenter[2, 0]) + rotCenter[3, 0];
            double radius = System.Math.Sqrt(t);
            Vector3 rotationCenterPosition = new Vector3((float)rotCenter[0, 0], (float)rotCenter[1, 0], (float)rotCenter[2, 0]);
            Debug.Log("r=" + radius + "x=" + rotationCenterPosition.x + "y="+ rotationCenterPosition.y+ "z=" +rotationCenterPosition.z);

            return (radius, rotationCenterPosition);
        }

        public bool IsCalibrated (){
            return calibration_done;
        }

        public void ExportCoordinatesToCSV(string filePath)
        {
            string csvContent = "Side,X,Y,Z\n";
            foreach (Vector3 point in leftCoordinates)
                csvContent += "Left," + point.x + "," + point.y + "," + point.z + "\n";

            foreach (Vector3 point in rightCoordinates)
                csvContent += "Right," + point.x + "," + point.y + "," + point.z + "\n";

            File.WriteAllText(filePath, csvContent);

            Debug.Log("Coordonnées exportées dans fichier CSV : " + filePath);
        }

    }
}