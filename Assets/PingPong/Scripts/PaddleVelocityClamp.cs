using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Grabbable))]
public class PaddleVelocityClamp : MonoBehaviour
{
    public float maxLinearVelocity = 2f;
    public float maxAngularVelocity = 5f;

    private Rigidbody rb;
    private Grabbable grabbable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
    }

    void FixedUpdate()
    {
        if (grabbable.SelectingPointsCount > 0) 
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxLinearVelocity);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularVelocity);
        }
    }
}