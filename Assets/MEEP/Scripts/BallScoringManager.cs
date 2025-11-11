using UnityEngine;
using TMPro;
using Oculus.Interaction; 

public class BallScoringManager : MonoBehaviour
{
    [Header("References")]
    public Rigidbody ballRb;
    public Grabbable grabbable; 

    [Header("Zones")]
    public Collider playerSide;
    public Collider enemySide;
    public Collider floor;

    [Header("Serve Settings")]
    public Transform playerServePoint;
    public Transform enemyServePoint;
    public float serveHeight = 1.2f;
    public float serveDelay = 2f;

    [Header("Score UI")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;

    private int playerBounces = 0;
    private int enemyBounces = 0;
    private int playerScore = 0;
    private int enemyScore = 0;

    private bool rallyActive = false;
    private string nextServer = "Player"; 

    void Start()
    {
        if (ballRb == null) ballRb = GetComponent<Rigidbody>();
        if (grabbable == null) grabbable = GetComponent<Grabbable>();

        nextServer = (Random.value > 0.5f) ? "Player" : "Enemy";

        grabbable.WhenPointerEventRaised += OnPointerEvent;

        UpdateScoreUI();
        StartCoroutine(ServeAfterDelay(1f));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!rallyActive) return;

        if (collision.collider == playerSide)
        {
            playerBounces++;
            enemyBounces = 0;
            CheckScoring("Player");
        }
        else if (collision.collider == enemySide)
        {
            enemyBounces++;
            playerBounces = 0;
            CheckScoring("Enemy");
        }
        else if (collision.collider == floor)
        {
            Debug.Log("Ball hit floor");
            EndRally("Floor");
        }
    }

    void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Unselect && !rallyActive && nextServer == "Player")
        {
            rallyActive = true;
            ballRb.isKinematic = false;
        }
    }

    void CheckScoring(string side)
    {
        if (side == "Player" && playerBounces >= 2)
        {
            enemyScore++;
            nextServer = "Enemy";
            EndRally("Enemy");
        }
        else if (side == "Enemy" && enemyBounces >= 2)
        {
            playerScore++;
            nextServer = "Player";
            EndRally("Player");
        }
    }

    void EndRally(string winner)
    {
        rallyActive = false;
        UpdateScoreUI();

        playerBounces = 0;
        enemyBounces = 0;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        StartCoroutine(ServeAfterDelay(serveDelay));
    }

    System.Collections.IEnumerator ServeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ServeBall(nextServer);
    }

    void ServeBall(string side)
    {
        Transform servePoint = (side == "Player") ? playerServePoint : enemyServePoint;
        if (servePoint == null) return;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        transform.position = servePoint.position + Vector3.up * serveHeight;

        if (side == "Enemy")
        {
            Invoke(nameof(AIServe), 1f);
        }

    }

    void AIServe()
    {
        ballRb.isKinematic = false;
        ballRb.AddForce(Vector3.forward * 3f + Vector3.up * 2f, ForceMode.VelocityChange);
        rallyActive = true;
    }

    void UpdateScoreUI()
    {
        if (playerScoreText) playerScoreText.text = $"Player: {playerScore}";
        if (enemyScoreText) enemyScoreText.text = $"Enemy: {enemyScore}";
    }
}
