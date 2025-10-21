using UnityEngine;
using Oculus.Interaction;

public class GameActivationManager : MonoBehaviour
{
    [Header("References")]
    public GameObject playerBox;                 
    public Grabbable paddleGrabbable;           
    public ActiveStateSelector thumbsUpLeft;    
    public ActiveStateSelector thumbsDownLeft;   

    private bool gameStarted = false;

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
        if (playerBox != null)
            playerBox.SetActive(true);  
        Debug.Log("Game started — player locked in box.");
    }

    private void UnlockPlayer()
    {
        gameStarted = false;
        if (playerBox != null)
            playerBox.SetActive(false);
        Debug.Log("Game ended — player unlocked.");
    }
}
