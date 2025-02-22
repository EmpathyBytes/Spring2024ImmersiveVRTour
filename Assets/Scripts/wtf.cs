using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public class GestureRecognizer : MonoBehaviour
{
    public XRHandSubsystem handSubsystem; // Reference to XR Hand subsystem
    private XRHand leftHand;
    private XRHand rightHand;

    public string currentGesture = "None"; // Stores recognized gesture

    void Start()
    {
        var descriptors = new List<XRHandSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(descriptors);
        if (descriptors.Count > 0)
        {
            handSubsystem = descriptors[0].Create();
        }
    }

    void Update()
    {
        if (handSubsystem != null)
        {
            leftHand = handSubsystem.leftHand;
            rightHand = handSubsystem.rightHand;

            if (leftHand.isTracked || rightHand.isTracked)
            {
                DetectGesture();
            }
        }
    }

    void DetectGesture()
    {
        if (IsThumbsUp(leftHand) || IsThumbsUp(rightHand))
        {
            currentGesture = "ThumbsUp";
        }
        else
        {
            currentGesture = "None";
        }
    }

    bool IsThumbsUp(XRHand hand)
    {
        if (!hand.isTracked) return false;

        // Get joint positions
        if (!hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTip) ||
            !hand.GetJoint(XRHandJointID.ThumbMetacarpal).TryGetPose(out Pose thumbProximal) ||
            !hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTip) ||
            !hand.GetJoint(XRHandJointID.IndexProximal).TryGetPose(out Pose indexProximal) ||
            !hand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out Pose middleTip) ||
            !hand.GetJoint(XRHandJointID.RingTip).TryGetPose(out Pose ringTip) ||
            !hand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out Pose pinkyTip))
        {
            return false; // Return false if any joint position isn't available
        }

        // 1. Check if thumb is extended upwards (higher than the knuckle)
        bool isThumbExtended = thumbTip.position.y > thumbProximal.position.y;

        // 2. Check if all other fingers are curled (tips below their knuckles)
        bool areFingersCurled = (indexTip.position.y < indexProximal.position.y) &&
                                (middleTip.position.y < indexProximal.position.y) &&
                                (ringTip.position.y < indexProximal.position.y) &&
                                (pinkyTip.position.y < indexProximal.position.y);

        return isThumbExtended && areFingersCurled;
    }
}
