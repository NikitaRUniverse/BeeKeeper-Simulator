using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    // Selection of the weather types
    public enum WeatherType { Sunny, Cloudy, Rainy }
    public WeatherType currentWeather;

    // Scale for the Perlin Noise
    public float timeScale = 0.1f;
    private float weatherValue;

    // The scene light and its lights
    public Light sceneLight;
    public Color sunnyLightColor = new Color(1f, 0.95f, 0.8f);
    public Color cloudyLightColor = new Color(0.7f, 0.7f, 0.75f);
    public Color rainyLightColor = new Color(0.5f, 0.5f, 0.55f);

    public float lightTransitionSpeed = 1f;  // Speed of the light color transition

    private Color targetLightColor;

    public ParticleSystem rainEffect;  // Rain particle system

    void Start()
    {
        // Init weather

        switch (currentWeather)
        {
            case WeatherType.Sunny:
                targetLightColor = sunnyLightColor;
                break;
            case WeatherType.Cloudy:
                targetLightColor = cloudyLightColor;
                break;
            case WeatherType.Rainy:
                targetLightColor = rainyLightColor;
                break;
        }

        if (currentWeather != WeatherType.Rainy && rainEffect != null)
        {
            rainEffect.Stop();
        }
    }


    void Update()
    {
        // Generating a Perlin noise value based on time 

        weatherValue = Mathf.PerlinNoise(Time.time * timeScale, 0.0f);

        // Updating the weather based on Perlin noise value

        if (weatherValue < 0.33f)
        {
            SetWeather(WeatherType.Sunny);
        }
        else if (weatherValue < 0.66f)
        {
            SetWeather(WeatherType.Cloudy);
        }
        else
        {
            SetWeather(WeatherType.Rainy);
        }

        if (sceneLight != null)
        {
            sceneLight.color = Color.Lerp(sceneLight.color, targetLightColor, lightTransitionSpeed * Time.deltaTime);
        }
    }

    // Setting the visual effects of the weather

    void SetWeather(WeatherType newWeather)
    {
        if (currentWeather != newWeather)
        {
            currentWeather = newWeather;

            switch (currentWeather)
            {
                case WeatherType.Sunny:
                    Debug.Log("The weather is sunny.");
                    targetLightColor = sunnyLightColor;
                    if (rainEffect != null)
                    {
                        rainEffect.Stop();
                    }
                    break;
                case WeatherType.Cloudy:
                    Debug.Log("The weather is cloudy.");
                    targetLightColor = cloudyLightColor;
                    if (rainEffect != null)
                    {
                        rainEffect.Stop();
                    }
                    break;
                case WeatherType.Rainy:
                    Debug.Log("It's raining.");
                    targetLightColor = rainyLightColor;
                    if (rainEffect != null)
                    {
                        rainEffect.Play();
                    }
                    break;
            }
        }
    }
}