using System.Collections;
using TMPro;
using UnityEngine;

public class AtariRunningScreen : MonoBehaviour
{
    [SerializeField]
    private Transform[] players;

    [SerializeField]
    private Transform endAnchor;

    [SerializeField]
    private GameObject instructions;

    [SerializeField]
    private GameObject countdownCard;

    private float[] velocities;
    private Vector3[] startPositions = new Vector3[3];

    private AtariStation station;

    private void Awake()
    {
        station = GetComponentInParent<AtariStation>();
        for (int i = 0; i < 3; i++)
            startPositions[i] = players[i].position;
    }

    private void Reset()
    {
        for (int i = 0; i < 3; i++)
            players[i].position = startPositions[i];

        instructions.SetActive(true);
        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Be the first to the finish! Press the button to run faster.";
    }

    private void OnEnable()
    {
        Reset();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return new WaitUntil(() => !station.ButtonPressed);
        yield return new WaitUntil(() => station.ButtonPressed);

        instructions.SetActive(false);
        countdownCard.SetActive(true);

        var countdownText = countdownCard.GetComponentInChildren<TextMeshPro>();
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        countdownCard.SetActive(false);

        velocities = new float[3];

        var previouslyPressed = false;
        var won = false;

        const float Fps = 1f / 60f;
        var time = 0f;

        while (!won)
        {
            time += Time.deltaTime;

            while (time >= Fps)
            {
                time -= Fps;

                if (station.ButtonPressed)
                {
                    if (!previouslyPressed)
                        velocities[0] += 1.31f;

                    previouslyPressed = true;
                }
                else
                    previouslyPressed = false;

                var rng = Random.value;
                if (rng < 0.004f)
                    velocities[1] += 8.9f;
                else if (rng < 0.05f)
                    velocities[1] += 1.6f;
                else if (rng < 0.15f)
                    velocities[1] += 0.4f;
                else if (rng < 0.35f)
                    velocities[1] += 0.12f;

                rng = Random.value;
                if (rng < 0.01f)
                    velocities[2] += 5.5f;
                else if (rng < 0.05f)
                    velocities[2] += 1.9f;
                else if (rng < 0.7f)
                    velocities[2] += 0.06f;

                for (int i = 0; i < 3; i++)
                {
                    players[i].transform.position += 0.009f * new Vector3(velocities[i], 0);

                    if (players[i].transform.position.x >= endAnchor.position.x)
                    {
                        instructions.SetActive(true);
                        instructions.GetComponentInChildren<TextMeshPro>().text =
                            i == 0
                                ? "You win! Congratulations! Press the button to exit."
                                : $"Oh no! Player {i + 1} beat you to the finish line! Press the button to exit.";

                        won = true;

                        if (i == 0)
                            OlympicsTrophyDisplay.Instance.UpdateRunning();
                        break;
                    }

                    velocities[i] -= velocities[i] * 0.077f;
                    if (velocities[i] < 1)
                        velocities[i] = 1;
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => station.ButtonPressed);

        station.Reset();
        gameObject.SetActive(false);
    }
}
