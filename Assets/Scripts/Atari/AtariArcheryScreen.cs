using System.Collections;
using TMPro;
using UnityEngine;

public class AtariArcheryScreen : MonoBehaviour
{
    [SerializeField]
    private Transform bow;

    [SerializeField]
    private LineRenderer bowLine;

    [SerializeField]
    private GameObject instructions;

    [SerializeField]
    private float maxPullDistance;

    [SerializeField]
    private float arrowSpeed;

    [SerializeField]
    private float gravity;

    [SerializeField]
    private Transform arrow;

    [SerializeField]
    private Vector2 bounds;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI missesText;

    [SerializeField]
    private BoxCollider2D[] targetBounds;

    private int score;
    private int misses;

    private float[] targetBoundsAreas;
    private float targetBoundsArea;

    private Transform currentArrow;
    private AtariStation station;

    private Vector2 ClampedDir =>
        new(
            station.CurrentDirection.x > 0 ? 0 : station.CurrentDirection.x,
            station.CurrentDirection.z > 0 ? 0 : station.CurrentDirection.z
        );

    private void Awake()
    {
        station = GetComponentInParent<AtariStation>();
        instructions.SetActive(true);

        targetBoundsAreas = new float[targetBounds.Length];
        for (int i = 0; i < targetBounds.Length; i++)
        {
            targetBoundsAreas[i] = targetBounds[i].bounds.size.x * targetBounds[i].bounds.size.y;
            targetBoundsArea += targetBoundsAreas[i];
        }
    }

    private void Reset()
    {
        target.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        missesText.gameObject.SetActive(false);
        instructions.SetActive(true);

        instructions.GetComponentInChildren<TextMeshPro>().text =
            "Hit the targets! Use the joystick to aim and press the button to shoot. If you miss 3 times, it's game over!";

        score = 0;
        misses = 0;
    }

    private void OnEnable()
    {
        Reset();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        currentArrow = Instantiate(arrow, bow);
        currentArrow.gameObject.SetActive(true);

        yield return new WaitUntil(() => !station.ButtonPressed);
        yield return new WaitUntil(() => station.ButtonPressed);
        yield return new WaitUntil(() => !station.ButtonPressed);

        instructions.SetActive(false);

        scoreText.text = $"SCORE: 0";
        scoreText.gameObject.SetActive(true);
        missesText.text = "";
        missesText.gameObject.SetActive(true);

        bowLine.positionCount = 3;
        bowLine.SetPositions(new Vector3[] { new(0.2f, 0.79f), new(0.2f, 0), new(0.2f, -0.79f) });

        target.gameObject.SetActive(true);
        target.position = GetRandomTargetPos();

        while (true)
        {
            while (!station.ButtonPressed || ClampedDir.magnitude == 0)
            {
                Aim();
                yield return null;
            }

            bow.localEulerAngles = new(0, 0, 0);
            bowLine.SetPosition(1, new(0.2f, 0));

            yield return StartCoroutine(ShootArrow());

            if (misses == 3)
                break;

            currentArrow = Instantiate(arrow, bow);
            currentArrow.gameObject.SetActive(true);
        }

        scoreText.gameObject.SetActive(false);
        missesText.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
        instructions.SetActive(true);
        instructions.GetComponentInChildren<TextMeshPro>().text =
            $"Game over! Your score is {score}. Press the button to exit.";

        OlympicsTrophyDisplay.Instance.UpdateAtariArchery(score);

        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => station.ButtonPressed);

        Destroy(currentArrow.gameObject);

        station.Reset();
        gameObject.SetActive(false);
    }

    private void Aim()
    {
        if (ClampedDir == Vector2.zero)
            bow.localEulerAngles = new(0, 0, 0);
        else
        {
            var angle = Mathf.Atan2(ClampedDir.x, -ClampedDir.y) * Mathf.Rad2Deg;
            bow.localEulerAngles = new(0, 0, angle + 90);
        }

        bowLine.SetPosition(1, new(0.2f - maxPullDistance * ClampedDir.magnitude, 0));
        currentArrow.localPosition = new(0.35f - maxPullDistance * ClampedDir.magnitude, 0);
    }

    private Vector3 GetRandomTargetPos()
    {
        var r = Random.Range(0f, targetBoundsArea);
        var boundsIndex = 0;
        var total = 0f;
        for (int i = 0; i < targetBoundsAreas.Length; i++)
        {
            total += targetBoundsAreas[i];
            if (r <= total)
            {
                boundsIndex = i;
                break;
            }
        }

        var min = targetBounds[boundsIndex].bounds.min;
        var max = targetBounds[boundsIndex].bounds.max;

        return new(Random.Range(min.x, max.x), Random.Range(min.y, max.y), target.position.z);
    }

    private IEnumerator ShootArrow()
    {
        currentArrow.parent = transform;

        var velocity = -ClampedDir * arrowSpeed;
        var arrowCollider = currentArrow.GetComponent<Collider2D>();
        var targetCollider = target.GetComponent<Collider2D>();
        while (
            currentArrow.localPosition.x < bounds.x
            && currentArrow.localPosition.x > -bounds.x
            && currentArrow.localPosition.y < bounds.y
            && currentArrow.localPosition.y > -bounds.y
        )
        {
            velocity += gravity * Time.deltaTime * Vector2.up;
            currentArrow.localPosition += (Vector3)velocity * Time.deltaTime;

            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            currentArrow.localEulerAngles = new(0, 0, angle);

            if (arrowCollider.IsTouching(targetCollider))
            {
                score++;
                scoreText.text = $"SCORE: {score}";

                yield return new WaitForSeconds(0.25f);

                var targetPos = transform.InverseTransformPoint(GetRandomTargetPos());
                StartCoroutine(MoveItem(target, targetPos));

                currentArrow.parent = bow;
                yield return StartCoroutine(MoveItem(currentArrow, new(0.35f, 0)));

                Destroy(currentArrow.gameObject);
                yield break;
            }

            yield return null;
        }

        misses++;
        missesText.text = new('X', misses);

        if (misses == 3)
            yield break;

        yield return new WaitForSeconds(0.25f);

        currentArrow.parent = bow;
        yield return StartCoroutine(MoveItem(currentArrow, new(0.35f, 0)));
        Destroy(currentArrow.gameObject);
    }

    private IEnumerator MoveItem(Transform item, Vector3 pos)
    {
        var startPos = item.localPosition;
        var startAngle = item.localEulerAngles;

        var time = 0f;
        while (time < 0.6f)
        {
            time += Time.deltaTime;
            var t = Mathf.Clamp01(time / 0.6f);

            t = t * t * (3f - 2f * t);

            item.localPosition = Vector3.Lerp(startPos, pos, t);
            item.localEulerAngles = new(
                Mathf.LerpAngle(startAngle.x, 0, t),
                Mathf.LerpAngle(startAngle.y, 0, t),
                Mathf.LerpAngle(startAngle.z, 0, t)
            );

            yield return null;
        }

        item.localPosition = pos;
        item.localEulerAngles = Vector3.zero;
    }
}
