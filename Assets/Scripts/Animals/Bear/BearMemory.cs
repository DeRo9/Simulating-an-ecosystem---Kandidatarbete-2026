using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BearMemory : AnimalMemory
{
    
    float[,] preyMemory;

    void Start()
    {
        base.Start();
        preyMemory = new float [GetGridSizeX(),GetGridSizeZ()];
    }

    void Update()
    {
        base.Update();
        for(int x = 0; x < GetGridSizeX();x++);
        for(int z = 0; z < GetGridSizeZ();z++);
        {
            preyMemory[x,z] -= 0.1f * Time.deltaTime;
            preyMemory[x,z] = Mathf.Max(0,preyMemory[x,z]);
        }
    }

    public void RememberPrey(Vector3 pos)
    {
        var chunk = GetChunk (pos);
        preyMemory[chunk.x,chunk.y] += 5f;

    }

    public Vector2Int GetBestPreyChunk()
    {
        float bestValue = 0f;
        Vector2Int bestChunk = new Vector2Int(-1, -1);

        for (int x = 0; x < GetGridSizeX(); x++)
        for (int z = 0; z < GetGridSizeZ(); z++)
        {
            if (preyMemory[x, z] > bestValue)
            {
                bestValue = preyMemory[x, z];
                bestChunk = new Vector2Int(x, z);
            }
        }

        return bestChunk;
    }

    public float GetPreyValue(int x, int z)
    {
        return preyMemory[x, z];
    }
}