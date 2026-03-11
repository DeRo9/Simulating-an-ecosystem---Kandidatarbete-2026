using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("Camera")]
    public FreeCamera cameraMovement;

    [Header("Folders")]
    public Transform herbivoresFolder;
    public Transform carnivoreFolder;
    public Transform omnivoreFolder;
    public Transform berryBushFolder;

    [Header("UI")]
    public GameObject startMenuPanel;

    [Header("Animal Setup Panels")]
    public AnimalSetupPanel mooseSetup;
    public AnimalSetupPanel wolfSetup;
    public AnimalSetupPanel bearSetup;

    [Header("Food Setup Panel")]
    public FoodSetupPanel berryBushSetup;
    public FoodSetupPanel mushroomSetup;

    [Header("Food")]
    public MushroomSpawner mushroomSpawner;

    [Header("Prefabs")]
    public GameObject moosePrefab;
    public GameObject wolfPrefab;
    public GameObject bearPrefab;

    public GameObject berryBushPrefab;

    [Header("Simulation Length")]
    public Slider simulationLengthSlider;
    public TextMeshProUGUI simulationTimeText;
    private float simulationTime;
    private float timer;
    private bool simulationRunning;

    [Header("information UI")]
    public InformationUI informationUI;

    public float spawnRadius = 1000f;

    private void Start()
    {
        simulationLengthSlider.value = 60f;
        cameraMovement.enabled = false;
        Time.timeScale = 1f;

    }

    public float recordInterval = 5f;
    private float recordTimer = 0f;

    void Update() 
    {
        if (!simulationRunning) 
        {
            return;
        }

        timer += Time.deltaTime;
        recordTimer += Time.deltaTime;

        if(recordTimer >= recordInterval)
        {
            RecordPopulation();
            recordTimer = 0f;
        }

        if (timer >= simulationTime)
        {
            EndSimulation();
        }
    }

    public void StartSimulation()
    {
        cameraMovement.enabled = true;

        SpawnAnimals(moosePrefab, mooseSetup, herbivoresFolder);
        SpawnAnimals(wolfPrefab, wolfSetup, carnivoreFolder);
        SpawnAnimals(bearPrefab, bearSetup, omnivoreFolder);
        SpawnFood(berryBushPrefab, berryBushSetup.amount, berryBushFolder);
        
        mushroomSpawner.SetMaxMushrooms(mushroomSetup.amount);
        mushroomSpawner.InitializeSpawn();

        simulationTime = simulationLengthSlider.value;
        timer = 0f;
        simulationRunning = true;

        startMenuPanel.SetActive(false);
    }

    void SpawnAnimals(GameObject animalPrefab, AnimalSetupPanel setup, Transform parentFolder)
    {
        
        for (int i = 0; i < setup.amount; i++)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            GameObject animalObj = Instantiate(animalPrefab, randomPoint, Quaternion.identity, parentFolder);
            Animal animal = animalObj.GetComponent<Animal>();


            if (animal != null)
            {
                animal.speed = setup.updatedSpeed;
                animal.size = setup.updatedSize;
                animal.sightRange = setup.updatedSight;
                animal.hearingRange = setup.updatedHearing;
            }
        }
    }

    void SpawnFood(GameObject foodPrefab, int count, Transform parentFolder)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            Instantiate(berryBushPrefab, randomPoint, Quaternion.identity, parentFolder);
        }
    }

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

    void EndSimulation ()
    {
        simulationRunning = false;

        /*SimulationResults.initialBearsAmount = bearSetup.amount;
        SimulationResults.finalBearsAmount = omnivoreFolder.childCount;

        SimulationResults.initialWolvesAmount = wolfSetup.amount;
        SimulationResults.finalWolvesAmount = carnivoreFolder.childCount;

        SimulationResults.initialMooseAmount = mooseSetup.amount;
        SimulationResults.finalMooseAmount = herbivoresFolder.childCount;*/

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        SimulationResults.simulationLength = simulationTime;

        SceneManager.LoadScene("SimOver");
    }

    public void UpdateTimeText()
    {
        simulationTimeText.text = $"Simulation Length: {simulationLengthSlider.value} seconds";
    }

    void RecordPopulation()
    {
        SimulationResults.bearsHistory.Add(omnivoreFolder.childCount);
        SimulationResults.wolvesHistory.Add(carnivoreFolder.childCount);
        SimulationResults.mooseHistory.Add(herbivoresFolder.childCount);

    }


}
