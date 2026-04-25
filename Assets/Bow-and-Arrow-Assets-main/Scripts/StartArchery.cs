using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartArchery : MonoBehaviour
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
        private bool isHovering = false;

        public GameObject PointSystem;
        public GameObject Bow;

        private bool gameStart = false;

        [SerializeField] private ArrowSpawner arrowSpawner;
        [SerializeField] private PointCount pointCount;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        lidistance = Vector3.Distance(this.transform.position, leftIndex.transform.position);
        ridistance = Vector3.Distance(this.transform.position, rightIndex.transform.position);
        licdistance = Vector3.Distance(this.transform.position, leftControllerIndex.transform.position);
        ricdistance = Vector3.Distance(this.transform.position, rightControllerIndex.transform.position);

        float closestDistance = Mathf.Min(lidistance, ridistance, licdistance, ricdistance);

        if (closestDistance < selectDistance)
        {
            if (!isHovering && !click)
            {
                click = true;
                isHovering = true;
                StartCoroutine(StartGame());
            }
        }
        else
        {
            isHovering = false;
        }
    }

    private IEnumerator StartGame() 
    {
        if (buzzSound != null && buzzSound.clip != null) 
        {
            buzzSound.PlayOneShot(buzzSound.clip);
            yield return new WaitForSeconds(buzzSound.clip.length);
        }

        if (arrowSpawner != null)
        {
            arrowSpawner.ResetAllArrowsExternal();
            pointCount.UpdateScoreTextExternal();
        }

        gameStart = !gameStart;
        PointSystem.SetActive(gameStart);
        Bow.SetActive(gameStart);

        

        click = false;
    }  
}
