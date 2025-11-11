using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullInteraction : MonoBehaviour
{

    public Transform notch;
    public Transform stringCenter;
    public Transform startNotch;
    public Vector3 startCenter;
    public LineRenderer lineRenderer;

    public LineRenderer trajectory;

    public Transform arrowNotch;
    public Vector3 arrowNotchStart;

    public Vector3 delta;

    public float minPull = -0.21f; 
    public float maxPull = 0.0f; 
    public float returnSpeed = 2f;

    private bool isGrabbed;

    public float pullAmount;

    public static event System.Action<float> PullReleased;

    public Renderer notchRenderer;//debugging


    // Start is called before the first frame update
    void Start()
    {
        startNotch.localPosition = notch.localPosition;
        startCenter = lineRenderer.GetPosition(1);
        arrowNotchStart = arrowNotch.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        if (!isGrabbed)
        {
            notch.localPosition = Vector3.MoveTowards(
                notch.localPosition,
                startNotch.localPosition,
                returnSpeed * Time.deltaTime
            );
        }

        delta = notch.localPosition - startNotch.localPosition;
        float zDelta = delta.z;
        float clampedZ = Mathf.Clamp(zDelta, minPull, maxPull);

        Vector3 arrowPos = arrowNotch.localPosition;
        arrowPos.x = 0;
        arrowPos.y = 0;
        arrowPos.z = arrowNotchStart.z + clampedZ * 1.4286f;
        arrowNotch.localPosition = arrowPos;

        stringCenter.localPosition = startCenter + new Vector3(0f, 0f, clampedZ);
        lineRenderer.SetPosition(1, stringCenter.localPosition);

        pullAmount = Mathf.InverseLerp(maxPull, minPull, clampedZ);

        if (trajectory != null)
        {
            if (isGrabbed){
                trajectory.enabled = true;
            } else{
                trajectory.enabled = false;
            }
        }

    }

    public void OnGrabbed(){
        isGrabbed = true;
    }

    public void OnReleased(){
        isGrabbed = false;

        float normalizedPull = Mathf.InverseLerp(maxPull, minPull, delta.z);
        pullAmount = normalizedPull; // multiply by scale here

        PullReleased?.Invoke(pullAmount);

        notchRenderer.material.color = Color.blue;
    }

    public bool IsGrabbed() {
        return isGrabbed;
    }
}
