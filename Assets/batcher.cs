using UnityEngine;

public class FixPianoKeyRotation : MonoBehaviour
{
    public GameObject[] keys;

 [ContextMenu("reAlign Piano Keys")]
    void Start()
    {
        foreach (GameObject key in keys)
        {
            key.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Try (90f, 0f, 0f) if needed
        }
    }
}

