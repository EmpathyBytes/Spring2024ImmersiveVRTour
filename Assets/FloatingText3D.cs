using UnityEngine;
using TMPro;

public class FloatingText3D : MonoBehaviour
{
    public float floatSpeed = 0.2f;   // How fast the text rises
    public float fadeDuration = 1f;   // How long until it disappears

    private TextMeshPro textMesh;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        startColor = textMesh.color;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (Camera.main != null)
        {
            Vector3 dir = Camera.main.transform.position - transform.position;
            dir.y = 0; // keep upright
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
                transform.Rotate(0, 180f, 0); // flip if mirrored
            }
        }

        startColor.a -= Time.deltaTime / fadeDuration;
        textMesh.color = startColor;

        if (startColor.a <= 0f)
            Destroy(gameObject);
    }

    public void SetText(string message, Color color)
    {
        textMesh.text = message;
        textMesh.color = color;
        startColor = color;
    }
}
