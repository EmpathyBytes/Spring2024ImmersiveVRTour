using UnityEngine;

public class PianoKeyAligner : MonoBehaviour
{
    public float spacing = 0.021f;
    public string keyPrefix = "K";
    public int keyCount = 49;

    [ContextMenu("Align Piano Keys")]
    void AlignKeys()
    {
        for (int i = 0; i < keyCount; i++)
        {
            string keyName = keyPrefix + (i + 1);
            Transform key = transform.Find(keyName);
            if (key != null)
            {
                key.localPosition = new Vector3(i * spacing, 0f, 0f);
            }
            else
            {
                Debug.LogWarning("Missing key: " + keyName);
            }
        }
    }
[ContextMenu("Backup Key Positions")]
void BackupOriginalPositions()
{
    for (int i = 0; i < keyCount; i++)
    {
        string keyName = keyPrefix + (i + 1);
        Transform key = transform.Find(keyName);
        if (key != null)
        {
            PlayerPrefs.SetFloat(keyName + "_x", key.localPosition.x);
            PlayerPrefs.SetFloat(keyName + "_y", key.localPosition.y);
            PlayerPrefs.SetFloat(keyName + "_z", key.localPosition.z);
        }
    }
    Debug.Log("Backed up original positions.");
}

[ContextMenu("Restore Key Positions")]
void RestoreOriginalPositions()
{
    for (int i = 0; i < keyCount; i++)
    {
        string keyName = keyPrefix + (i + 1);
        Transform key = transform.Find(keyName);
        if (key != null && PlayerPrefs.HasKey(keyName + "_x"))
        {
            float x = PlayerPrefs.GetFloat(keyName + "_x");
            float y = PlayerPrefs.GetFloat(keyName + "_y");
            float z = PlayerPrefs.GetFloat(keyName + "_z");
            key.localPosition = new Vector3(x, y, z);
        }
    }
    Debug.Log("Restored original positions.");
}


}
