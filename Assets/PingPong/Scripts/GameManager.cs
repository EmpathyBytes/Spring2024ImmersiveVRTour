using UnityEngine;
using Oculus.Interaction;

public class GameManager : MonoBehaviour
{
    public GameObject playerBox;                 
    public Grabbable paddleGrabbable;           
    public ActiveStateSelector thumbsUpLeft;    
    public ActiveStateSelector thumbsDownLeft;
    public Transform ballServePoint;           
    public BallScoringManager scoringManager;  
    public GameObject ballPrefab;              
    public Rigidbody ballRb;

    private bool gameStarted = false;
    public static bool GameIsActive = false;

    private void OnEnable()
    {
        if (thumbsUpLeft != null)
            thumbsUpLeft.WhenSelected += OnThumbsUp;

        if (thumbsDownLeft != null)
            thumbsDownLeft.WhenSelected += OnThumbsDown;
    }

    private void OnDisable()
    {
        if (thumbsUpLeft != null)
            thumbsUpLeft.WhenSelected -= OnThumbsUp;

        if (thumbsDownLeft != null)
            thumbsDownLeft.WhenSelected -= OnThumbsDown;
    }

    private void Update()
    {
        if (gameStarted && paddleGrabbable.SelectingPointsCount == 0)
        {
            UnlockPlayer();
        }
    }

    private void OnThumbsUp()
    {
        if (!gameStarted && paddleGrabbable.SelectingPointsCount > 0)
        {
            LockPlayerInBox();
        }
    }

    private void OnThumbsDown()
    {
        if (gameStarted)
        {
            UnlockPlayer();
        }
    }

    private void LockPlayerInBox()
    {
        gameStarted = true;
        GameIsActive = true;
        if (playerBox != null)
            playerBox.SetActive(true);

        StartServe();
    }

    private void UnlockPlayer()
    {
        gameStarted = false;
        GameIsActive = false;
        if (playerBox != null)
            playerBox.SetActive(false);

        ResetBall();
    }

    private void StartServe()
    {
        if (!ballServePoint) return;

        if (ballRb != null)
        {
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.isKinematic = true;
            ballRb.transform.position = ballServePoint.position;
        }
        else if (ballPrefab != null)
        {
            GameObject newBall = Instantiate(ballPrefab, ballServePoint.position, Quaternion.identity);
            ballRb = newBall.GetComponent<Rigidbody>();
        }

        if (scoringManager != null)
        {
            scoringManager.BeginNewServe("Player");
        }
    }

    private void ResetBall()
    {
        if (ballRb != null)
        {
            ballRb.velocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.isKinematic = true;
            ballRb.transform.position = ballServePoint.position;
        }
    }
}