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

    public Transform berryBushFolder;

    [Header("UI")]
    public GameObject startMenuPanel;
    public Slider mooseAmountSlider;
    public TextMeshProUGUI mooseAmountSliderText;
    public Slider wolfAmountSlider;
    public TextMeshProUGUI wolfAmountSliderText;
    public Slider bearAmountSlider;
    public TextMeshProUGUI bearAmountSliderText;


    [Header("Prefabs")]
    public GameObject moosePrefab;
    public GameObject wolfPrefab;
    public GameObject bearPrefab;

    public GameObject berryBushPrefab;

    [Header("information UI")]
    public InformationUI informationUI;

    public void StartSimulation()
    {
        int mooseCount = (int)mooseAmountSlider.value;
        int wolfCount = (int)wolfAmountSlider.value;
        int bearCount = (int)bearAmountSlider.value;
        SpawnAnimals(moosePrefab, mooseCount, herbivoresFolder);
        SpawnAnimals(wolfPrefab, wolfCount, carnivoreFolder);
        SpawnAnimals(bearPrefab, bearCount, omnivoreFolder);
        SpawnFood(berryBushPrefab, 100, berryBushFolder);
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
            informationUI.SetType("Moose");
        }
    }

    void SpawnFood(GameObject foodPrefab, int count, Transform parentFolder)
    {
        //Vector3 Point2 = new Vector3(285.539246f, 55.4835625f, 264.506256f);
        //Instantiate(berryBushPrefab, Point2, Quaternion.identity, berryBushFolder);
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            Instantiate(berryBushPrefab, randomPoint, Quaternion.identity, parentFolder);
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

    public void UpdateMooseAmountSliderText()
    {
        mooseAmountSliderText.text = "Amount of Moose: " + mooseAmountSlider.value;
    }

    public void UpdateWolfAmountSliderText()
    {
        wolfAmountSliderText.text = "Amount of Wolves: " + wolfAmountSlider.value;
    }

    public void UpdateBearAmountSliderText()
    {
        bearAmountSliderText.text = "Amount of Bears: " + bearAmountSlider.value;
    }
}
