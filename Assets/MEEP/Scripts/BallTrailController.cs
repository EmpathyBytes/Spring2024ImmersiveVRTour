using UnityEngine;

public class BallTrailController : MonoBehaviour
{
    public TrailRenderer trail;
    public float minTime = 0.05f;
    public float maxTime = 0.25f;
    public float minWidth = 0.005f;
    public float maxWidth = 0.02f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;


        float t = Mathf.Clamp01(speed / 10f);

  
        trail.time = Mathf.Lerp(minTime, maxTime, t);

  
        trail.startWidth = Mathf.Lerp(minWidth, maxWidth, t);
        trail.endWidth = 0f;

     
        Color c = Color.white;
        c.a = Mathf.Lerp(0.05f, 0.4f, t);
        trail.startColor = c;
        trail.endColor = new Color(c.r, c.g, c.b, 0);
    }
}