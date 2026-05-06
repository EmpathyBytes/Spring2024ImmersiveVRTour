using System.Collections.Generic;
using UnityEngine;

public class LightDimmer : MonoBehaviour
{
    public List<LightControl> lights = new List<LightControl>();

    private int currentLevel = 2;
    private int totalLevels = 5;

    public void OnButtonPressed()
    {
        currentLevel = (currentLevel + 1) % totalLevels;

        foreach (LightControl light in lights)
        {
            light.SetLevel(currentLevel);
        }
    }
}