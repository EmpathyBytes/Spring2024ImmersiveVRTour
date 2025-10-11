using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab;      
    public Transform spawnPoint;      
    public bool arrowNotched = false;
    private bool bowGrabbed = false;
    public float spawnDelay = 1f;
    

    public GameObject currentArrow = null;

    public Renderer notchRenderer;

    void Start()
    {
        
        PullInteraction.PullReleased += NotchEmpty;

    }

    private void OnDestroy() {
    PullInteraction.PullReleased -= NotchEmpty;
    }

    // Update is called once per frame
    void Update()
    {

        if (bowGrabbed && !arrowNotched) {
            arrowNotched = true;
            StartCoroutine(SpawnArrowDelayed());
        }

        if (!bowGrabbed && currentArrow != null) {
            Destroy(currentArrow);
            NotchEmpty(1f);
        }
    }

    private void NotchEmpty(float value) {
        arrowNotched = false;
        currentArrow = null;
    }


    public void OnGrabbed() {

        bowGrabbed = true;
    }

    public void OnReleased() {
        bowGrabbed = false;
        if (notchRenderer != null)
        {
            notchRenderer.material.color = Color.white;
        }
    }

    IEnumerator SpawnArrowDelayed()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (arrowPrefab != null && spawnPoint != null)
        {
            currentArrow = Instantiate(arrowPrefab, spawnPoint.transform);
            if (notchRenderer != null)
        {
            notchRenderer.material.color = Color.red;
        }
        }
        else
        {
            Debug.LogWarning("ArrowPrefab or SpawnPoint not assigned!");
        }
    }
}
