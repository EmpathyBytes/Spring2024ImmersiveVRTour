using UnityEngine;

public class PianoKeyTriggerHandler : MonoBehaviour
{
    [SerializeField] private string noteName; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerFinger"))
        {
            PianoKeySoundManager.Instance.PlayNote(noteName);
        }
    }
}
