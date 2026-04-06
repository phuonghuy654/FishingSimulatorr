using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] public Light sun;

    [SerializeField] public float cycleDurationInMinutes = 3f;

    [SerializeField] public Gradient ambientColor;

    [SerializeField] public Gradient sunColor;

    [SerializeField] public AnimationCurve sunIntensity;

    [SerializeField] public AnimationCurve ambientIntensity;

    [SerializeField] private float currentTimeOfDay = 0.1f;
    private float cycleDurationInSeconds;

    void Start()
    {
        cycleDurationInSeconds = cycleDurationInMinutes * 60f;
    }

    void Update()
    {
        currentTimeOfDay += Time.deltaTime / cycleDurationInSeconds;
        if (currentTimeOfDay >= 1)
        {
            currentTimeOfDay -= 1;
        }

        sun.transform.rotation = Quaternion.Euler(currentTimeOfDay * 360f, -30f, 0);
        UpdateLighting(currentTimeOfDay);
    }

    void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientSkyColor = ambientColor.Evaluate(timePercent);
        sun.color = sunColor.Evaluate(timePercent);

        sun.intensity = sunIntensity.Evaluate(timePercent);
        RenderSettings.ambientIntensity = ambientIntensity.Evaluate(timePercent);
    }
}