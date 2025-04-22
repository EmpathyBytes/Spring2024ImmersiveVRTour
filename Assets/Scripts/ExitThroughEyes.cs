using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitThroughEyes : MonoBehaviour
{
        [SerializeField] private float selectDistance;
        [SerializeField] private float eyedistance;
        [SerializeField] private string sceneName;
        [SerializeField] private OVRCameraRig OVRCameraRig;
        [SerializeField] private AudioSource buzzSound;
        private bool click = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (click) return;
        eyedistance = Vector3.Distance (this.transform.position, OVRCameraRig.centerEyeAnchor.position);
        if (eyedistance < selectDistance)
        {
            click = true;
            StartCoroutine(SoundAndSwitch());
        }  
    }
    private IEnumerator SoundAndSwitch() {
        if (buzzSound != null) {
            buzzSound.Play();
            AudioClip clip = buzzSound.GetComponent<AudioClip>();
            buzzSound.PlayOneShot(clip);
            yield return new WaitForSeconds(buzzSound.clip.length);
        }
        SceneManager.LoadScene(sceneName);
    }  
}
