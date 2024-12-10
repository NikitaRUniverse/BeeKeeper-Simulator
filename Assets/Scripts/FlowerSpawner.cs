using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    public GameObject flowerPrefab;
    public int numberOfFlowers = 20;  // Number of flowers to generate initially
    public Vector2 spawnAreaSize = new Vector2(50f, 50f); // Size of the spawn area
    public float centralExclusionRadius = 5f;  // Radius of the central area where flowers will not spawn
    public float constantY = 0f;  // Y-coordinate for flowers
    public UIFlowerCounter flowerCounterUI;
    public int newFlowers = 20;  // Number of flowers to generate during the rain

    // Selection of colors for random choosing
    public Color color1 = new Color(1f, 0.95f, 0.8f);
    public Color color2 = new Color(0.7f, 0.7f, 0.75f);
    public Color color3 = new Color(0.5f, 0.5f, 0.55f);
    public Color color4 = new Color(0.7f, 0.3f, 0.3f);
    public Color color5 = new Color(0.2f, 0.6f, 0.4f);

    private int initialFlowers;
    private WeatherSystem weatherSystem;
    private bool spawned;

    void Start()
    {
        initialFlowers = numberOfFlowers;
        GenerateFlowers();
        flowerCounterUI.UpdateFlowerCount(numberOfFlowers, initialFlowers);
        weatherSystem = GameObject.FindGameObjectWithTag("Weather").GetComponent<WeatherSystem>();
    }

    void Update()
    {
        // Spawning new flowers during the rain

        if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Rainy)
        {
            if (!spawned)
            {
                newSpawn();
                spawned = true;
            }
        }

        else
        {
            spawned = false;
        }
    }

    // Generating flowers in the beginning

    private void GenerateFlowers()
    {
        for (int i = 0; i < numberOfFlowers; i++)
        {
            Vector3 spawnPosition = GenerateSpawnPosition(); 
            GameObject newFlower = Instantiate(flowerPrefab, spawnPosition, Quaternion.identity);
            Color randomColor = GetRandomColor();
            Renderer flowerRenderer = newFlower.GetComponent<Renderer>();
            if (flowerRenderer != null)
            {
                flowerRenderer.material.color = randomColor;
            }

            newFlower.GetComponent<Flower>().OnFlowerDestroyed += OnFlowerDestroyed;
        }
    }

    // Randomly choose a color for a flower

    private Color GetRandomColor()
    {
        int randomIndex = Random.Range(0, 5); 
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

    // Selecting the position for generation of the flowers

    private Vector3 GenerateSpawnPosition()
    {
        Vector3 spawnPos;
        do
        {
            // Spawn flowers in the area
            float x = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float z = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            spawnPos = new Vector3(x, constantY, z);

            if (Vector3.Distance(spawnPos, Vector3.zero) < centralExclusionRadius)
                continue;

        } while (Vector3.Distance(spawnPos, Vector3.zero) < centralExclusionRadius); // Prevents from spawning flowers in the center

        return spawnPos;
    }

    // Substracting the flower from the counter

    private void OnFlowerDestroyed()
    {
        numberOfFlowers--;
        flowerCounterUI.UpdateFlowerCount(numberOfFlowers, initialFlowers);
    }

    // Spawning the flowers during the rain

    public void newSpawn()
    {
        for (int i = 0; i < newFlowers; i++)
        {
            if (numberOfFlowers < initialFlowers)
            {
                Vector3 spawnPosition = GenerateSpawnPosition();
                GameObject newFlower = Instantiate(flowerPrefab, spawnPosition, Quaternion.identity);

                Color randomColor = GetRandomColor();
                Renderer flowerRenderer = newFlower.GetComponent<Renderer>();
                if (flowerRenderer != null)
                {
                    flowerRenderer.material.color = randomColor;
                }

                numberOfFlowers++;
                newFlower.GetComponent<Flower>().OnFlowerDestroyed += OnFlowerDestroyed;
            }
        }
        flowerCounterUI.UpdateFlowerCount(numberOfFlowers, initialFlowers);
    }
}
