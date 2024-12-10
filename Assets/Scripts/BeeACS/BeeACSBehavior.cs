using System.Collections;
using UnityEngine;

public class BeeACSBehavior : MonoBehaviour
{
    private enum State {
        SEARCHING,
        RETURNING,
        TURNING
    }


    public Vector3 hivePosition;

    public PheromoneMap pheromoneMap;
    public float maxSpeed;
    public float maxAcceleration;
    public float sensorsRange;

    private State _state;

    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _velocityTarget;

    private Transform[] _sensors;
    private float[] _sensorValues;
    private Vector3 _pheromoneTarget;
    private Pheromone[] _pheromonesBuffer;
    
    private PheromoneType _sensedPheromone;
    private PheromoneType _laidPheromone;

    private Vector3 _forward;
    

    void Start()
    {
        _position = transform.position;
        _forward = Vector3.forward;
        _velocity = Vector3.forward * maxSpeed;
        _velocityTarget = _velocity;

        Vector3 leftSensorDir = (_forward + Vector3.left*sensorsRange).normalized;
        Vector3 rightSensorDir = (_forward + Vector3.right*sensorsRange).normalized;

        _sensors = new Transform[3];
        GameObject blank = new GameObject();
        _sensors[0] = Instantiate(blank, _position + leftSensorDir, Quaternion.identity, transform).GetComponent<Transform>();
        _sensors[1] = Instantiate(blank, _position + _forward, Quaternion.identity, transform).GetComponent<Transform>();
        _sensors[2] = Instantiate(blank, _position + rightSensorDir, Quaternion.identity, transform).GetComponent<Transform>();
        _sensorValues = new float[3];
        _pheromonesBuffer = new Pheromone[9];

        _state = State.SEARCHING;
        _sensedPheromone = PheromoneType.TO_FLOWERS;
        _laidPheromone = PheromoneType.FROM_FLOWERS;

        StartCoroutine(StartSensingPheromones());
    }

    void Init(Vector3 hivePosition)
    {
        this.hivePosition = hivePosition;
    }

    void FixedUpdate()
    {
        switch (_state) {
        case State.SEARCHING:
            // TurnToFlowers();
            // TurnToPheromones();
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
        Debug.Log(_velocity.sqrMagnitude + " " + _velocityTarget.sqrMagnitude);
    }

    void Move()
    {
        Vector3 turnAccel = Vector3.ClampMagnitude((_velocityTarget - _velocity) * maxAcceleration, maxAcceleration);

        _velocity = Vector3.ClampMagnitude(_velocity + turnAccel * Time.fixedDeltaTime, maxSpeed);
        _position += _velocity * Time.fixedDeltaTime;
        
        _forward = _velocity.normalized;

        transform.SetPositionAndRotation(_position, Quaternion.FromToRotation(Vector3.forward, _forward));
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
        for (int i = 0; i < _sensors.Length; i++) CheckSensor(i);
        
        Vector3 leftSensorDir = (_forward - transform.right).normalized;
        Vector3 rightSensorDir = (_forward + transform.right).normalized;

        if (_sensorValues[0] >= _sensorValues[1] && _sensorValues[0] >= _sensorValues[2]) {
            _pheromoneTarget = leftSensorDir;
        } else if (_sensorValues[2] >= _sensorValues[0] && _sensorValues[2] >= _sensorValues[1]) {
            _pheromoneTarget = rightSensorDir;
        } else {
            _pheromoneTarget = _forward;
        }
    }

    void CheckSensor(int sensorIdx)
    {
        _sensorValues[sensorIdx] = 0;
        Transform sensor = _sensors[sensorIdx];

        pheromoneMap.CollectAroundCenter(_pheromonesBuffer, sensor.position, _sensedPheromone);

        foreach (Pheromone p in _pheromonesBuffer) {
            if (p == null) break;
            float lifetime = Time.time - p.creationTime;
            float evaporation = Mathf.Max(1, lifetime/pheromoneMap.evaporationTime);
            _sensorValues[sensorIdx] += p.value * (1-evaporation);
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
        _velocityTarget = _pheromoneTarget*maxSpeed;
    }

    void TurnBack()
    {
        StartCoroutine(StartTurningTowards(-_forward));
    }

    IEnumerator StartTurningTowards(Vector3 dir) {
        _state = State.TURNING;
        _velocityTarget = dir.normalized * maxSpeed;

        yield return new WaitForSeconds(0.3f);
        _state = State.SEARCHING;
    }

    void SteerSlightly()
    {
        // Get a random direction slightly different from _velocityTarget
		Vector3 smallestRandomDir = Vector3.zero;
		float change = -1;
		const int iterations = 3;
		for (int i = 0; i < iterations; i++)
		{
			Vector2 randomDir = Random.insideUnitCircle.normalized;
			float dot = Vector3.Dot(_velocityTarget, randomDir);
			if (dot > change)
			{
				change = dot;
				smallestRandomDir = new Vector3(randomDir.x, 0, randomDir.y);
			}
		}

        _velocityTarget = Vector3.ClampMagnitude(_velocityTarget + smallestRandomDir*maxSpeed, maxSpeed);
    }

}