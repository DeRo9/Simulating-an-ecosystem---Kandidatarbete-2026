using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Folders")]
    public Transform herbivoresFolder;
    public Transform carnivoreFolder;
    public Transform omnivoreFolder;

    [Header("UI")]
    public GameObject startMenuPanel;

    [Header("Prefabs")]
    public GameObject deerPrefab;

    public void StartSimulation()
    {
        SpawnAnimals(deerPrefab, herbivoresFolder);
        startMenuPanel.SetActive(false);
    }

    void SpawnAnimals(GameObject animalPrefab, Transform parentFolder)
    {
        Vector3 randomPoint = new Vector3(285.539246f, 55.4835625f, 264.506256f);
        GameObject animal = Instantiate(animalPrefab, randomPoint, Quaternion.identity, parentFolder);
    }
}
