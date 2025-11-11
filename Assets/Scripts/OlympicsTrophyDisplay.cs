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
    private int requiredArcheryScore;

    private int currentCollected;

    private void Awake() => Instance = this;

    public void UpdateCollected()
    {
        currentCollected++;
        if (currentCollected == requiredObjects)
            explorationTrophy.SetActive(true);
    }

    public void UpdateRunning() => runningTrophy.SetActive(true);

    public void UpdateArchery(int score)
    {
        if (score >= requiredArcheryScore)
            archeryTrophy.SetActive(true);
    }

}
