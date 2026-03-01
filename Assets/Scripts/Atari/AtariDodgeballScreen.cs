using System.Collections;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using TMPro;
using UnityEngine;

public class AtariDodgeballScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject instructions;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private Vector2 bounds;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Transform[] spawnPositions;

    [SerializeField]
    private GameObject ballTemplate;

    private AtariStation station;

    [SerializeField]
    private float minBallSpeed;

    [SerializeField]
    private float maxBallSpeed;

    private List<Ball> balls;

    private void Awake()
    {
        station = GetComponentInParent<AtariStation>();
        instructions.SetActive(true);
    }

    private void Reset()
    {
        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Avoid the balls as long as you can. If you get hit, it's over!";

        instructions.SetActive(true);
        balls = new();

        player.transform.localPosition = new();

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

        bool lost = false;
        int ticks = 0;
        while (!lost)
        {
            time += Time.deltaTime;

            while (time >= Fps && !lost)
            {
                ticks++;
                scoreText.text = $"Score: {ticks / 60}";

                time -= Fps;

                var dir = new Vector3(station.CurrentDirection.x, station.CurrentDirection.z);
                player.position += moveSpeed * dir;

                if (
                    System.Math.Abs(player.localPosition.x) > 8
                    || System.Math.Abs(player.localPosition.y) > 8
                )
                {
                    lost = true;
                    break;
                }

                var rate = Mathf.Lerp(0.033f, 0.050f, ticks / 60f / 67f);
                if (Random.Range(0f, 1f) < rate)
                {
                    var ball = Instantiate(ballTemplate, transform).transform;
                    ball.position = spawnPositions[Random.Range(0, spawnPositions.Length)].position;

                    float hue;
                    do hue = Random.value;
                    while (hue > 0.15f && hue < 0.45f);

                    ball.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(hue, 1, 1);

                    balls.Add(
                        new()
                        {
                            Transform = ball,
                            Speed = Random.Range(minBallSpeed, maxBallSpeed),
                            Direction = (player.position - ball.position).normalized,
                            Collider = ball.GetComponent<CircleCollider2D>(),
                        }
                    );
                }

                foreach (var ball in balls)
                {
                    ball.Transform.position += ball.Speed * ball.Direction;
                    if (playerCollider.IsTouching(ball.Collider))
                    {
                        lost = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Game over! Try for a higher score next time!";

        instructions.SetActive(true);

        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => station.ButtonPressed);

        foreach (var ball in balls)
            Destroy(ball.Transform.gameObject);

        OlympicsTrophyDisplay.Instance.UpdateDodgeball(ticks / 60);

        station.Reset();
        gameObject.SetActive(false);
    }

    private class Ball
    {
        public Transform Transform;
        public CircleCollider2D Collider;
        public Vector3 Direction;
        public float Speed;
    }
}
