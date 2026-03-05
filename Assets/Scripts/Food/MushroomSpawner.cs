
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
    private bool isSimulationRunning = false;

    void Start()
    {
        isSimulationRunning = false;
    }

    public void InitializeSpawn()
    {
        isSimulationRunning = true;
        // Spawn half the mushrooms initially, rest will be spawned gradually in Update()
        int initialAmount = maxMushrooms / 2;
        for (int i = 0; i < initialAmount; i++)
        {
            SpawnMushroom();
        }
    }

    void Update()
    {
        // Only spawn additional mushrooms if simulation is running
        if (!isSimulationRunning)
            return;

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


    public void SetMaxMushrooms(int max)
    {
        maxMushrooms = max;
    }
}