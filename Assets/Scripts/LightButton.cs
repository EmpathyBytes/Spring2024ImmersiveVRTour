using System.Collections;
using UnityEngine;

public class LightButton : MonoBehaviour
{
    [SerializeField] private GameObject leftIndex;
    [SerializeField] private GameObject rightIndex;
    [SerializeField] private GameObject leftControllerIndex;
    [SerializeField] private GameObject rightControllerIndex;

    [SerializeField] private float selectDistance = 0.05f;

    [SerializeField] private LightDimmer lightDimmer;

    private bool clicked = false;

    void Update()
    {
        if (clicked) return;

        float lidistance = Vector3.Distance(transform.position, leftIndex.transform.position);
        float ridistance = Vector3.Distance(transform.position, rightIndex.transform.position);
        float licdistance = Vector3.Distance(transform.position, leftControllerIndex.transform.position);
        float ricdistance = Vector3.Distance(transform.position, rightControllerIndex.transform.position);

        if (lidistance < selectDistance ||
            ridistance < selectDistance ||
            licdistance < selectDistance ||
            ricdistance < selectDistance)
        {
            clicked = true;
            StartCoroutine(Press());
        }
    }

    private IEnumerator Press()
    {
        lightDimmer.OnButtonPressed();

        yield return new WaitForSeconds(0.2f); 
        clicked = false;
    }
}