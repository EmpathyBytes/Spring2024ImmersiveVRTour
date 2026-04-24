using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSky : MonoBehaviour
{
        [SerializeField] private GameObject leftIndex;
        [SerializeField] private GameObject rightIndex;
        [SerializeField] private GameObject leftControllerIndex;
        [SerializeField] private GameObject rightControllerIndex;
        [SerializeField] private float lidistance;
        [SerializeField] private float ridistance;
        [SerializeField] private float licdistance;
        [SerializeField] private float ricdistance;
        [SerializeField] private float selectDistance;
        [SerializeField] private GameObject OVRCameraRig;
        [SerializeField] private AudioSource buzzSound;
        private bool click = false;
        
        public Material daySkybox;
        public Material nightSkybox;
        public Light sunLight;

        private bool isNight = false;

        private Material originalDaySkybox;

    // Start is called before the first frame update
    void Start()
    {
        if (daySkybox == null)
        {
            daySkybox = RenderSettings.skybox;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (click) return;
        lidistance = Vector3.Distance (this.transform.position, leftIndex.transform.position);
        ridistance = Vector3.Distance (this.transform.position, rightIndex.transform.position);
        licdistance = Vector3.Distance (this.transform.position, leftControllerIndex.transform.position);
        ricdistance = Vector3.Distance (this.transform.position, rightControllerIndex.transform.position);
        if (lidistance < selectDistance || ridistance < selectDistance || licdistance < selectDistance || ricdistance < selectDistance)
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
        
        if (isNight)
        {
            // Switch to DAY
            RenderSettings.skybox = daySkybox;
            if (sunLight != null)
            {
                sunLight.intensity = 1f;
                sunLight.color = Color.white;
            }
            isNight = false;
            Debug.Log("Switched to DAY");
        }
        else
        {
            // Switch to NIGHT
            RenderSettings.skybox = nightSkybox;
            if (sunLight != null)
            {
                sunLight.intensity = 0.2f;
                sunLight.color = new Color(0.3f, 0.3f, 0.5f);
            }
            isNight = true;
            Debug.Log("Switched to NIGHT");
        }
        
        // Force skybox update
        DynamicGI.UpdateEnvironment();
    }  
}
