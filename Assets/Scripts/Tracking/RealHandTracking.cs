using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

namespace TeleopReachy
{
    public class RealHandTracking : Singleton<RealHandTracking>
    {
        [SerializeField] private XRHandJointID jointId = XRHandJointID.Palm;
        [SerializeField] private XRHandFingerID fingerId = XRHandFingerID.Index;

        private XRHandSubsystem handSubsystem;

        private XRHand leftHand;
        private XRHand rightHand;

        private Pose rightHandPose;
        private Pose leftHandPose;

        public bool IsOkPosePerformed { get; private set; }
        public bool IsLeftHandTracked { get; private set; }
        public bool IsRightHandTracked { get; private set; }

        public bool IsHandTrackingUsed()
        {
            return IsLeftHandTracked || IsRightHandTracked;
        }

        private void OnEnable()
        {
            IsOkPosePerformed = false;
            StartCoroutine(WaitForHandSubsytem());
        }

        IEnumerator WaitForHandSubsytem()
        {
            List<XRHandSubsystem> subsystems = new();
            while (handSubsystem == null)
            {
                SubsystemManager.GetSubsystems(subsystems);
                foreach (var sub in subsystems)
                {
                    if (sub != null && sub.running)
                    {
                        handSubsystem = sub;
                        handSubsystem.trackingAcquired += OnTrackingAcquired;
                        handSubsystem.trackingLost += OnTrackingLost;
                        handSubsystem.updatedHands += OnUpdatedHands;
                        break;
                    }
                }
                yield return null;
            }
        }

        void OnTrackingAcquired(XRHand hand)
        {
            Debug.LogError("Tracking");
            switch (hand.handedness)
            {
                case Handedness.Left:
                    IsLeftHandTracked = true;
                    break;

                case Handedness.Right:
                    IsRightHandTracked = true;
                    break;
            }
        }

        void OnTrackingLost(XRHand hand)
        {
            Debug.LogError("Lost");

            switch (hand.handedness)
            {
                case Handedness.Left:
                    IsLeftHandTracked = false;
                    break;

                case Handedness.Right:
                    IsRightHandTracked = false;
                    break;
            }
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            leftHand = subsystem.leftHand;
            rightHand = subsystem.rightHand;
        }


        void Update()
        {
            if (handSubsystem == null) 
            {
                return;
            }

            if (leftHand.isTracked) 
            {   
                XRHandJoint leftPalm = leftHand.GetJoint(jointId);
                leftPalm.TryGetPose(out leftHandPose);
            }

            if (rightHand.isTracked) 
            {   
                XRHandJoint rightPalm = rightHand.GetJoint(jointId);
                rightPalm.TryGetPose(out rightHandPose);
            }

            // float thumb_BaseCurl;
            // XRFingerShapeMath.CalculateFingerShape(rightHand, XRHandFingerID.Thumb, XRFingerShapeTypes.BaseCurl).TryGetBaseCurl(out thumb_BaseCurl);
            // float index_TipCurl;
            // XRFingerShapeMath.CalculateFingerShape(rightHand, XRHandFingerID.Index, XRFingerShapeTypes.TipCurl).TryGetTipCurl(out index_TipCurl);
            // float middle_TipCurl;
            // XRFingerShapeMath.CalculateFingerShape(rightHand, XRHandFingerID.Middle, XRFingerShapeTypes.TipCurl).TryGetTipCurl(out middle_TipCurl);
            // float ring_TipCurl;
            // XRFingerShapeMath.CalculateFingerShape(rightHand, XRHandFingerID.Ring, XRFingerShapeTypes.TipCurl).TryGetTipCurl(out ring_TipCurl);
            // float little_TipCurl;
            // XRFingerShapeMath.CalculateFingerShape(rightHand, XRHandFingerID.Little, XRFingerShapeTypes.TipCurl).TryGetTipCurl(out little_TipCurl);

            // Debug.LogError(middle_TipCurl);
        }

        public Pose GetRightHandPose()
        {
            return rightHandPose;
        }

        public Pose GetLeftHandPose()
        {
            return leftHandPose;
        }

        public void PoseDetected(XRHandPose pose)
        {
            Debug.LogError("pose detected");
            IsOkPosePerformed = true;
        }

        public void ShapeDetected(XRHandShape shape)
        {
            Debug.LogError("shape detected");
            IsOkPosePerformed = true;
        }

        public void PoseStopped()
        {
            IsOkPosePerformed = false;
        }
    }
}