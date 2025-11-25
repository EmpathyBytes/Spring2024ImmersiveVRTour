using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointCount : MonoBehaviour
{
    private int pointCount = 0;
    private int highScore = 0;

    public ArrowSpawner arrowSpawner;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI arrowsLeftText;

    private int arrowsLeft;

    void Start(){
        UpdateScoreText();
        arrowsLeft = arrowSpawner.getArrowsLeft();
        
    }
    public void addPointCount(int addi){
        pointCount += addi;
        UpdateScoreText();
    }

    public void resetPoints(){
        if (pointCount > highScore){
            highScore = pointCount;
        }
        pointCount = 0;
    }

    public int getPointCount() {
        return pointCount;
    }

    private void UpdateScoreText() {
        arrowsLeft = arrowSpawner.getArrowsLeft();
        if (scoreText != null)
            scoreText.text = "Current Score: " + pointCount.ToString();
        
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
        if (arrowsLeftText != null)
            arrowsLeftText.text = "Arrows Left : " + arrowsLeft.ToString();
    }

    public void UpdateScoreTextExternal(){
        UpdateScoreText();
    }
}
