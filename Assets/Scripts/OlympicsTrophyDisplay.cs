using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OlympicsTrophyDisplay : MonoBehaviour
{
    public static OlympicsTrophyDisplay Instance { get; private set; }

    [SerializeField]
    private GameObject runningTrophy;

    [SerializeField]
    private GameObject archeryTrophy;

    [SerializeField]
    private GameObject dodgeballTrophy;

    [SerializeField]
    private GameObject kayakingTrophy;

    [SerializeField]
    private GameObject explorationTrophy;

    [SerializeField]
    private int requiredObjects;

    [SerializeField]
    private int requiredAtariArcheryScore;

    [SerializeField]
    private int requiredArcheryScore;

    [SerializeField]
    private int requiredDodgeballScore;

    [SerializeField]
    private int requiredKayakingScore;

    private int currentCollected;

    private bool completedAtariArchery;
    private bool completedArchery;

    [SerializeField]
    private ParticleSystem fireworks;

    [SerializeField]
    private Volume volume;

    private bool activated;

    private void Awake() => Instance = this;

    private void TryActivateFireworks()
    {
        if (
            !activated
            && runningTrophy.activeInHierarchy
            && archeryTrophy.activeInHierarchy
            && dodgeballTrophy.activeInHierarchy
            && kayakingTrophy.activeInHierarchy
            && explorationTrophy.activeInHierarchy
        )
        {
            StartCoroutine(Fireworks());
            activated = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Fireworks());
    }

    private IEnumerator Fireworks()
    {
        volume.profile = Instantiate(volume.profile);
        volume.profile.TryGet<ColorAdjustments>(out var adjustments);

        yield return new WaitForSeconds(5.5f);
        yield return StartCoroutine(ChangeLighting(adjustments, 0f, -7f));

        for (int i = 0; i < 3; i++)
        {
            fireworks.Play();
            while (fireworks.isPlaying)
                yield return null;
            yield return new WaitForSeconds(0.3f);
        }

        while (fireworks.isPlaying)
            yield return null;

        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(ChangeLighting(adjustments, -7f, 0f));
    }

    private IEnumerator ChangeLighting(ColorAdjustments adjustments, float start, float end)
    {
        var time = 0f;
        while (time < 5f)
        {
            adjustments.postExposure.value = Mathf.Lerp(start, end, time / 5f);
            time += Time.deltaTime;
            yield return null;
        }
        adjustments.postExposure.value = end;
    }

    public void UpdateCollected()
    {
        currentCollected++;
        if (currentCollected == requiredObjects)
            explorationTrophy.SetActive(true);

        TryActivateFireworks();
    }

    public void UpdateRunning()
    {
        runningTrophy.SetActive(true);
        TryActivateFireworks();
    }

    public void UpdateAtariArchery(int score)
    {
        if (score >= requiredAtariArcheryScore)
            completedAtariArchery = true;
        if (completedArchery && completedAtariArchery)
            archeryTrophy.SetActive(true);

        TryActivateFireworks();
    }

    public void UpdateArchery(int score)
    {
        if (score >= requiredArcheryScore)
            completedArchery = true;
        if (completedArchery && completedAtariArchery)
            archeryTrophy.SetActive(true);

        TryActivateFireworks();
    }

    public void UpdateDodgeball(int score)
    {
        if (score >= requiredDodgeballScore)
            dodgeballTrophy.SetActive(true);

        TryActivateFireworks();
    }

    public void UpdateKayaking(int score)
    {
        if (score >= requiredKayakingScore)
            kayakingTrophy.SetActive(true);

        TryActivateFireworks();
    }
}
