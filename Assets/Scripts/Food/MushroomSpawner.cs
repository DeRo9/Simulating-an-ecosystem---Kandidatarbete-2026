
using UnityEngine;
using UnityEngine.AI;

public class MushroomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject Mushroom;
    [SerializeField] private Transform Mushrooms; 
    [SerializeField] private int maxMushrooms = 30;
    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private float spawnInterval = 5f;

    private int currentMushrooms = 0;
    private float timer;

    void Start()
    {
        for (int i = 0; i < maxMushrooms / 2; i++)
        {
            SpawnMushroom();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            if (currentMushrooms < maxMushrooms)
            {
                SpawnMushroom();
            }
        }
    }
 void SpawnMushroom()
{
    Vector3 pos = GetRandomNavMeshPosition();
    if (pos != Vector3.zero)
    {
        Instantiate(Mushroom, pos, Quaternion.identity, Mushrooms); //Instantiate(Mushroom, pos, Quaternion.identity);
        currentMushrooms++;
    }
}

Vector3 GetRandomNavMeshPosition()
{
    for (int i = 0; i < 10; i++) // try 10 times
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection.y = 0f;
        Vector3 randomPoint = transform.position + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }
    }
    return Vector3.zero;
}

    public void DecreaseMushroomCount()
    {
        currentMushrooms--;
    }
}