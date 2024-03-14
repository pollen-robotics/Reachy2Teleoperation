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

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Vector3> rightCoordinates = new List<Vector3>();

        public IEnumerator CaptureAndCalibrate()
        {
            yield return StartCoroutine(CapturePoints(trackedLeftHand, leftCoordinates));
            yield return StartCoroutine(CapturePoints(trackedRightHand, rightCoordinates));
            (TransitionRoomManager.Instance.meanArmSize, TransitionRoomManager.Instance.midShoulderPoint) = UpperBodyFeatures(leftCoordinates, rightCoordinates);
            Debug.Log("Calibration terminée.");
        }

        public IEnumerator CapturePoints(Transform trackedHand, List<Vector3> sideCoordinates)
        {
            Debug.Log("Capture des points start.");
            
            float samplingFrequency = 100;
            float samplingInterval = 1.0f / samplingFrequency;

            while (sideCoordinates.Count < 70)
            {
                yield return new WaitForSeconds(samplingInterval);
                sideCoordinates.Add(trackedHand.position);
            }

            Debug.Log("Capture des points stop.");

        }

        public (double armSize, Vector3 midShoulderPoint) UpperBodyFeatures(List<Vector3>leftCoordinates, List<Vector3>rightCoordinates)
        {
 
            (double leftArmSize, Vector3 leftShoulderCenter) = CenterRotationLSM(leftCoordinates);
            (double rightArmSize, Vector3 rightShoulderCenter) = CenterRotationLSM(rightCoordinates);
            Debug.Log("LSM des deux côtés ok");
            
            double meanArmSize = (leftArmSize + rightArmSize) / 2f;
            Vector3 midShoulderPoint = (leftShoulderCenter + rightShoulderCenter) / 2f;

            Debug.Log("Epaule G : " + leftShoulderCenter +"/ Epaule D : " + rightShoulderCenter + "/Milieu Epaule  : " + midShoulderPoint +", Taille moyenne des bras : " + meanArmSize);

            return (meanArmSize, midShoulderPoint);
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
            Debug.Log("Multiple regression ok");

            double t = (rotCenter[0, 0] * rotCenter[0, 0]) + (rotCenter[1, 0] * rotCenter[1, 0]) + (rotCenter[2, 0] * rotCenter[2, 0]) + rotCenter[3, 0];
            double radius = System.Math.Sqrt(t);
            Vector3 rotationCenterPosition = new Vector3((float)rotCenter[0, 0], (float)rotCenter[1, 0], (float)rotCenter[2, 0]);
            Debug.Log("r=" + radius + "x=" + rotationCenterPosition.x + "y="+ rotationCenterPosition.y+ "z=" +rotationCenterPosition.z);

            return (radius, rotationCenterPosition);
        }
    }
}
