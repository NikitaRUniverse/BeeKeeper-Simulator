using UnityEngine;

public class SmokeArea : MonoBehaviour
{
    public float lifetime = 5f;    // Duration of the smoke
    public float effectRadius = 5f; // Radius in which bees will be affected

    void Start()
    {
        // Destroy the smoke after its lifetime ends
        Destroy(gameObject, lifetime);
    }

    // Debugging stuff

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(transform.position, effectRadius);
    }

    // Checking if a bee is within the smoke radius

    public bool IsBeeInEffect(Vector3 beePosition)
    {
        return Vector3.Distance(transform.position, beePosition) < effectRadius;
    }
}
