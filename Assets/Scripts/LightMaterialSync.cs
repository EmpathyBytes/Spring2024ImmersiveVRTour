using UnityEngine;

public class LightMaterialSync : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    public void SetLevel(int level)
    {
        if (targetRenderer == null)
        {
            return;
        }

        var mat = targetRenderer.material;

        Color c = GetColor(level);

        mat.SetColor("_BaseColor", c);
        mat.SetColor("_EmissionColor", c);
        mat.EnableKeyword("_EMISSION");
    }

    private Color GetColor(int level)
    {
        switch (level)
        {
            case 0: return new Color32(20, 20, 20, 255);   // off
            case 1: return new Color32(100, 100, 100, 255);
            case 2: return new Color32(140, 140, 140, 255);
            case 3: return new Color32(170, 170, 170, 255);
            case 4: return new Color32(200, 200, 200, 255);
            default: return Color.black;
        }
    }
}