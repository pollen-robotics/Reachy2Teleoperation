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
        private Transform newUserCenter;
        private bool calib_right_side = false;
        private bool calib_left_side = false;
        private bool calibration_done = false;
        private ControllersManager controllers;
        private bool start_calib_keyboard = false;
        private bool buttonX = false;
        public UnityEvent event_OnCalibChanged;

     


        public void Start()
        {
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

            
            }

        public void Update()
        { 
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
            if (buttonX)
                start_calib_keyboard = true;

            if (!start_calib_keyboard)
                InstructionsTextUIManager.Instance.IndicateToPressX();

            // capturing points from right side, then left side
            if (!calib_right_side && start_calib_keyboard) {
                InstructionsTextUIManager.Instance.IndicateInitialCalibration("right");
                CapturePoints("right");}

            else if (!calib_left_side && start_calib_keyboard){
                InstructionsTextUIManager.Instance.IndicateInitialCalibration("left");
                CapturePoints("left");}

            else if (calib_left_side && !calibration_done) {
                InstructionsTextUIManager.Instance.IndicateEndofCalibration();
                UpperBodyFeatures();
                TransitionRoomManager.Instance.FixNewPosition();
                // ajout du game object au centre de l'utilisateur 
                newUserCenter.localPosition = TransitionRoomManager.Instance.midShoulderPoint;
                calibration_done = true;
                event_OnCalibChanged.Invoke();} 

            // else if (calibration_done){
            //     InstructionsTextUIManager.Instance.IndicateToPressA();
            //     }
            
        }

        private void CapturePoints (string side)
        {
            if (side == "right"){
                if (rightCoordinates.Count < 70){
                    Debug.Log("last point" + lastPointRight);
                    if (Vector3.Distance(lastPointRight, trackedRightHand.position)> 0.07f){
                        rightCoordinates.Add(trackedRightHand.position);
                        lastPointRight = trackedRightHand.position;
                        Debug.Log(rightCoordinates.Count);}
                } else {
                    calib_right_side = true;
                    start_calib_keyboard = false;}
            } else if (side == "left"){
                if (leftCoordinates.Count < 70){
                    Debug.Log("last point" + lastPointLeft);
                    if (Vector3.Distance(lastPointLeft, trackedLeftHand.position)> 0.07f){
                        leftCoordinates.Add(trackedLeftHand.position);
                        lastPointLeft = trackedLeftHand.position;
                        Debug.Log(leftCoordinates.Count);}
                } else  {
                    calib_left_side = true;
                    start_calib_keyboard = false;}
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

    }
}