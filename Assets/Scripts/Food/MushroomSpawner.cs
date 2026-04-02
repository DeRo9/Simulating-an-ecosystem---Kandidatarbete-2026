
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; // To be able to use lists

public class MushroomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mushroomPrefab;
    [SerializeField] private Transform Mushrooms;

    [SerializeField] private int mushroomMaxInitialization = 30;
    [SerializeField] private int mushroomMapLimit = 500;

    [SerializeField] private float SpawnRadius = 100f;
    [SerializeField] private float reproduceRadius = 10f;
    [SerializeField] private float timeInterval = 10f; // 1 second = 1 hour

    private float timer;
    private bool isSimulationRunning = false;
    private int maxMushrooms;

    private readonly List<GameObject> mushrooms = new List<GameObject>();

    void Start()
    {
    
        maxMushrooms = mushroomMaxInitialization;
        isSimulationRunning = false;
    }

    public void InitializeSpawn()
    {
        isSimulationRunning = true;

        
        if (SeasonManager.Instance.IsWinter)
        {
            maxMushrooms = mushroomMaxInitialization / 20;
        }
        else if (SeasonManager.Instance.IsSummer)
        {
            maxMushrooms = mushroomMaxInitialization;
        }

        int initialAmount = Mathf.Min(maxMushrooms / 2, mushroomMapLimit);
        for (int i = 0; i < initialAmount; i++)
        {
            SpawnMushrooms();
        }
    }

    void Update()
    {
        if (!isSimulationRunning)
            return;

        timer += Time.deltaTime;

        if (timer >= timeInterval)
        {
            timer = 0f;
            ReproduceMushrooms();
        }
    }

    void ReproduceMushrooms()
    {
        mushrooms.RemoveAll(item => item == null);
        List<GameObject> current = new List<GameObject>(mushrooms);

        float baseChance;

        if (SeasonManager.Instance.IsWinter)
        {
            baseChance = 0.05f;
        }
        else if (SeasonManager.Instance.IsSummer)
            baseChance = 0.6f;
        else if (SeasonManager.Instance.IsRaining)
            baseChance = 0.8f;

        foreach (GameObject mushroom in current)
        {
            if (mushrooms.Count >= mushroomMapLimit || mushrooms.Count >= maxMushrooms)
                return;

            if (Random.value > 0.5f)
                continue;

            Vector3 spawnPos;

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
        if (mushrooms.Count >= mushroomMapLimit || mushrooms.Count >= maxMushrooms)
            return;

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

    public void SetMaxMushrooms(int max)
    {
        maxMushrooms = Mathf.Max(0, max);
    }
}