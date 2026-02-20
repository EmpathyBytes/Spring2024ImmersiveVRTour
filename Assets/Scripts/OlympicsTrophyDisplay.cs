using UnityEngine;

public class OlympicsTrophyDisplay : MonoBehaviour
{
public static OlympicsTrophyDisplay Instance { get; private set; }

    [SerializeField]
    private GameObject runningTrophy;

    [SerializeField]
    private GameObject archeryTrophy;

    [SerializeField]
    private GameObject explorationTrophy;

    [SerializeField]
    private int requiredObjects;

    [SerializeField]
    private int requiredAtariArcheryScore;

    [SerializeField]
    private int requiredArcheryScore;

    private int currentCollected;
    
    private bool completedAtariArchery;
    private bool completedArchery;

    private void Awake() => Instance = this;

    public void UpdateCollected()
    {
        currentCollected++;
        if (currentCollected == requiredObjects)
            explorationTrophy.SetActive(true);
    }

    public void UpdateRunning() => runningTrophy.SetActive(true);

    public void UpdateAtariArchery(int score)
    {
        if (score >= requiredAtariArcheryScore)
            completedAtariArchery = true;
        if (completedArchery && completedAtariArchery)
            archeryTrophy.SetActive(true);
    }

    public void UpdateArchery(int score)
    {
        if (score >= requiredArcheryScore)
            completedArchery = true;
        if (completedArchery && completedAtariArchery)
            archeryTrophy.SetActive(true);
    }

}
