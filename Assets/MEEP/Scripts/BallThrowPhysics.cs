using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))]
public class BallThrowPhysics : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForce = 1.4f;
    public float angularForce = 1.0f;
    public float forwardBias = 0.4f;
    public float maxSpeed = 10f;

    private Rigidbody rb;
    private Grabbable grabbable;
    private Vector3 lastPos;
    private Quaternion lastRot;
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;

    private bool wasGrabbed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
    }

    private void FixedUpdate()
    {
        bool isGrabbed = grabbable.SelectingPointsCount > 0;

        if (isGrabbed)
        {
            linearVelocity = (transform.position - lastPos) / Time.fixedDeltaTime;
            angularVelocity = GetAngularVelocity(transform.rotation, lastRot);

            lastPos = transform.position;
            lastRot = transform.rotation;
            wasGrabbed = true;
        }
        else if (wasGrabbed)
        {
            wasGrabbed = false;
            ThrowBall();
        }
    }

    private void ThrowBall()
    {
        Vector3 throwVel = linearVelocity * throwForce + transform.forward * forwardBias;
        Vector3 throwAng = angularVelocity * angularForce;

        rb.isKinematic = false;
        rb.velocity = Vector3.ClampMagnitude(throwVel, maxSpeed);
        rb.angularVelocity = throwAng;
    }

    private Vector3 GetAngularVelocity(Quaternion current, Quaternion previous)
    {
        Quaternion deltaRotation = current * Quaternion.Inverse(previous);
        float angle;
        Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);
        if (angle > 180f) angle -= 360f;
        if (Mathf.Abs(angle) < 0.001f) return Vector3.zero;
        return axis * angle * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}
