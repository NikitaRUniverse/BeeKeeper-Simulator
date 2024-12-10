using UnityEngine;

public class SmokeSpawner : MonoBehaviour
{
    public GameObject smokePrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnSmokeAtMousePosition();
        }
    }

    // Spawns the smoke at the mouse point
    void SpawnSmokeAtMousePosition()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Instantiate(smokePrefab, hit.point, Quaternion.identity);
        }
    }
}
