using UnityEngine;
using Oculus.Interaction;

public class BallScoringManager : MonoBehaviour
{
    [Header("References")]
    public Rigidbody ballRb;
    public Grabbable grabbable;
    public EnemyAI enemyAI;
    [Header("Zones")]
    public Collider playerSide;
    public Collider enemySide;
    public Collider floor;
    public Collider playerZone;

    [Header("Serve Settings")]
    public Transform playerServePoint;
    public Transform enemyServePoint;
    public float serveHeight = 1.2f;
    public float serveDelay = 1.2f;

    private int playerBounces = 0;
    private int enemyBounces = 0;

    private bool rallyActive = false;
    private string nextServer = "Player";

    private int totalPoints = 0;

    void Start()
    {
        if (!ballRb) ballRb = GetComponent<Rigidbody>();
        if (!grabbable) grabbable = GetComponent<Grabbable>();

        nextServer = (Random.value > 0.5f) ? "Player" : "Enemy";

        grabbable.WhenPointerEventRaised += OnPointerEvent;

        StartCoroutine(ServeAfterDelay(1f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!rallyActive) return;

        if (collision.collider == playerSide)
        {
            playerBounces++;
            enemyBounces = 0;

            ScoreBoardManager.Instance.IncrementRally();
            CheckBounceLogic("Player");
        }
        else if (collision.collider == enemySide)
        {
            enemyBounces++;
            playerBounces = 0;

            ScoreBoardManager.Instance.IncrementRally();
            CheckBounceLogic("Enemy");
        }
        else if (collision.collider == floor)
        {
            EndRally("Floor");
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect &&
            !rallyActive &&
            nextServer == "Player")
        {
            rallyActive = true;
            ballRb.isKinematic = false;
        }
    }

    private void CheckBounceLogic(string side)
    {
        if (side == "Player" && playerBounces >= 2)
        {
            ScoreBoardManager.Instance.AddEnemyPoint();
            EndRally("Enemy");
        }
        else if (side == "Enemy" && enemyBounces >= 2)
        {
            ScoreBoardManager.Instance.AddPlayerPoint();
            EndRally("Player");
        }
    }

    private void EndRally(string winner)
    {
        rallyActive = false;
        playerBounces = 0;
        enemyBounces = 0;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        totalPoints++;

        UpdateServer();

        ScoreBoardManager.Instance.IncrementRally();
        StartCoroutine(ServeAfterDelay(serveDelay));
    }


    private void UpdateServer()
    {
        if (totalPoints % 2 == 0)
        {
            nextServer = (nextServer == "Player") ? "Enemy" : "Player";
        }
    }

    private System.Collections.IEnumerator ServeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ServeBall(nextServer);
    }

    void ServeBall(string side)
    {
        if (!GameManager.GameIsActive)
            return;
        Transform servePoint = (side == "Player") ? playerServePoint : enemyServePoint;
        if (!servePoint) return;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        transform.position = servePoint.position + Vector3.up * serveHeight;

        if (side == "Enemy")
        {
            Invoke(nameof(AIServe), 0.8f);
        }
    }

    void AIServe()
    {
        ballRb.isKinematic = false;

        Vector3 targetPoint = enemyAI.GetRandomPointInZone(playerZone, 0.45f);

        Vector3 dir = (targetPoint - ballRb.position).normalized;
        float servePower = 4f;

        ballRb.AddForce(dir * servePower + Vector3.up * 1.5f, ForceMode.VelocityChange);

        rallyActive = true;
    }


    public void BeginNewServe(string server)
    {
        nextServer = server;

        rallyActive = false;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        StartCoroutine(ServeAfterDelay(0.5f));
    }

   
}
