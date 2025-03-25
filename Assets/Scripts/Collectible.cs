using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using UnityEngine;

public class Collectible :  MonoBehaviour
{
    public int scoreValue = 0;  
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

    // Start is called before the first frame update
    private bool scoreAdded = false;
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
            if (!scoreAdded) {
             scoreValue++;
             Debug.Log("Score increased to: " + scoreValue);
             scoreAdded = true; 
            }
          
        }
    }

   
}

