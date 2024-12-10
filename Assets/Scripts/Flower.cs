using UnityEngine;
using System;

public class Flower : MonoBehaviour
{
    public float initialLifetime = 10f; // The initial health of the flower
    public float lifetime; // The current health of the flower
    public float timerDecreaseSpeed = 1f; // Speed at which the health decreases
    public float reverseTimerSpeed = 0.5f; // Speed at which the health restores
    public float speedIncreaseRate = 0.05f; // Rate at which the health decreasing increases over time
    private float saveTimerDecreaseSpeed;

    public event Action OnFlowerDestroyed;

    public Color color1 = new Color(1f, 0.95f, 0.8f);
    public Color color2 = new Color(0.7f, 0.7f, 0.75f);
    public Color color3 = new Color(0.5f, 0.5f, 0.55f);
    public Color color4 = new Color(0.7f, 0.3f, 0.3f);
    public Color color5 = new Color(0.2f, 0.6f, 0.4f);

    private bool isBeeNearby = false;  // Tracks if a bee is near the flower
    private WeatherSystem weatherSystem;

    private Renderer flowerRenderer; // Renderer for color updates
    public Color initialColor = Color.white; // Starting color
    private Color greyColor = Color.grey; // Target color

    void Start()
    {
        initialColor = GetRandomColor();

        lifetime = initialLifetime;
        weatherSystem = GameObject.FindGameObjectWithTag("Weather").GetComponent<WeatherSystem>();
        saveTimerDecreaseSpeed = timerDecreaseSpeed;

        flowerRenderer = GetComponent<Renderer>();
        if (flowerRenderer != null)
        {
            flowerRenderer.material.color = initialColor;
        }
    }

    void Update()
    {
        UpdateLifetime();
        UpdateColor();
    }

    private Color GetRandomColor()
    {
        int randomIndex = UnityEngine.Random.Range(0, 5);
        switch (randomIndex)
        {
            case 0:
                return color1;
            case 1:
                return color2;
            case 2:
                return color3;
            case 3:
                return color4;
            case 4:
                return color5;
            default:
                return color1;
        }
    }

    // Update flower lifetime
    private void UpdateLifetime()
    {
        if (isBeeNearby)
        {
            // If a bee is above the flower, health will be restored
            lifetime += reverseTimerSpeed * Time.deltaTime;
        }
        else
        {
            // If it's sunny, the health decreases faster
            if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Sunny)
            {
                lifetime -= timerDecreaseSpeed * Time.deltaTime;
                timerDecreaseSpeed += speedIncreaseRate * 1.5f * Time.deltaTime;
            }

            // If it's cloudy, the health decreases normally
            else if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Cloudy)
            {
                lifetime -= timerDecreaseSpeed * Time.deltaTime;
                timerDecreaseSpeed += speedIncreaseRate * Time.deltaTime;
            }
        }

        lifetime = Mathf.Clamp(lifetime, 0f, initialLifetime);

        // Flower dies if the health reaches zero
        if (lifetime <= 0f)
        {
            OnFlowerDestroyed?.Invoke();
            Destroy(gameObject);
        }

        // Health is full during the rain
        if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Rainy)
        {
            lifetime = initialLifetime;
            timerDecreaseSpeed = saveTimerDecreaseSpeed;
        }
    }

    // Update flower color based on lifetime
    private void UpdateColor()
    {
        if (flowerRenderer != null)
        {
            float t = 1 - (lifetime / initialLifetime); // Calculate interpolation factor (0 to 1)
            flowerRenderer.material.color = Color.Lerp(initialColor, greyColor, t); // Interpolate color
        }
    }

    // Check for bees entering the flower's area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bee"))
        {
            isBeeNearby = true;
        }
    }

    // Check for bees exiting the flower's area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bee"))
        {
            isBeeNearby = false;
        }
    }
}