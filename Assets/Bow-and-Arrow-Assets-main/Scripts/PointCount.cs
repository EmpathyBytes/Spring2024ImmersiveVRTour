using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointCount : MonoBehaviour
{
    private int pointCount = 0;
    private int highScore = 0;

    public TextMeshPro scoreText;
    public TextMeshPro highScoreText;

    void Start(){
        UpdateScoreText();
    }
    public void addPointCount(int addi){
        pointCount += addi;
        OlympicsTrophyDisplay.Instance.UpdateArchery(pointCount);
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
        if (scoreText != null)
            scoreText.text = "Current Score: " + pointCount.ToString();
        
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
    }
}
