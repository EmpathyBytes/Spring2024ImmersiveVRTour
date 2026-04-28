using System.Collections;
using UnityEngine;

public class AtariTitleScreen : MonoBehaviour
{
    [SerializeField]
    private Transform[] titleTransforms;

    [SerializeField]
    private Transform selectionIcon;

    private AtariStation station;

    private void Awake() => station = GetComponentInParent<AtariStation>();

    private void OnEnable() => StartCoroutine(Run());

    private int selectedGame;

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !station.ButtonPressed);

        while (true)
        {
            if (Vector3.Dot(station.CurrentDirection, Vector3.forward) > 0.7f)
            {
                selectedGame = (selectedGame - 1 + titleTransforms.Length) % titleTransforms.Length;
                selectionIcon.position =
                    titleTransforms[selectedGame].position + new Vector3(2.5f, 0, 0);
                yield return new WaitUntil(() =>
                    Vector3.Dot(station.CurrentDirection, Vector3.forward) <= 0.7f
                );
            }
            else if (Vector3.Dot(station.CurrentDirection, Vector3.back) > 0.7f)
            {
                selectedGame = (selectedGame + 1 + titleTransforms.Length) % titleTransforms.Length;
                selectionIcon.position =
                    titleTransforms[selectedGame].position + new Vector3(2.5f, 0, 0);

                yield return new WaitUntil(() =>
                    Vector3.Dot(station.CurrentDirection, Vector3.back) <= 0.7f
                );
            }

            if (station.ButtonPressed)
            {
                switch (selectedGame)
                {
                    case 0:
                        station.LoadRunning();
                        break;
                    case 1:
                        station.LoadArchery();
                        break;
                    case 2:
                        station.LoadDodgeball();
                        break;
                    case 3:
                        station.LoadKayaking();
                        break;
                }
            }

            yield return null;
        }
    }
}
