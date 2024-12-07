using UnityEngine;
using System;

public class Flower : MonoBehaviour
{
    public float initialLifetime = 10f; // The initial health of the flower
    public float lifetime; // The current health of the flower
    public float timerDecreaseSpeed = 1f; // Speed at which the health decreases
    public float reverseTimerSpeed = 0.5f; // Speed at which the health restores
    public float speedIncreaseRate = 0.05f; // Rate at which the health descreasing increases over time
    private float saveTimerDecreaseSpeed;

    public event Action OnFlowerDestroyed;

    private bool isBeeNearby = false;  // Tracks if a bee is near the flower
    private WeatherSystem weatherSystem;

    void Start()
    {
        lifetime = initialLifetime;
        weatherSystem = GameObject.FindGameObjectWithTag("Weather").GetComponent<WeatherSystem>();
        saveTimerDecreaseSpeed = timerDecreaseSpeed;
    }

    void Update()
    {
        UpdateLifetime();
    }

    // Updating health

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
