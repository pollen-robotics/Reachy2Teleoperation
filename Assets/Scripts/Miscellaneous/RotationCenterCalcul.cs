using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;

namespace TeleopReachy
{
    public class RotationCenterCalcul 
    {
        public static List<Vector3> leftCoordinates = new List <Vector3>();
        public static List<Vector3> rightCoordinates = new List <Vector3>();

        public (double armSize, Vector3 leftShoulderCenter, Vector3 rightShoulderCenter) BothShoulderCalibration(Transform trackedLeftHand, Transform trackedRightHand)
        {
            MonoBehaviour handPointsCapture = GameObject.FindObjectOfType<CaptureHandPoints>();
            handPointsCapture.StartCoroutine(CapturePoints(trackedLeftHand, leftCoordinates));
            handPointsCapture.StartCoroutine(CapturePoints(trackedRightHand, rightCoordinates));
            (double leftArmSize, Vector3 leftShoulderCenter) = CenterShoulderLSM (leftCoordinates);
            (double rightArmSize, Vector3 rightShoulderCenter) = CenterShoulderLSM (rightCoordinates);
            double meanArmSize = (leftArmSize+rightArmSize)/2;

            return (meanArmSize, leftShoulderCenter, rightShoulderCenter);

        }

        public (double armSize, Vector3 shoulderCenter) CenterShoulderLSM (List<Vector3> sideCoordinates)
        {
            int numberOfPoints = sideCoordinates.Count;
            
            double[,] A = new double [numberOfPoints,4];
            double[,] f = new double [numberOfPoints,1] ;

            for (int i = 0 ; i < numberOfPoints ; i++)
            {
                A[i,0] = sideCoordinates[i].x * 2;
                A[i,1] = sideCoordinates[i].y * 2;
                A[i,2] = sideCoordinates[i].z * 2;
                A[i,3] = 1.0f;
                f[i,0] = sideCoordinates[i].x * sideCoordinates[i].x + sideCoordinates[i].y *sideCoordinates[i].y  + sideCoordinates[i].z * sideCoordinates[i].z ;
            }

            var aMatrix = Matrix<double>.Build.DenseOfArray(A);
		    var fMatrix = Matrix<double>.Build.DenseOfArray(f);
            var shoulderPosition = MultipleRegression.NormalEquations(aMatrix, fMatrix);

            // solve for the radius
            double t = (shoulderPosition[0,0] * shoulderPosition[0,0]) + (shoulderPosition[1,0] * shoulderPosition[1,0]) + (shoulderPosition[2,0] * shoulderPosition[2,0]) + shoulderPosition[3,0];
            double radius = System.Math.Sqrt(t);
            Vector3 shoulderCenter = new Vector3((float)shoulderPosition[0, 0], (float)shoulderPosition[1, 0], (float)shoulderPosition[2, 0]);
           
            return (radius, shoulderCenter);
            

        }

        private static IEnumerator CapturePoints(Transform trackedHand, List<Vector3> sideCoordinates) 
        {
            float samplingFrequency = 100; 
            float samplingInterval = 1.0f / samplingFrequency;

            while (sideCoordinates.Count < 70)
            {
                yield return new WaitForSeconds(samplingInterval);
                sideCoordinates.Add(trackedHand.position);
                
            }
        }
    }
}

