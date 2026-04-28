using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AtariKayakingScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject instructions;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private Transform[] oars;

    [SerializeField]
    private Transform player;

    private AtariStation station;

    [SerializeField]
    private Transform rock;

    private List<List<Transform>> rocks;

    [SerializeField]
    private BoxCollider2D boatCollider;

    private void Awake() => station = GetComponentInParent<AtariStation>();

    private void Reset()
    {
        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Steer left and right with the joystick, and rhythmically press, hold, and release the button, but don't press it too soon again! Avoid getting pushed down by the rocks and swept to the bottom of the river! How long can you last?";
        instructions.SetActive(true);
        player.transform.localPosition = new();
        rocks = new();
        scoreText.text = "Score: 0";
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
        yield return new WaitUntil(() => !station.ButtonPressed);

        instructions.SetActive(false);

        const float Fps = 1f / 60f;
        var time = 0f;

        var playerCollider = player.GetComponent<PolygonCollider2D>();

        var velocity = Vector3.zero;
        var rowSpeed = 0.97f;
        var blockedTimer = 0f;
        var previouslyPressed = false;
        var lost = false;
        var rockTimer = 0;
        var rockHitVelocity = 0f;

        var score = 0;
        while (!lost)
        {
            time += Time.deltaTime;

            while (time >= Fps && !lost)
            {
                time -= Fps;

                var inputDir = new Vector3(station.CurrentDirection.x, station.CurrentDirection.z);
                velocity.x += inputDir.x * 0.001f;
                velocity.x *= 0.983f;

                HandleButton(ref previouslyPressed, ref blockedTimer, ref velocity, ref rowSpeed);

                velocity.y -= 0.0017f;
                velocity.y *= 0.983f;

                if (velocity.y < -0.02f)
                    velocity.y = -0.02f;
                velocity.y = Mathf.Clamp(velocity.y, -.017f, .05f);
                velocity.x = Mathf.Clamp(velocity.x, -0.025f, 0.025f);

                if (rockHitVelocity < 0)
                    rockHitVelocity *= 0.91f;

                velocity.y += Mathf.Clamp(rockHitVelocity, -1000, 0);

                player.position += new Vector3(velocity.x, Mathf.Clamp(velocity.y, -10000, .0067f));
                player.localPosition = new(
                    Mathf.Clamp(player.localPosition.x, -2.12f, 2.12f),
                    Mathf.Clamp(player.localPosition.y, -10000, 4.61f),
                    0
                );

                if (player.localPosition.y < -5.5f)
                {
                    lost = true;
                    break;
                }

                rockTimer++;

                if (rockTimer == 427 / 4)
                {
                    var gaps = score > 13 ? 4 : 5;
                    var gapStart = Random.Range(1, 19 - gaps - 1);

                    var row = new List<Transform>();
                    for (var i = 0; i < 19; i++)
                    {
                        if (i >= gapStart && i < gapStart + gaps)
                            continue;

                        var rock = Instantiate(this.rock, transform);
                        rock.transform.localPosition = new(-2.72f, 4.8f, 0);
                        rock.gameObject.SetActive(true);
                        rock.position += new Vector3(0.3f * i, 0);
                        row.Add(rock);
                    }
                    rocks.Add(row);
                    rockTimer = 0;
                    score++;
                    scoreText.text = $"Score: {score}";
                }

                for (var i = rocks.Count - 1; i >= 0; i--)
                {
                    var row = rocks[i];
                    var delete = false;

                    foreach (var rock in row)
                    {
                        if (boatCollider.IsTouching(rock.GetComponent<BoxCollider2D>()))
                            rockHitVelocity = -0.1f;

                        rock.transform.position += Vector3.down * 0.0053f * 4;
                        if (rock.transform.position.y < -7f)
                            delete = true;
                    }

                    if (delete)
                    {
                        foreach (var rock in row)
                            Destroy(rock.gameObject);

                        rocks.RemoveAt(i);
                    }
                }
            }

            yield return null;
        }

        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Game over! Try for a higher score next time!";

        instructions.SetActive(true);

        OlympicsTrophyDisplay.Instance.UpdateDodgeball(score);

        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => station.ButtonPressed);

        foreach (var row in rocks)
        foreach (var rock in row)
            Destroy(rock.gameObject);

        station.Reset();
        gameObject.SetActive(false);
    }

    private void HandleButton(
        ref bool previouslyPressed,
        ref float recoveryTimer,
        ref Vector3 velocity,
        ref float power
    )
    {
        if (station.ButtonPressed)
        {
            if (recoveryTimer < 14)
            {
                if (!previouslyPressed)
                    velocity.y = 0.09f;
                else
                    velocity.y = -0.01f;

                oars[0].localEulerAngles = new(0, 0, 0);
                oars[1].localEulerAngles = new(0, 0, 0);
            }
            else
            {
                if (!previouslyPressed)
                    power = 0.97f;

                velocity.y += 0.012f * power;
                power *= 0.928f;

                if (power < 0.15f)
                {
                    velocity.y -= 0.001f;

                    oars[0].localEulerAngles = new(0, 0, 0);
                    oars[1].localEulerAngles = new(0, 0, 0);
                }
                else
                {
                    oars[0].localEulerAngles = new(0, 0, -25);
                    oars[1].localEulerAngles = new(0, 0, 25);
                }
            }

            previouslyPressed = true;
        }
        else
        {
            if (previouslyPressed)
            {
                var frames = Mathf.Lerp(0, 15, Mathf.InverseLerp(0.17f, 0.67f, power));
                recoveryTimer = -frames;
            }

            recoveryTimer++;
            previouslyPressed = false;
            power = 0f;

            oars[0].localEulerAngles = new(0, 0, 25);
            oars[1].localEulerAngles = new(0, 0, -25);
        }
    }
}
