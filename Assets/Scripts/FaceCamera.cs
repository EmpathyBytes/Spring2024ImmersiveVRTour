using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;

    void Start()
    {
        // If none assigned, fall back to Camera.main
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetCamera != null)
        {
            transform.LookAt(targetCamera);
            transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
    }
}
