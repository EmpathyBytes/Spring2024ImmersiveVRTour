// using UnityEngine;

// public class HPDet : MonoBehaviour
// {
//     [SerializeField] private OVRHand hand;
//     [SerializeField] private GameObject radialProgressBar;


//     void Update()
//     {

//         if (hand == null || radialProgressBar == null) return;

//         bool isTracked = hand.IsTracked;

//         bool thumb = hand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);
//         bool middle = hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
//         bool ring = hand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
        
//         bool onlyThumbMiddle = thumb && middle;

//       Debug.Log($"[HPDet] Thumb: {thumb}, Middle: {middle}, Ring: {ring}");
      
//         bool gestureDetected = isTracked && onlyThumbMiddle && ring;

//         radialProgressBar.SetActive(gestureDetected);
//     }

//     // private bool IsPalmFacingCamera()
//     // {
//     //     var cameraTransform = Camera.main.transform;

//     //     // Palm transform = this GameObject (wrist-attached)
//     //     Vector3 palmForward = transform.forward; // forward from wrist = palm normal
//     //     Vector3 toCamera = (cameraTransform.position - transform.position).normalized;

//     //     float dot = Vector3.Dot(palmForward, toCamera);

//     //     return dot > palmFacingThreshold; // closer to 1 means facing
//     // }
// }

using UnityEngine;

public class HPDet : MonoBehaviour
{
    [SerializeField] private GameObject radialProgressBar;
    [SerializeField] private OVRHand hand;

    private bool wasBunnyLastFrame = false;
    private bool uiIsVisible = false;

    void Update()
    {
        if (hand == null || radialProgressBar == null) return;

        bool isTracked = hand.IsTracked;

        bool thumb = hand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);
        bool middle = hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
        bool ring = hand.GetFingerIsPinching(OVRHand.HandFinger.Ring);

        // Optional: if you want strict bunny (index/pinky must not be pinching)
        // bool index = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        // bool pinky = hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        bool bunnyNow = isTracked && thumb && middle && ring;

        if (bunnyNow && !wasBunnyLastFrame)
        {
            uiIsVisible = !uiIsVisible;
            radialProgressBar.SetActive(uiIsVisible);
            Debug.Log($"[HPDet] Bunny Gesture toggled radial UI: {(uiIsVisible ? "ON" : "OFF")}");
        }

        wasBunnyLastFrame = bunnyNow;
    }
}
