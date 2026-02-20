using UnityEngine;

public class PlayerHitDetector : MonoBehaviour
{
    public Rigidbody ball;
    public float softTapThreshold = 1.5f;
    public float smashThreshold = 4f;

    public float softTapPower = 2f;
    public float normalPower = 5f;
    public float smashPower = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Ball"))
            return;

        Vector3 paddleVel = rb.velocity;
        float speed = paddleVel.magnitude;

        Vector3 hitDir = (collision.transform.position - transform.position).normalized;

        float finalPower;

        if (speed < softTapThreshold)
        {
            finalPower = softTapPower; 
        }
        else if (speed < smashThreshold)
        {
            finalPower = normalPower;  
        }
        else
        {
            finalPower = smashPower; 
        }

        ball.velocity = hitDir * finalPower;
    }
}
