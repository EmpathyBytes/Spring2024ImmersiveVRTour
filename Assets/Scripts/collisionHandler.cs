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

    private AudioSource audioSource;
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
            }

            /*Debug.Log(collision.gameObject.name);*/
            Debug.Log(drum.name);
        }
    }

    /*void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("drumSet"))
        {
            played = false;
        }
    }*/

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
}
