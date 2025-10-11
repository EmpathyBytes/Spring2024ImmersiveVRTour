using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointCount : MonoBehaviour
{
    private int pointCount = 0;

    public TextMeshPro scoreText;

    void Start(){
        UpdateScoreText();
    }
    public void addPointCount(int addi){
        pointCount += addi;
        UpdateScoreText();
    }

    public int getPointCount() {
        return pointCount;
    }

    private void UpdateScoreText() {
        if (scoreText != null)
            scoreText.text = "Score: " + pointCount.ToString();
    }
}
