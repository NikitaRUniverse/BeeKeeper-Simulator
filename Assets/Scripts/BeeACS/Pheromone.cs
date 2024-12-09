using UnityEngine;

public class Pheromone : MonoBehaviour
{
    public float creationTime;
    public float value;
    public float type;
}

public enum PheromoneType {
    SEARCH,
    RETURN,
    DANGER
}
