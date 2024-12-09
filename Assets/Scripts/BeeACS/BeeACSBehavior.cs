using System;
using UnityEngine;

public class BeeACSBehavior : MonoBehaviour
{
    public float moveSpeedMax;
    public float turnSpeed;

    private Vector2 _position;
    private Vector2 _velocity;
    private Vector2 _directionTarget;

    public GameObject sensorPrefab;
    private PheromoneSensor _sensorLeft;
    private PheromoneSensor _sensorForward;
    private PheromoneSensor _sensorRight;

    private Vector2 _left;
    private Vector2 _forward;
    private Vector2 _right;

    public PheromoneMap pheromoneMap;
    private PheromoneType _pheromoneTargetType;
    

    void Start()
    {
        _sensorLeft = Instantiate(sensorPrefab, , Quaternion.identity, transform);

        _pheromoneTargetType = PheromoneType.SEARCH;
    }

    void FixedUpdate()
    {
        // Turn towards directionTarget at a fixed rate turnSpeed
        Vector2 velocityTarget = _directionTarget * moveSpeedMax;
        Vector2 turnForceTarget = (velocityTarget - _velocity) * turnSpeed;
        Vector2 acceleration = Vector2.ClampMagnitude(turnForceTarget, turnSpeed);

        _velocity = Vector2.ClampMagnitude(_velocity + acceleration * Time.fixedDeltaTime, moveSpeedMax);
        _position += _velocity * Time.fixedDeltaTime;

        float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(_position, Quaternion.Euler(0, 0, angle));
    }

    void TurnToPheromones() {
        CheckPheromones(_sensorLeft);
        CheckPheromones(_sensorForward);
        CheckPheromones(_sensorRight);

        if (_sensorLeft.value >= _sensorForward.value && _sensorLeft.value >= _sensorRight.value) {
            _directionTarget = _left;
        } else if (_sensorForward.value >= _sensorLeft.value && _sensorForward.value >= _sensorRight.value) {
            _directionTarget = _forward;
        } else {
            _directionTarget = _right;
        }
    }

    void CheckPheromones(PheromoneSensor sensor) {
        sensor.value = 0;

        Pheromone[] pheromones = pheromoneMap.GetAllInCircle(sensor.transform.position, sensor.radius);

        foreach (Pheromone p in pheromones) {
            float lifetime = Time.time - p.creationTime;
            float evaporation = Mathf.Max(1, lifetime/pheromoneMap.evaporationTime);
            sensor.value += p.value * (1-evaporation);
        }
    }
}
