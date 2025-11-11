using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArrowTrajectoryVR : MonoBehaviour
{
    [Header("References")]
    public Transform arrowStart;          // Where the arrow is nocked
    public PullInteraction pullInteraction; // Reference to your bow string script

    [Header("Trajectory Settings")]
    public int segmentCount = 30;         // Number of points in the path
    public float timeStep = 0.1f;         // Time between points
    public float maxArrowSpeed = 10f;     // Max arrow speed at full pull
    public LayerMask collisionMask;       // Layers to check for trajectory collision

    public Arrow arrow;
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
    }

    void Update()
    {
        if (arrowStart == null || pullInteraction == null)
            return;

        DrawTrajectory();
    }

    void DrawTrajectory()
    {
        Vector3[] points = new Vector3[segmentCount];

        // Calculate initial velocity based on pullAmount
        float arrowSpeed = maxArrowSpeed * pullInteraction.pullAmount;
        Vector3 pos = arrowStart.position;
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        Vector3 velocity = arrowStart.forward * arrowSpeed / rb.mass;
        velocity *= (1 - rb.drag * timeStep);

        velocity *= 0.95f;


        for (int i = 0; i < segmentCount; i++)
        {
            points[i] = pos;

            // Predict next position
            Vector3 nextPos = pos + velocity * timeStep;
            
            // Check for collision
            if (Physics.Linecast(pos, nextPos, out RaycastHit hit, collisionMask))
            {
                points[i] = hit.point; // Stop line at collision
                // Clear remaining points
                for (int j = i + 1; j < segmentCount; j++)
                    points[j] = hit.point;
                break;
            }

            // Apply gravity
            velocity += Physics.gravity * timeStep;
            pos = nextPos;
        }

        lineRenderer.SetPositions(points);
    }
}
