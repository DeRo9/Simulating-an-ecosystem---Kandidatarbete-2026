using UnityEngine;


// The idea:
// if animal is hungry
// 1. check nearby food
// 2. if none found, move to best remembered food area


// Overview:
// * Remember where food was found
// * Remember where danger occured
// * Over time, memory slowly fades
// * When hungry, go to best remembered food area
// * When in danger, flee to safest remembered area


public class AnimalMemory : MonoBehaviour
{

    //chunk size (chunks that will gain points)
    public float chunkSize = 50f;

    // Example below is for the 200 x 200 world 
    //
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]
    // [0,0][0,0][0,0][0,0]


    // 2D arrays
    // Example, foodMemory[2,1] = 5f means chunk (2,1) is remembered to have food (5)
    float[,] foodMemory;
    float[,] dangerMemory;
    float[,] preyMemory;


    int gridSizeX;
    int gridSizeZ;

    // Memory slowsly fades over time
    float memoryDecayRate = 0.1f;

    Vector3 terrainOrigin;
    Vector3 terrainSize;

    void Start()
    {
        // Get current terrain
        Terrain terrain = Terrain.activeTerrain;

        // Store terrain position and size
        terrainOrigin = terrain.transform.position;
        terrainSize = terrain.terrainData.size;

        // Calculate grid size
        gridSizeX = Mathf.CeilToInt(terrainSize.x / chunkSize);
        gridSizeZ = Mathf.CeilToInt(terrainSize.z / chunkSize);

        // Each chunk has memory value
        foodMemory = new float[gridSizeX, gridSizeZ];
        dangerMemory = new float[gridSizeX, gridSizeZ];
        preyMemory = new float[gridSizeX, gridSizeZ];
    }

    void Update()
    {
        // Go through all chunks
        for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Decrease memory
                foodMemory[x, z] -= memoryDecayRate * Time.deltaTime;
                dangerMemory[x, z] -= memoryDecayRate * Time.deltaTime;
                preyMemory[x,z] -= memoryDecayRate * Time.deltaTime;

                // Negative values not ok
                foodMemory[x, z] = Mathf.Max(0, foodMemory[x,z]);
                dangerMemory[x, z] = Mathf.Max(0, dangerMemory[x,z]);
                preyMemory[x, z] = Mathf.Max(0, preyMemory[x, z]);
            }
    }


    // Convert world position to chunk (which chunk am I in?)
    public Vector2Int GetChunk(Vector3 position)
    {
        float localX = position.x - terrainOrigin.x;
        float localZ = position.z - terrainOrigin.z;

        int x = Mathf.Clamp((int)(localX / chunkSize),0,gridSizeX-1);
        int z = Mathf.Clamp((int)(localZ / chunkSize),0,gridSizeZ-1);

        return new Vector2Int(x,z);
    }


    // Chunk to random world position in the specific chunk
    public Vector3 GetRandomPointInChunk(Vector2Int chunk)
    {
        float minX = terrainOrigin.x + chunk.x * chunkSize;
        float minZ = terrainOrigin.z + chunk.y * chunkSize;

        float randomX = Random.Range(minX, minX + chunkSize);
        float randomZ = Random.Range(minZ, minZ + chunkSize);

        float y = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ));

        return new Vector3(randomX, y, randomZ);
    }


    // Find chunk, add food memory
    public void RememberFood(Vector3 pos)
    {
        var chunkpos = GetChunk(pos);
        foodMemory[chunkpos.x,chunkpos.y] += 4f;
    }


    public void RememberDanger(Vector3 pos)
    {
        var chunkpos = GetChunk(pos);
        dangerMemory[chunkpos.x, chunkpos.y] += 7f;  // Danger is remembered longer than food
    }

    public void RememberPrey(Vector3 pos)
    {
        var chunkpos = GetChunk(pos);
        preyMemory[chunkpos.x,chunkpos.y] += 5f;
    }
    // Returns best food chunk
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

    void GetBestPreyChunk()
    {
        float bestvalue = 0f;
        Vector2Int bestchunk = new Vector2Int(-1, -1);
        for (int x = 0; x < gridSizeX; x++)
            for (int z = 0; z < gridSizeZ; z++)
            {
                if (preyMemory[x, z] > bestvalue)
                {
                    bestvalue = preyMemory[x, z];
                    bestchunk = new Vector2Int(x, z);
                }
            }
    }


    // Returns safest chunk
    public Vector2Int GetSafestChunk()
    {
        float safestValue = float.MaxValue; // start VERY high
        Vector2Int safestChunk = new Vector2Int(-1, -1);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                if (dangerMemory[x, z] < safestValue)
                {
                    safestValue = dangerMemory[x, z];
                    safestChunk = new Vector2Int(x, z);
                }
            }
        }

        return safestChunk;
    }


    public float GetFoodValue(int x, int z)
    {
        return foodMemory[x,z];
    }


    public float GetDangerValue(int x, int z)
    {
        return dangerMemory[x,z];
    }

    public float GetPreyValue(int x, int z)
    {
        return preyMemory[x,z];
    }
    public int GetGridSizeX() => gridSizeX;
    public int GetGridSizeZ() => gridSizeZ;




    // Implement this is each animal behavior class


    // when hungry, they need to go through dangerous point, it can either go around it,
    // or risk it depening on how hungry it is. Or just keep searching where it is at depending
    // on how desperate it is


    // TODO
    // Add some calculations or logic regarding choosing chunk depending on risk
    // priorities, weighting the hunger and the danger
    // If 50% hungry, search in new area or chunk, or a random adjacent chunk that is the safest


    // TODO
    // If 30% hungry, go to best food chunk, but if it means danger to get there, look for the
    // next best food chunk, if still danger, look for next best food chunk that is safe to go to


    // TODO
    // If 15% hungry, risk it and go to best food chunk even if danger it may be danger to go there



}