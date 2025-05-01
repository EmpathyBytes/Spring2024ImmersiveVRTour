using UnityEngine;
using UnityEngine.UI;

public class progressbarfiller : MonoBehaviour
{
    [SerializeField] private objectCount objectCounter;
    [SerializeField] private Image fillImage;

    void Start()
    {
        if (fillImage != null)
            fillImage.fillAmount = 0f; // Make sure it's empty at the beginning
    }

    void Update()
    {
        if (objectCounter == null || fillImage == null) return;

        bool[] picked = objectCounter.getObjectsPicked();
        if (picked == null || picked.Length == 0) return;

        int collected = 0;
        foreach (bool b in picked)
            if (b) collected++;

        float fillAmount = (float)collected / picked.Length;
        fillImage.fillAmount = fillAmount;
    }
}
