using UnityEngine;

public class PaddleBouncePhysics : MonoBehaviour
{
    [Header("Bounce Tuning")]
    public float bounceMultiplier = 1.2f;   
    public float minBounceSpeed = 0.5f;    
    public float maxBounceSpeed = 8f;       

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Paddle"))
            return;

        Rigidbody paddleRb = collision.rigidbody;
        if (paddleRb == null)
            return;

   
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        
        Vector3 incomingVelocity = rb.velocity - paddleRb.velocity * 0.5f;

        Vector3 reflected = Vector3.Reflect(incomingVelocity, normal);

      
        Vector3 addedForce = paddleRb.velocity * bounceMultiplier;

        Vector3 newVelocity = reflected + addedForce;

        float speed = newVelocity.magnitude;
        if (speed < minBounceSpeed) newVelocity = newVelocity.normalized * minBounceSpeed;
        else if (speed > maxBounceSpeed) newVelocity = newVelocity.normalized * maxBounceSpeed;

        rb.velocity = newVelocity;
    }
}