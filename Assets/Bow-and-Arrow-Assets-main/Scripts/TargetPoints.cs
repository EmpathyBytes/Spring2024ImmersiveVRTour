using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoints : MonoBehaviour
{

    public int pointAmount = 0;

    private Renderer targetRenderer;
    public float flashAmount = 1.5f;
    public float flashDuration = 0.2f;
    private Color originalColor;

    private bool hasHit = false;
    public ArcheryTarget controller;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
            originalColor = targetRenderer.material.color;
        
            
    }
    
    void OnTriggerEnter(Collider other) {

        

        if (other.CompareTag("Arrow"))
        {

            //controller.hitDecide(this);

        }
    }

    public void Hit(PointCount pointCounter){
        if (hasHit) return;
        hasHit = true;
        pointCounter.addPointCount(pointAmount);
        StartCoroutine(FlashTarget());
    }

    private IEnumerator FlashTarget(){
        if (targetRenderer == null) yield break;

        Color flashColor = Color.Lerp(originalColor, Color.white, flashAmount);

        targetRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        targetRenderer.material.color = originalColor;

        hasHit = false;

        controller.ResetScored();
    }
    public Color getColor(){
        return originalColor;
    }
}
