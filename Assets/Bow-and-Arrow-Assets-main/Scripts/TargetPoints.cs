using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoints : MonoBehaviour
{

    public int pointAmount = 0;
    public PointCount pointCounter;

    private Renderer targetRenderer;
    public Color flashColor = Color.green;
    public float flashDuration = 0.2f;
    private Color originalColor;

    private bool hasHit = false;


    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
            originalColor = targetRenderer.material.color;
        
            
    }
    
    void OnTriggerEnter(Collider other) {

        if (hasHit) return;

        if (other.CompareTag("Arrow"))
        {
            if (pointCounter != null)
            {
                hasHit = true;
                pointCounter.addPointCount(pointAmount);
                StartCoroutine(FlashTarget());
            }
        }
    }

    private IEnumerator FlashTarget(){
        if (targetRenderer == null) yield break;

        targetRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        targetRenderer.material.color = originalColor;
        hasHit = false;
    }
}
