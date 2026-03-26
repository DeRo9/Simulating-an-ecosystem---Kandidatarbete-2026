using UnityEngine;
public class RainSnowFollow : MonoBehaviour
{
    [Header("Rain Grid Settings")]
    public Transform player;
    public GameObject rainPrefab;
    public float cellSize = 35f;
    public float yOffset = 30f;

    private Vector2Int currentCell = new Vector2Int(-1000, -1000);
    private GameObject[,] rainGrid = new GameObject[3, 3];

    void Start()
    {

        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                GameObject instance = Instantiate(rainPrefab, transform);
                instance.name = $"RainCell_{x - 1}_{z - 1}";
                rainGrid[x, z] = instance;
            }
        }
        currentCell = GetCellCoord(player.position);
        UpdateRainGrid(currentCell);
    }

    void Update()
    {
        if (player == null || rainPrefab == null)
            return;

        Vector2Int playerCell = GetCellCoord(player.position);

        if (playerCell != currentCell)
        {
            currentCell = playerCell;
            UpdateRainGrid(currentCell);
        }
    }

    Vector2Int GetCellCoord(Vector3 position)
    {
        int cellX = Mathf.FloorToInt(position.x / cellSize);
        int cellZ = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(cellX, cellZ);
    }

    void UpdateRainGrid(Vector2Int cellCoord)
    {
        Vector3 playerPos = player.position;
        Vector2Int center = cellCoord;

        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                GameObject cell = rainGrid[x, z];
                if (cell == null) continue;

                float worldX = (center.x + (x - 1)) * cellSize;
                float worldZ = (center.y + (z - 1)) * cellSize;
                Vector3 targetPos = new Vector3(worldX + cellSize / 2f, playerPos.y + yOffset, worldZ + cellSize / 2f);

                cell.transform.position = targetPos;
                if (!cell.activeSelf)
                    cell.SetActive(true);

                ParticleSystem ps = cell.GetComponent<ParticleSystem>();
                if (ps != null && !ps.isPlaying)
                    ps.Play();
            }
        }
    }

}
