using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;


public class HandJoints : MonoBehaviour
{
    public Handedness handedness;

    public Vector3 indexInWrist;
    public Vector3 middleInWrist;
    public Vector3 ringInWrist;
    public Vector3 thumbInWrist;

    public float indexPinch;
    public float middlePinch;
    public float ringPinch;

    public Vector3 indexToThumbDistance;
    public Vector3 middleToThumbDistance;
    public Vector3 ringToThumbDistance;
    
    private XRHandSubsystem handSubsystem;

    public bool IsInitialized { get; private set; }

    private void OnEnable()
    {
        IsInitialized = false;
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
                    IsInitialized = true;
                    break;
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        if (handSubsystem == null) 
        {
            return;
        }

        XRHand hand = (handedness == Handedness.Left) ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) 
        {
            return;
        }

        Pose indexTipPose = InWristPose(hand, XRHandJointID.IndexTip);
        Pose indexMCPPose = InWristPose(hand, XRHandJointID.IndexProximal);
        Pose middleTipPose = InWristPose(hand, XRHandJointID.MiddleTip);
        Pose middleMCPPose = InWristPose(hand, XRHandJointID.MiddleProximal);
        Pose ringTipPose = InWristPose(hand, XRHandJointID.RingTip);
        Pose ringMCPPose = InWristPose(hand, XRHandJointID.RingProximal);
        Pose thumbTipPose = InWristPose(hand, XRHandJointID.ThumbTip);
        Pose thumbMCPPose = InWristPose(hand, XRHandJointID.ThumbMetacarpal);

        // Debug.LogError(ringMCPPose.position);
        // indexInWrist = (indexTipPose.position - indexMCPPose.position);
        // indexInWrist.x = indexTipPose.position.x;
        // middleInWrist = (middleTipPose.position - middleMCPPose.position);
        // middleInWrist.x = middleTipPose.position.x;
        // ringInWrist = (ringTipPose.position - ringMCPPose.position);
        // ringInWrist.x = ringTipPose.position.x;
        // thumbInWrist = (thumbTipPose.position - thumbMCPPose.position);

        indexInWrist = indexTipPose.position;
        middleInWrist = middleTipPose.position;
        ringInWrist = ringTipPose.position;
        thumbInWrist = thumbTipPose.position;

        XRFingerShapeMath.CalculateFingerShape(hand, XRHandFingerID.Index, XRFingerShapeTypes.Pinch).TryGetPinch(out indexPinch);
        XRFingerShapeMath.CalculateFingerShape(hand, XRHandFingerID.Middle, XRFingerShapeTypes.Pinch).TryGetPinch(out middlePinch);
        XRFingerShapeMath.CalculateFingerShape(hand, XRHandFingerID.Ring, XRFingerShapeTypes.Pinch).TryGetPinch(out ringPinch);

        indexToThumbDistance = DistanceToThumbInWrist(hand, XRHandJointID.IndexTip);
        middleToThumbDistance = DistanceToThumbInWrist(hand, XRHandJointID.MiddleTip);
        ringToThumbDistance = DistanceToThumbInWrist(hand, XRHandJointID.RingTip);
    }

    private Pose InWristPose(XRHand hand, XRHandJointID jointID)
    {
        hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose);
        XRHandJoint joint = hand.GetJoint(jointID);
        if (joint.TryGetPose(out Pose jointPose))
        {
            Vector3 posJointInWrist = Quaternion.Inverse(wristPose.rotation) * (jointPose.position - wristPose.position);
            Quaternion rotJointInWrist = Quaternion.Inverse(wristPose.rotation) * jointPose.rotation;
            return new Pose(posJointInWrist, rotJointInWrist);
        }
        else
        {
            return Pose.identity;
        }
    }

    private Vector3 DistanceToThumbInWrist(XRHand hand, XRHandJointID jointID)
    {
        hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose);
        XRHandJoint joint = hand.GetJoint(jointID);
        XRHandJoint thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        if (joint.TryGetPose(out Pose jointPose) && thumb.TryGetPose(out Pose thumbPose))
        {
            Vector3 distanceToThumb = jointPose.position - thumbPose.position;
            Vector3 distanceToThumbInWrist = Quaternion.Inverse(wristPose.rotation) * distanceToThumb;
            return distanceToThumbInWrist;
        }
        else
        {
            return new Vector3(0, 0, 0);
        }
    }

}