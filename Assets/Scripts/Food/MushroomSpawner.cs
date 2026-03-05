
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; //To be able to use lists

public class MushroomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mushroomPrefab;
    [SerializeField] private Transform Mushrooms;

    [SerializeField] private int mushroomMaxInitialization = 30;

    [SerializeField] private int mushroomMapLimit = 500;


    [SerializeField] private float SpawnRadius = 100f;
    [SerializeField] private float reproduceRadius = 10f;

    [SerializeField] private float timeInterval = 8760f; // 1 second = 1 hour

    private float timer;

    private List<GameObject> mushrooms = new List<GameObject>();


    void Start()
    {
        // initial mushrooms spawn
        for (int i = 0; i < mushroomMaxInitialization; i++)
        {
            SpawnMushrooms();
        }
    }


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeInterval)
        {
            timer = 0f;
            ReproduceMushrooms();
        }
    }


    void ReproduceMushrooms()
    {
        List<GameObject> current = new List<GameObject>(mushrooms);

        foreach (GameObject mushroom in current)
        {
            if (mushrooms.Count >= mushroomMapLimit)
                return;

            // 50% chance to reproduce for each mushroom
            if (Random.value > 0.5f)
                continue;

            Vector3 spawnPos;

            // 90% spawn near parent, other just randomly
            if (Random.value < 0.9f)
            {
                spawnPos = GetNavMeshPositionNearOtherMushrooms(mushroom.transform.position, reproduceRadius);
            }
            else
            {
                spawnPos = GetRandomNavMeshPoint();
            }

            if (spawnPos != Vector3.zero)
            {
                GameObject newMushroom = Instantiate(mushroomPrefab, spawnPos, Quaternion.identity, Mushrooms);
                mushrooms.Add(newMushroom);
            }
        }
    }


    void SpawnMushrooms()
    {
        Vector3 pos = GetRandomNavMeshPoint();

        if (pos != Vector3.zero)
        {
            GameObject newMushroom = Instantiate(mushroomPrefab, pos, Quaternion.identity, Mushrooms);
            mushrooms.Add(newMushroom);
        }
    }


    Vector3 GetRandomNavMeshPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitSphere * SpawnRadius;
            random.y = 0;

            Vector3 point = transform.position + random;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, 10f, NavMesh.AllAreas))
                return hit.position;
        }

        return Vector3.zero;
    }


    Vector3 GetNavMeshPositionNearOtherMushrooms(Vector3 origin, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitSphere * radius;
            random.y = 0;

            Vector3 point = origin + random;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }

        return Vector3.zero;
    }


    public void RemoveMushroom(GameObject mushroom)
    {
        mushrooms.Remove(mushroom);
    }
}