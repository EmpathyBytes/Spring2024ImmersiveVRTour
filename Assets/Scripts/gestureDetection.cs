using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;

public class GestureRecognizer : MonoBehaviour
{
    /*
    public XRHandSubsystem handSubsystem;
    private XRHand left;
    private XRHand right;
    public string currGesture = "None"; 
    */
    [SerializeField] private TextMeshPro RcurrGesture;
    [SerializeField] private TextMeshPro LcurrGesture;

    void Start()
    {
        /*
        var descriptors = new List<XRHandSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(descriptors);
        if (descriptors.Count > 0)
        {
            handSubsystem = descriptors[0].Create();
        }
        */

        RcurrGesture.text = "Right";
        LcurrGesture.text = "Left";
    }

    public string GetRCurrentPose()
    {
        return RcurrGesture.text;
    }

    void Update()
    {
        switch (RcurrGesture.text)
        {
            case ("None"):
                RcurrGesture.text = "Deez";
                return;
            case ("Nuts"):
                RcurrGesture.text = "Joe";
                return;
            default:
                return;
        }
        /*
        if (handSubsystem != null)
        {
            left = handSubsystem.leftHand;
            right = handSubsystem.rightHand;

            if (left.isTracked || right.isTracked)
            {
                DetectGesture();
            }
        }
        */
    }
    /*
    void DetectGesture()
    {
        if (IsThumbsUp(left) || IsThumbsUp(right))
        {
            currGesture = "ThumbsUp";
        }
        else
        {
            currGesture = "None";
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
            return false;
        }


        bool isThumbExtended = thumbTip.position.y > thumbProximal.position.y;


        bool areFingersCurled = (indexTip.position.y < indexProximal.position.y) &&
                                (middleTip.position.y < indexProximal.position.y) &&
                                (ringTip.position.y < indexProximal.position.y) &&
                                (pinkyTip.position.y < indexProximal.position.y);

        return isThumbExtended && areFingersCurled;
    }
    */
}
