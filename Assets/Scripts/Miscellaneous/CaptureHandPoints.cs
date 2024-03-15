using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace TeleopReachy
{
    public class CaptureHandPoints : Singleton<CaptureHandPoints>
    {
        public Transform trackedLeftHand;
        public Transform trackedRightHand;

        private Vector3 lastPointLeft;
        private Vector3 lastPointRight;

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Vector3> rightCoordinates = new List<Vector3>();
        private bool calib_right_side = false;
        private bool calib_left_side = false;
        private bool calibration_done = false;


        public void Start()
        {
            trackedLeftHand = GameObject.Find("TrackedLeftHand").transform;
            trackedRightHand = GameObject.Find("TrackedRightHand").transform;

            if (trackedLeftHand == null || trackedRightHand == null) {
                Debug.Log("Manettes non trouvées."); 
                return;
            } 
            lastPointLeft = trackedLeftHand.position;
            lastPointRight = trackedRightHand.position;

            
            }

        public void Update()
        { 

            // capturing points from right side, then left side
            if (calib_right_side == false) {
                InstructionsTextUIManager.Instance.IndicateInitialCalibration("right");
                CapturePoints(trackedRightHand, rightCoordinates, lastPointRight, calib_right_side);}
            else if (calib_left_side == false){
                InstructionsTextUIManager.Instance.IndicateInitialCalibration("left");
                CapturePoints(trackedLeftHand, leftCoordinates, lastPointLeft, calib_left_side); }
            else if (calibration_done == false) {
                UpperBodyFeatures();
                Debug.Log(TransitionRoomManager.Instance.meanArmSize);
                TransitionRoomManager.Instance.FixNewPosition();
                calibration_done = true;
                // ajout du game object au centre de l'utilisateur 
                Transform userCenter = Camera.main.transform.Find("UserCenter");
                userCenter.position = TransitionRoomManager.Instance.midShoulderPoint;
            } else 
                InstructionsTextUIManager.Instance.IndicateEndofCalibration();


            
        }

        private void CapturePoints (Transform trackedHand, List <Vector3> sideCoordinates, Vector3 lastPoint, bool calib_side)
        {
            if (sideCoordinates.Count < 70){
                if (Vector3.Distance(lastPoint, trackedHand.position)> 0.05f){
                    sideCoordinates.Add(trackedHand.position);
                    lastPoint = trackedHand.position;}
            } else 
                calib_side = true;
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
    }
}