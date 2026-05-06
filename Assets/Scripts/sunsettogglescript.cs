using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunsettogglescript : MonoBehaviour
{
    [Header("Interaction Targets")]
    [SerializeField] private GameObject leftIndex;
    [SerializeField] private GameObject rightIndex;
    [SerializeField] private GameObject leftControllerIndex;
    [SerializeField] private GameObject rightControllerIndex;

    [Header("Skybox Materials")]
    [SerializeField] private Material originalSkybox;
    [SerializeField] private Material megaSunMaterial;

    [Header("Bush Settings")]
    [SerializeField] private Renderer bushRenderer;
    [SerializeField] private Material customBushMaterial;

    [Tooltip("Enter the slot numbers you want to change (e.g., 2, 4, 5, 6 for the leaves)")]
    [SerializeField] private List<int> leafSlotIndexes = new List<int> { 2, 4, 5, 6 };

    private Material[] originalBushMaterials;

    [Header("Settings")]
    [SerializeField] private float selectDistance = 0.1f;
    [SerializeField] private AudioSource buzzSound;

    private bool isMegaSunActive = false;
    private bool isProcessing = false;

    void Start()
    {
        if (originalSkybox == null) originalSkybox = RenderSettings.skybox;

        if (bushRenderer != null)
        {
            // IMPORTANT: Use sharedMaterials here to get the 'blueprint' of the materials
            originalBushMaterials = bushRenderer.sharedMaterials;
        }
    }

    void Update()
    {
        if (isProcessing) return;

        if (IsPointNear(leftIndex) || IsPointNear(rightIndex) ||
            IsPointNear(leftControllerIndex) || IsPointNear(rightControllerIndex))
        {
            StartCoroutine(ToggleSkyboxRoutine());
        }
    }

    private bool IsPointNear(GameObject target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.transform.position) < selectDistance;
    }

    private IEnumerator ToggleSkyboxRoutine()
    {
        isProcessing = true;
        if (buzzSound != null) buzzSound.Play();

        // Create a temporary array to hold the materials we want to show
        Material[] currentMaterials = new Material[originalBushMaterials.Length];

        if (isMegaSunActive)
        {
            // TOGGLE OFF: Go back to normal
            RenderSettings.skybox = originalSkybox;
            currentMaterials = originalBushMaterials;
        }
        else
        {
            // TOGGLE ON: Apply Mega Sun and Swap Leaves
            RenderSettings.skybox = megaSunMaterial;

            // Copy all original materials first (so branches/bark stay the same)
            for (int i = 0; i < originalBushMaterials.Length; i++)
            {
                currentMaterials[i] = originalBushMaterials[i];
            }

            // Now, override ONLY the slots we listed in leafSlotIndexes
            foreach (int index in leafSlotIndexes)
            {
                if (index < currentMaterials.Length)
                {
                    currentMaterials[index] = customBushMaterial;
                }
            }
        }

        if (bushRenderer != null) bushRenderer.materials = currentMaterials;

        isMegaSunActive = !isMegaSunActive;
        DynamicGI.UpdateEnvironment();

        yield return new WaitForSeconds(1.0f);
        isProcessing = false;
    }
}