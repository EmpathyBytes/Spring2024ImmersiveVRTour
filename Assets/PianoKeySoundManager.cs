using UnityEngine;
using System.Collections.Generic;

public class PianoKeySoundManager : MonoBehaviour
{
    public static PianoKeySoundManager Instance;

    [SerializeField] private AudioSource audioSource;

    [System.Serializable]
    public struct NoteAudio
    {
        public string noteName;
        public AudioClip clip;
    }

    [SerializeField] private List<NoteAudio> noteClips;
    private Dictionary<string, AudioClip> noteMap = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        foreach (var note in noteClips)
        {
            noteMap[note.noteName] = note.clip;
        }
    }

    public void PlayNote(string noteName)
    {
        if (noteMap.TryGetValue(noteName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Note '{noteName}' not found!");
        }
    }
}

