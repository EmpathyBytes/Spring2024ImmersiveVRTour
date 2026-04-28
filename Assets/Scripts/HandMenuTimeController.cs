using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class HandMenuTimeController : MonoBehaviour
{
    [Header("UI (Hand Menu)")]
    public Slider timeSlider;
    public TMP_Text timeText;

    [Header("Lighting")]
    public Light sun;
    public Material skyboxMaterial;

    [Range(0f, 24f)]
    public float timeOfDay = 12f;

    private float lastTimeOfDay = -1f;

    void OnEnable()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 24f;
            timeSlider.value = timeOfDay;
            timeSlider.onValueChanged.RemoveListener(UpdateTime);
            timeSlider.onValueChanged.AddListener(UpdateTime);
        }

        UpdateTime(timeOfDay);
    }

    void Update()
    {
        if (!Mathf.Approximately(timeOfDay, lastTimeOfDay))
        {
            UpdateTime(timeOfDay);

            if (timeSlider != null && !Mathf.Approximately(timeSlider.value, timeOfDay))
                timeSlider.value = timeOfDay;
        }
    }

    public void UpdateTime(float hour)
    {
        timeOfDay = hour;
        lastTimeOfDay = hour;

        float dayAmount = Mathf.Clamp01(Mathf.Sin((hour - 6f) / 12f * Mathf.PI));

        if (sun != null)
        {
            float sunAngle = (hour / 24f) * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            sun.intensity = Mathf.Lerp(0.02f, 1.2f, dayAmount);
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(0.03f, 1f, dayAmount);
        RenderSettings.reflectionIntensity = Mathf.Lerp(0.1f, 1f, dayAmount);

        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
        {
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(0.08f, 1f, dayAmount));
        }

        if (timeText != null)
        {
            int h = Mathf.FloorToInt(hour);
            int m = Mathf.FloorToInt((hour - h) * 60f);

            string period = h >= 12 ? "PM" : "AM";
            int displayHour = h % 12;
            if (displayHour == 0) displayHour = 12;

            timeText.text = $"{displayHour}:{m:00} {period}";
        }

        DynamicGI.UpdateEnvironment();
    }
}