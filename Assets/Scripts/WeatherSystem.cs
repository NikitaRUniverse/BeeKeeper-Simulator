using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    // Selection of the weather types
    public enum WeatherType { Sunny, Cloudy, Rainy }
    public WeatherType currentWeather;

    // The scene light and its lights
    public Light sceneLight;
    public Color sunnyLightColor = new Color(1f, 0.95f, 0.8f);
    public Color cloudyLightColor = new Color(0.7f, 0.7f, 0.75f);
    public Color rainyLightColor = new Color(0.5f, 0.5f, 0.55f);

    private Color targetLightColor;

    public ParticleSystem rainEffect;  // Rain particle system
    public float minRainEmission = 50f; // Minimum rain intensity
    public float maxRainEmission = 300f; // Maximum rain intensity

    public float currentValue = 1f;
    private float targetValue = 0f;
    public float lerpSpeed = 2f;
    private float changeInterval = 1f;
    private float timer = 0f;

    void Start()
    {
        // Init weather
        targetValue = Random.Range(0, 2f);
    }


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            float bias = Mathf.InverseLerp(0f, 2f, currentValue);
            float random = Random.value;

            if (random < bias)
            {
                targetValue = Random.Range(0f, currentValue);
            }
            else
            {
                targetValue = Random.Range(currentValue, 2f);
            }

            timer = 0f;
            changeInterval = Random.Range(0.5f, 3f);
        }

        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * lerpSpeed);

        // Smoothly interpolate current value
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * lerpSpeed);

        // Determine weather based on currentValue
        if (currentValue < 0.67f)
        {
            currentWeather = WeatherType.Rainy;
            targetLightColor = rainyLightColor;
        }
        else if (currentValue < 1.33f)
        {
            currentWeather = WeatherType.Cloudy;
            targetLightColor = cloudyLightColor;
        }
        else
        {
            currentWeather = WeatherType.Sunny;
            targetLightColor = sunnyLightColor;
        }

        // Adjust scene light color dynamically
        if (sceneLight)
        {
            sceneLight.color = Color.Lerp(sceneLight.color, targetLightColor, Time.deltaTime * lerpSpeed);
        }

        // Adjust rain effect intensity based on currentValue
        if (rainEffect)
        {
            var emission = rainEffect.emission;

            if (currentValue < 0.6f)
            {
                // Map currentValue to the emission range for Rainy weather
                float rainIntensity = Mathf.Lerp(maxRainEmission, minRainEmission, currentValue / 0.67f);
                emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, rainIntensity, Time.deltaTime);
            }
            else
            {
                emission.rateOverTime = 0;
            }
        }
    }
}