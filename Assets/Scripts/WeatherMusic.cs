using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherMusic : MonoBehaviour
{
    // Smooth changing volumes of every version of the music

    public AudioSource sunny;
    public AudioSource cloudy;
    public AudioSource rainy;

    private float weather = 2f;

    private void Start()
    {
        sunny.Play();
        cloudy.Play();
        rainy.Play();
    }

    private void Update()
    {
        weather = FindAnyObjectByType<WeatherSystem>().currentValue;

        if (weather >= 1f)
        {
            sunny.volume = weather - 1f;
            cloudy.volume = 2f - weather;
            rainy.volume = 0f;
        }
        
        else
        {
            sunny.volume = 0f;
            cloudy.volume = weather;
            rainy.volume = 1f - weather;
        }
    }
}
