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

            float fullCurl;
            XRFingerShapeMath.CalculateFingerShape(rightHand, fingerId, XRFingerShapeTypes.Pinch).TryGetPinch(out fullCurl);
            // Debug.Log(fullCurl);
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
            IsOkPosePerformed = true;
        }

        public void PoseStopped()
        {
            IsOkPosePerformed = false;
        }
    }
}