using UnityEngine;

public class LightControl : MonoBehaviour
{
    public Light targetLight;
    public LightMaterialSync barSync;

    public float levelA = 0f; 
    public float levelB = 0.1f;
    public float levelC = 0.4f;
    public float levelD = 0.8f;
    public float levelE = 1.5f;

    public void SetLevel(int level)
    {
        switch (level)
        {
            case 0: targetLight.intensity = levelA; break;
            case 1: targetLight.intensity = levelB; break;
            case 2: targetLight.intensity = levelC; break;
            case 3: targetLight.intensity = levelD; break;
            case 4: targetLight.intensity = levelE; break;
        }

        if (barSync != null)
        {
            barSync.SetLevel(level);
        }
    }
}