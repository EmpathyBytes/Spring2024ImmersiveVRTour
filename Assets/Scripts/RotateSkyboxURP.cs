using UnityEngine;

public class RotateSkyboxURP : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 0.2f;
    float rot;

    void Update()
    {
        rot += degreesPerSecond * Time.deltaTime;
        if (rot >= 360f) rot -= 360f;

        // Panoramic skybox materials expose _Rotation in most Unity/URP versions
        if (RenderSettings.skybox && RenderSettings.skybox.HasProperty("_Rotation"))
            RenderSettings.skybox.SetFloat("_Rotation", rot);
    }
}
