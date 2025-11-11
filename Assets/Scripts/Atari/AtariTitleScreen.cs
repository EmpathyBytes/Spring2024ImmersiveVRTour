using System.Collections;
using UnityEngine;

public class AtariTitleScreen : MonoBehaviour
{
    [SerializeField]
    private Transform runningTitle;

    [SerializeField]
    private Transform archeryTitle;

    [SerializeField]
    private Transform selectionIcon;
    private bool running = true;

    private AtariStation station;

    private void Awake() => station = GetComponentInParent<AtariStation>();

    private void OnEnable() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !station.ButtonPressed);

        while (true)
        {
            if (Vector3.Dot(station.CurrentDirection, Vector3.forward) > 0.7f)
            {
                running = true;
                selectionIcon.position = runningTitle.position + new Vector3(2.5f, 0, 0);
            }
            else if (Vector3.Dot(station.CurrentDirection, Vector3.back) > 0.7f)
            {
                running = false;
                selectionIcon.position = archeryTitle.position + new Vector3(2.5f, 0, 0);
            }

            if (station.ButtonPressed)
            {
                if (running)
                    station.LoadRunning();
                else
                    station.LoadArchery();
            }

            yield return null;
        }
    }
}
