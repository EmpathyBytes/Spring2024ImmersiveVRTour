using TMPro;
using UnityEngine;

public class RigHeightAdjuster : MonoBehaviour
{
    public Transform rigRoot; 
    public float moveSpeed = 0.5f; 
    public float lerpSpeed = 3f;   
    public float minHeight = -2.0f;
    public float maxHeight = 2.5f;

    private bool movingUp;
    private bool movingDown;
    private float targetHeight;
    private float startHeight;

    public TextMeshProUGUI heightLabel;


    void Start()
    {
        startHeight = rigRoot.position.y;
        targetHeight = startHeight;
    }

    void Update()
    {
        if (movingUp)
            AdjustHeight(moveSpeed * Time.deltaTime);
        else if (movingDown)
            AdjustHeight(-moveSpeed * Time.deltaTime);

        Vector3 pos = rigRoot.position;
        pos.y = Mathf.Lerp(pos.y, targetHeight, Time.deltaTime * lerpSpeed);
        rigRoot.position = pos;
        if (heightLabel)
            heightLabel.text = $"{pos.y:F2} m";
    }

    public void StartMoveUp() => movingUp = true;
    public void StopMoveUp() => movingUp = false;
    public void StartMoveDown() => movingDown = true;
    public void StopMoveDown() => movingDown = false;
    public void AdjustHeight(float delta)
    {
        float baseHeight = startHeight;
        targetHeight = Mathf.Clamp(targetHeight + delta, baseHeight + minHeight, baseHeight + maxHeight);
    }
}