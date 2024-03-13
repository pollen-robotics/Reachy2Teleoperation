using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    public class CaptureHandPoints : MonoBehaviour
    {
        public Transform trackedLeftHand;
        public Transform trackedRightHand;
        
        private RotationCenterCalcul rotationCenterCalcul;

        private void Start()
        {
            rotationCenterCalcul = new RotationCenterCalcul();
            (double armSize, Vector3 leftShoulderCenter, Vector3 rightShoulderCenter) = rotationCenterCalcul.BothShoulderCalibration(trackedLeftHand, trackedRightHand);
        }
    }
}
