using UnityEngine;


// The idea:
// if animal is hungry
// 1. check nearby food
// 2. if none found, move to best remembered food area


public class AnimalMemory : MonoBehaviour
{

    //chunk size (chunks that will gain points)
    public float chunkSize = 50f; 


    // each chunk  [col, row], ex: foodMemory[3,1] = 5f, [3,1] refer to specific chunk, =5f is food-"points"
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]

    float[,] foodMemory;
    float[,] dangerMemory;


    int gridSizeX;
    int gridSizeZ;

    float memoryDecayRate = 0.1f;

    Vector3 terrainOrigin;
    Vector3 terrainSize;

    void Start()
    {
        Terrain terrain = Terrain.activeTerrain;

        terrainOrigin = terrain.transform.position;
        terrainSize = terrain.terrainData.size;

        gridSizeX = Mathf.CeilToInt(terrainSize.x / chunkSize);
        gridSizeZ = Mathf.CeilToInt(terrainSize.z / chunkSize);

        foodMemory = new float[gridSizeX, gridSizeZ];
        dangerMemory = new float[gridSizeX, gridSizeZ];
    }

    void Update()
    {
        for(int x = 0; x < gridSizeX; x++)
        for(int z = 0; z < gridSizeZ; z++)
        {
            foodMemory[x,z] -= memoryDecayRate * Time.deltaTime;
            dangerMemory[x,z] -= memoryDecayRate * Time.deltaTime;

            foodMemory[x,z] = Mathf.Max(0, foodMemory[x,z]);
            dangerMemory[x,z] = Mathf.Max(0, dangerMemory[x,z]);
        }
    }

    // Which chunk am I in? (worldPos to chunk)
    Vector2Int GetChunk(Vector3 position)
    {
        float localX = position.x - terrainOrigin.x;
        float localZ = position.z - terrainOrigin.z;

        int x = Mathf.Clamp((int)(localX / chunkSize),0,gridSizeX-1);
        int z = Mathf.Clamp((int)(localZ / chunkSize),0,gridSizeZ-1);

        return new Vector2Int(x,z);
    }


    // (chunk to worldPos)
    public Vector3 GetRandomPointInChunk(Vector2Int chunk) 
    {
        float minX = terrainOrigin.x + chunk.x * chunkSize;
        float minZ = terrainOrigin.z + chunk.y * chunkSize;

        float randomX = Random.Range(minX, minX + chunkSize);
        float randomZ = Random.Range(minZ, minZ + chunkSize);

        float y = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ));

        return new Vector3(randomX, y, randomZ);
    }

    

    public void RememberFood(Vector3 pos)
    {
        var chunkpos = GetChunk(pos);
        foodMemory[chunkpos.x,chunkpos.y] += 4f; 
    }

    public void RememberDanger(Vector3 pos)
    {
        var chunkpos = GetChunk(pos);
        dangerMemory[chunkpos.x,chunkpos.y] += 7f;
    }


    // Checks all cells and keeps the higesht food
    public Vector2Int GetBestFoodChunk()
    {
        float bestValue = 0f;
        Vector2Int bestChunk = new Vector2Int(-1, -1);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                if (foodMemory[x, z] > bestValue)
                {
                    bestValue = foodMemory[x, z];
                    bestChunk = new Vector2Int(x, z);
                }
            }
        }   

        return bestChunk;
    }


    //TODO
    // GetSafestChunk(): when animals fleeing, they go for a safe spot, also,
    // when hungry, they need to go through dangerous point, it can either go around it,
    // or risk it depening on how hungry it is


}