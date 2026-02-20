using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    [Header("Folders")]
    public Transform herbivoresFolder;
    public Transform carnivoreFolder;
    public Transform omnivoreFolder;

    [Header("UI")]
    public GameObject startMenuPanel;
    public Slider deerAmountSlider;
    public TextMeshProUGUI deerAmountSliderText;


    [Header("Prefabs")]
    public GameObject deerPrefab;


    public void StartSimulation()
    {
        int deerCount = (int)deerAmountSlider.value;
        SpawnAnimals(deerPrefab, deerCount, herbivoresFolder);
        startMenuPanel.SetActive(false);
    }

    void SpawnAnimals(GameObject animalPrefab, int count, Transform parentFolder)
    {
        Vector3 Point = new Vector3(285.539246f, 55.4835625f, 264.506256f);
        Instantiate(animalPrefab, Point, Quaternion.identity, parentFolder);
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            Instantiate(animalPrefab, randomPoint, Quaternion.identity, parentFolder);
        }
    }

    public float spawnRadius = 1000;

    Vector3 GetRandomNavMeshPoint()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            Vector3 randomPoint = transform.position + 
                                new Vector3(randomCircle.x, 500f, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1000f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        Debug.LogWarning("Failed to find NavMesh point");
        return transform.position;
    }

    public void UpdateDeerAmountSliderText()
    {
        deerAmountSliderText.text = "Amount of Deer: " + deerAmountSlider.value;
    }
}
