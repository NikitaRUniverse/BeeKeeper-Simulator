using Unity.VisualScripting;
using UnityEngine;

public class Bee : MonoBehaviour
{
    public float moveSpeed = 2f;      // Movement speed
    public float noiseScale = 0.1f;   // Scale for the Perlin Noise
    public float attractionStrength = 1f;  // How strongly the bee is attracted to the center
    public float attractionThreshold = 10f; // Distance threshold after which attraction increases

    // Hive
    public Vector3 targetPosition;
    public bool hasTarget = false;
    public Vector3 center = new Vector3(0, 0, 0);

    // Static Y coord
    private float staticY;

    // Noise offsets
    private float noiseOffsetX;
    private float noiseOffsetZ;

    private WeatherSystem weatherSystem;
    private float savedMoveSpeed;
    private float savedNoiseScale;

    public Transform visualModel;
    private Vector3 lastPosition;

    void Start()
    {
        savedMoveSpeed = moveSpeed;
        savedNoiseScale = noiseScale;
        weatherSystem = GameObject.FindGameObjectWithTag("Weather").GetComponent<WeatherSystem>();

        // Initialize noise offsets randomly for each bee so they have independent movement patterns
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetZ = Random.Range(0f, 100f);

        staticY = transform.position.y;

        hasTarget = false;

        lastPosition = transform.position;
    }

    void Update()
    {
        // Move bees to the hive during the rain

        if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Rainy)
        {
            hasTarget = true;
            moveSpeed = savedMoveSpeed * 2f;
            noiseScale = savedNoiseScale * 2f;
        }

        // Increasing the speed of the bees during the sunny weather

        else if (weatherSystem.currentWeather == WeatherSystem.WeatherType.Sunny)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            hasTarget = false;
            moveSpeed = savedMoveSpeed * 2f;
            noiseScale = savedNoiseScale * 2f;
        }

        // Makes the standard speed of the bees during the cloudy weather

        else
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            hasTarget = false;
            moveSpeed = savedMoveSpeed;
            noiseScale = savedNoiseScale;
        }



        // Moving to hive

        if (hasTarget)
        {
            FlyToTarget();
        }
        else
        {
            // Moving using Perlin Noise and distance-based attraction to the center

            Wander();
        }

        RotateBeeVisual();
    }

    public void SetTarget(Vector3 target)
    {
        hasTarget = true;
        targetPosition = new Vector3(target.x, staticY, target.z);
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    private void Wander()
    {
        Vector3 currentPosition = transform.position;

        // Calculate Perlin noise based movement
        float perlinX = Mathf.PerlinNoise(Time.time * noiseScale + noiseOffsetX, 0f) - 0.5f;
        float perlinZ = Mathf.PerlinNoise(0f, Time.time * noiseScale + noiseOffsetZ) - 0.5f;
        Vector3 perlinDirection = new Vector3(perlinX, 0, perlinZ).normalized;

        float distanceFromCenter = Vector3.Distance(currentPosition, center);
        float attractionFactor = Mathf.Clamp01(distanceFromCenter / attractionThreshold);
        Vector3 directionToCenter = (center - currentPosition).normalized;

        // Check if there is any smoke nearby
        SmokeArea nearestSmoke = FindNearestSmoke();
        if (nearestSmoke != null && nearestSmoke.IsBeeInEffect(currentPosition))
        {
            // Move away from the smoke
            Vector3 directionAwayFromSmoke = (currentPosition - nearestSmoke.transform.position).normalized;
            Vector3 movementDirection = directionAwayFromSmoke;
            transform.position += movementDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Normal wandering behavior
            Vector3 movementDirection = Vector3.Lerp(perlinDirection, directionToCenter, attractionFactor * attractionStrength);
            transform.position += movementDirection * moveSpeed * Time.deltaTime;
        }

        transform.position = new Vector3(transform.position.x, staticY, transform.position.z); // Keep Y static
    }

    private SmokeArea FindNearestSmoke()
    {
        SmokeArea[] smokeAreas = FindObjectsOfType<SmokeArea>();
        SmokeArea nearestSmoke = null;
        float shortestDistance = float.MaxValue;

        foreach (SmokeArea smoke in smokeAreas)
        {
            float distance = Vector3.Distance(transform.position, smoke.transform.position);
            if (distance < smoke.effectRadius && distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestSmoke = smoke;
            }
        }

        return nearestSmoke;
    }


    // Move the bee toward the hive
    private void FlyToTarget()
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        transform.position += directionToTarget * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, staticY, transform.position.z);

        // If close to the target, hiding
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }


    // Rotating the bee model to the movement direction
    private void RotateBeeVisual()
    {
        Vector3 movementDirection = transform.position - lastPosition;

        if (movementDirection.magnitude > 0.01f)
        {
            movementDirection.y = 0;
            visualModel.rotation = Quaternion.LookRotation(movementDirection);
        }

        lastPosition = transform.position;
    }
}
