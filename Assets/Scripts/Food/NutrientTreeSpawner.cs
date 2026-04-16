using UnityEngine;
using UnityEngine.AI;

public class NutrientTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject nutrientTreePrefab;
    [SerializeField] private Transform nutrientTree;
    [SerializeField] private int nutrientTreeInitializationAmount; //= 200f???

    private bool isSimulationRunning = false;

    void Start()
    {
        isSimulationRunning = false;
    }

    public void InitializeSpawn()
    {
        isSimulationRunning = true;

        for (int i = 0; i < nutrientTreeInitializationAmount; i++)
        {
            SpawnNutritionTree();
        }
    }

    void SpawnNutritionTree()
    {
        Vector3 pos = GetRandomTerrainPoint();

        if (pos != Vector3.zero)
        {
            Instantiate(nutrientTreePrefab, pos, Quaternion.identity, nutrientTree);
            
        }
        
    }

   Vector3 GetRandomTerrainPoint()
   {
       Terrain terrain = Terrain.activeTerrain;

       Vector3 terrainPos = terrain.transform.position;
       Vector3 terrainSize = terrain.terrainData.size;

       float randomX = Random.Range(0, terrainSize.x);
       float randomZ = Random.Range(0, terrainSize.z);

       float y = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));

       Vector3 worldPos = new Vector3(
            terrainPos.x + randomX,
            y + terrainPos.y,
            terrainPos.z + randomZ
       );

        NavMeshHit hit;
        if(NavMesh.SamplePosition(worldPos, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }



        return Vector3.zero;
   }

    public void SetTreeAmount(int amount)
    {
        nutrientTreeInitializationAmount = amount;
    }

}