using System.Collections.Generic;
using UnityEngine;

// not moving this to namespace L3 since its used by L3, L4 & L5
public class GameManager : MonoBehaviour
{
    public List<GameObject> obstacles;
    public List<GameObject> waypoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var obstacle in obstacles)
        {
            var minPosition = new Vector3(-49.5f, 0.5f, -49.5f);
            var maxPosition = new Vector3(49.5f, 0.5f, 49.5f);

            var randomPosition = new Vector3(
                Random.Range(minPosition.x, maxPosition.x),
                0.5f,
                Random.Range(minPosition.z, maxPosition.z)
            );

            obstacle.transform.position = randomPosition;
        }

        foreach (var waypoint in waypoints)
        {
            var minPosition = new Vector3(-49.5f, 0.5f, -49.5f);
            var maxPosition = new Vector3(49.5f, 0.5f, 49.5f);

            var randomPosition = new Vector3(
                Random.Range(minPosition.x, maxPosition.x), 0.5f, Random.Range(minPosition.z, maxPosition.z));
            waypoint.transform.position = randomPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
