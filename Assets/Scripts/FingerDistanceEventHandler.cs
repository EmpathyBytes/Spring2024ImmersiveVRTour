using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FingerDistanceEventHandler : MonoBehaviour
{
        [SerializeField] private GameObject leftIndex;
        [SerializeField] private GameObject rightIndex;
        [SerializeField] private GameObject leftControllerIndex;
        [SerializeField] private GameObject rightControllerIndex;
        [SerializeField] private float lidistance;
        [SerializeField] private float ridistance;
        [SerializeField] private float licdistance;
        [SerializeField] private float ricdistance;
        [SerializeField] private float hoverDistance = 0.1f;
        [SerializeField] private float selectDistance = 0.02f;
        [SerializeField] private string sceneName;
        [SerializeField] private Material yellow;
        [SerializeField] private Material white;
        [SerializeField] private GameObject gatechMap;
        [SerializeField] private GameObject OVRCameraRig;
        [SerializeField] private GameObject infoCard;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        lidistance = Vector3.Distance (this.transform.position, leftIndex.transform.position);
        ridistance = Vector3.Distance (this.transform.position, rightIndex.transform.position);
        licdistance = Vector3.Distance (this.transform.position, leftControllerIndex.transform.position);
        ricdistance = Vector3.Distance (this.transform.position, rightControllerIndex.transform.position);
        if (lidistance < selectDistance || ridistance < selectDistance || licdistance < selectDistance || ricdistance < selectDistance)
        {
            SceneManager.LoadScene(sceneName);
        } else if (lidistance < hoverDistance || ridistance < hoverDistance || licdistance < hoverDistance || ricdistance < hoverDistance) {
            this.GetComponent<MeshRenderer> ().material = white;
            infoCard.SetActive(true);
        } else {
            this.GetComponent<MeshRenderer> ().material = yellow;
            infoCard.SetActive(false);
        }
    }
}
