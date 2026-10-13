using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Rigidbody paddleRb;
    public Rigidbody ball;
    public Transform hitTarget;

    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float reactionTime = 0.12f;
    public float maxPower = 9f;
    public float minPower = 4f;

    [Header("Stats")]
    public int hitsAttempted = 0;
    public int hitsSuccessful = 0;

    [Header("AI Target Zones")]
    public Collider playerZone;


    public float minX = -15f;
    public float maxX = 15f;

    public float minZ = -15f;
    public float maxZ = 15f;

    public float minY = -3f;
    public float maxY = 3f;



    private bool canSwing = true;

    void Update()
    {
        if (!GameManager.GameIsActive)
            return;
        FollowBallXZ();
        TrySwing();
    }

    void LateUpdate()
    {

        if (!GameManager.GameIsActive)
            return;
        Vector3 pos = transform.position;
   
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }

    void FollowBallXZ()
    {
        Vector3 target = new Vector3(ball.position.x, paddleRb.position.y, ball.position.z);
        Vector3 nextPos = Vector3.Lerp(paddleRb.position, target, moveSpeed * Time.deltaTime);

        paddleRb.MovePosition(nextPos);
    }

    void TrySwing()
    {
        if (!canSwing) return;

        if ((ball.position - paddleRb.position).magnitude < 0.28f)
        {
            hitsAttempted++;
            StartCoroutine(SwingRoutine());
        }
    }

    System.Collections.IEnumerator SwingRoutine()
    {
        canSwing = false;

        yield return new WaitForSeconds(reactionTime);

        if ((ball.position - paddleRb.position).magnitude < 0.35f)
        {
            HitBall();
            hitsSuccessful++;
        }

        yield return new WaitForSeconds(0.25f);
        canSwing = true;
    }

    void HitBall()
    {
        Vector3 targetPoint = GetRandomPointInZone(playerZone);

        Vector3 dir = (targetPoint - ball.position).normalized;

        float power = Mathf.Lerp(minPower, maxPower,
            Mathf.Clamp01(ball.velocity.magnitude / 7f));

        ball.velocity = dir * power;
    }

    public float GetAccuracy()
    {
        if (hitsAttempted == 0) return 1f;
        return (float)hitsSuccessful / hitsAttempted;
    }

    public Vector3 GetRandomPointInZone(Collider zone, float bias = 0.6f)
    {
        Bounds b = zone.bounds;

        Vector3 randomPos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z)
        );

        return Vector3.Lerp(randomPos, b.center, bias);
    }

}
