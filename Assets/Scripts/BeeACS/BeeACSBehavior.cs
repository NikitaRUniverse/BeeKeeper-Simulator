using System.Collections;
using UnityEngine;

public class BeeACSBehavior : MonoBehaviour
{
    private enum State {
        SEARCHING,
        RETURNING,
        TURNING
    }


    public Vector2 hivePosition;

    public PheromoneMap pheromoneMap;
    public float maxSpeed;
    public float turnSpeed;
    public GameObject sensorPrefab;

    private State _state;

    private Vector2 _position;
    private Vector2 _velocity;
    private Vector2 _velocityTarget;

    private PheromoneSensor _sensorLeft;
    private PheromoneSensor _sensorForward;
    private PheromoneSensor _sensorRight;
    private Vector2 _pheromoneVelocityTarget;
    private Pheromone[] _pheromonesBuffer;
    
    private PheromoneType _sensedPheromone;
    private PheromoneType _laidPheromone;

    private Vector2 _forward;
    

    void Start()
    {
        Vector2 leftSensorDir = (_forward + (Vector2) transform.up).normalized;
        Vector2 rightSensorDir = (_forward - (Vector2) transform.up).normalized;

        _sensorLeft = Instantiate(sensorPrefab, _position + leftSensorDir, Quaternion.identity, transform).GetComponent<PheromoneSensor>();
        _sensorForward = Instantiate(sensorPrefab, _position + _forward, Quaternion.identity, transform).GetComponent<PheromoneSensor>();
        _sensorRight = Instantiate(sensorPrefab, _position + rightSensorDir, Quaternion.identity, transform).GetComponent<PheromoneSensor>();
        _pheromonesBuffer = new Pheromone[9];

        _sensedPheromone = PheromoneType.TO_FLOWERS;
        _sensedPheromone = PheromoneType.FROM_FLOWERS;

        StartCoroutine(StartSensingPheromones());
    }

    void Init(Vector2 hivePosition)
    {
        this.hivePosition = hivePosition;
    }

    void FixedUpdate()
    {
        switch (_state) {
        case State.SEARCHING:
            TurnToFlowers();
            TurnToPheromones();
            SteerSlightly();
            break;
        case State.RETURNING:
            TurnToPheromones();
            SteerSlightly();
            break;
        case State.TURNING:
            TurnBack();
            break;
        }
        
        Move();
    }

    void Move()
    {
        // Turn towards directionTarget at a fixed rate turnSpeed
        Vector2 turnForceTarget = (_velocityTarget - _velocity) * turnSpeed;
        Vector2 acceleration = Vector2.ClampMagnitude(turnForceTarget, turnSpeed);

        _velocity = Vector2.ClampMagnitude(_velocity + acceleration * Time.fixedDeltaTime, maxSpeed);
        _position += _velocity * Time.fixedDeltaTime;
        
        _forward = _velocity.normalized;

        transform.SetPositionAndRotation(_position, Quaternion.FromToRotation(Vector3.right, _forward));
    }

    IEnumerator StartSensingPheromones()
    {
        while (true) {
            SensePheromones();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void SensePheromones()
    {
        CheckSensor(_sensorLeft);
        CheckSensor(_sensorForward);
        CheckSensor(_sensorRight);
        
        Vector2 leftSensorDir = (_forward + (Vector2) transform.up).normalized;
        Vector2 rightSensorDir = (_forward - (Vector2) transform.up).normalized;

        if (_sensorLeft.value >= _sensorForward.value && _sensorLeft.value >= _sensorRight.value) {
            _pheromoneVelocityTarget = leftSensorDir;
        } else if (_sensorForward.value >= _sensorLeft.value && _sensorForward.value >= _sensorRight.value) {
            _pheromoneVelocityTarget = _forward;
        } else {
            _pheromoneVelocityTarget = rightSensorDir;
        }
    }

    void CheckSensor(PheromoneSensor sensor)
    {
        sensor.value = 0;
        pheromoneMap.CollectAroundCenter(_pheromonesBuffer, sensor.transform.position, _sensedPheromone);

        foreach (Pheromone p in _pheromonesBuffer) {
            float lifetime = Time.time - p.creationTime;
            float evaporation = Mathf.Max(1, lifetime/pheromoneMap.evaporationTime);
            sensor.value += p.value * (1-evaporation);
        }
    }

    void TurnToFlowers()
    {
        if (_state != State.SEARCHING) return;
        // TODO so far need to implement flower sensing
    }

    void TurnToPheromones()
    {
        if (_state == State.TURNING) return;
        _velocityTarget = _pheromoneVelocityTarget;
    }

    void TurnBack()
    {
        StartCoroutine(StartTurningTowards(-_forward));
    }

    IEnumerator StartTurningTowards(Vector2 dir) {
        _state = State.TURNING;
        _velocityTarget = dir.normalized * maxSpeed;

        yield return new WaitForSeconds(0.3f);
        _state = State.SEARCHING;
    }

    void SteerSlightly()
    {
        // Get a random direction slightly different from _velocityTarget
		Vector2 smallestRandomDir = Vector2.zero;
		float change = -1;
		const int iterations = 4;
		for (int i = 0; i < iterations; i++)
		{
			Vector2 randomDir = Random.insideUnitCircle.normalized;
			float dot = Vector2.Dot(_velocityTarget, randomDir);
			if (dot > change)
			{
				change = dot;
				smallestRandomDir = randomDir;
			}
		}

        _velocityTarget = smallestRandomDir;
    }

}