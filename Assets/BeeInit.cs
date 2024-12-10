using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeInit : MonoBehaviour
{
    public int bees;
    public GameObject beePrefab;

    void Start()
    {
        for (int i = 0; i < bees; i++)
        {
            GameObject bee = Instantiate(beePrefab, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity, transform.parent);
            bee.GetComponent<Bee>().targetPosition = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }
}
