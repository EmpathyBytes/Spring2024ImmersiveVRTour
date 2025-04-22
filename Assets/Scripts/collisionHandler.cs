using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class collisionHandler : MonoBehaviour
{
    [SerializeField] private AudioClip snare;
    [SerializeField] private AudioClip hihat;
    [SerializeField] private AudioClip lowtom;
    [SerializeField] private AudioClip hightom;
    [SerializeField] private AudioClip ride;
    [SerializeField] private AudioClip crash;
    public bool drumPlayed;

    private AudioSource audioSource;
    private bool played = false;
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("drumSet"))
        {
            GameObject drum = GameObject.Find("DrumCollider");
            audioSource = drum.GetComponent<AudioSource>();
            AudioClip clipToPlay = GetSound(collision.gameObject.name);

            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
                played = true;
                drumPlayed = true;
            }

            /*Debug.Log(collision.gameObject.name);*/
            Debug.Log(drum.name);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("drumSet"))
        {
            played = false;
        }
    }

    AudioClip GetSound(string drumName)
    {
        switch (drumName)
        {
            case "SnareDrum":
                return snare;
            case "HiHat":
                return hihat;
            case "LowTom":
                return lowtom;
            case "HiTom":
                return hightom;
            case "RideCymbal":
                return ride;
            case "CrashCymbal":
                return crash;
            default:
                return null;
        }
    }

    public bool getDrumPlayed() {
        return drumPlayed;
    }
}
