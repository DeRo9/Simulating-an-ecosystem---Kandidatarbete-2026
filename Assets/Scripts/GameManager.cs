using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;


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
    public FoodSetupPanel nutrientTree; //??

    [Header("Food")]
    public MushroomSpawner mushroomSpawner;
    public NutrientTreeSpawner nutrientTreeSpawner; //??

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
    private static bool simulationRunning;

    [Header("Reproducibility")]

    // Option to use a fixed seed for reproducibility of results.
    //  If false, a random seed will be used based on current time.
    public bool useFixedSeed = false;
    public int simulationSeed = 12345;

    [Header("information UI")]
    public InformationUI informationUI;

    [Header("Simulation UI")]
    public SimulationUI simulationUI;

    public float spawnRadius = 1000f;

    [Header("Weather")]
    
    [Header("Season")]
    public Toggle summerToggle;
    public Toggle winterToggle;
    public Toggle percipitationToggle; // set >0 for precipitation active (rain/snow based on season)

    // Performance optimization: precomputed spawn points
    private List<Vector3> recordedSpawnPoints = new List<Vector3>();
    private const int recordedSpawnPointsCount = 1000;
    public float recordInterval = 5f;
    private Coroutine recordingCoroutine;

    private void Awake()
    {
        for (int i = 0; i < recordedSpawnPointsCount; i++)
        {
            Vector3 point = GetRandomNavMeshPoint();
            recordedSpawnPoints.Add(point);
        }
    }

    private void Start()
    {
        simulationLengthSlider.value = 60f;
        cameraMovement.enabled = false;
        Time.timeScale = 1f;
        simulationUI.gameObject.SetActive(false);
        RenderSettings.skybox.SetFloat("_Exposure", 1f);
        RenderSettings.skybox.SetColor("_Tint", Color.white);
        summerToggle.isOn = true;

    }

    void Update() 
    {
        if (!simulationRunning) 
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= simulationTime)
        {
            EndSimulation();
        }
    }

    public void StartSimulation()
    {
        if(StatisticsTableManager.instance != null) StatisticsTableManager.instance.ResetStats();

        if (useFixedSeed)
        {
            UnityEngine.Random.InitState(simulationSeed);
        }

        cameraMovement.enabled = true;

        SpawnAnimals(moosePrefab, mooseSetup, herbivoresFolder);
        SpawnAnimals(wolfPrefab, wolfSetup, carnivoreFolder);
        SpawnAnimals(bearPrefab, bearSetup, omnivoreFolder);
        SpawnFood(berryBushPrefab, berryBushSetup.amount, berryBushFolder);
        
        mushroomSpawner.SetMaxMushrooms(mushroomSetup.amount);
        mushroomSpawner.InitializeSpawn();

        nutrientTreeSpawner.SetTreeAmount(nutrientTree.amount);
        nutrientTreeSpawner.InitializeSpawn();

        simulationTime = simulationLengthSlider.value;
        timer = 0f;
        simulationRunning = true;

        startMenuPanel.SetActive(false);
        simulationUI.gameObject.SetActive(true);

        // Start population recording coroutine instead of using Update timer
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
        }
        recordingCoroutine = StartCoroutine(RecordPopulationCoroutine());
    }

    void SpawnAnimals(GameObject animalPrefab, AnimalSetupPanel setup, Transform parentFolder)
    {
        
        for (int i = 0; i < setup.amount; i++)
        {
            Vector3 randomPoint = GetPrecomputedSpawnPoint();
            GameObject animalObj = Instantiate(animalPrefab, randomPoint, Quaternion.identity, parentFolder);
            Animal animal = animalObj.GetComponent<Animal>();


            if (animal != null)
            {
                animal.age = (float)System.Math.Round(UnityEngine.Random.Range(0f, animal.startingMaxAge), 2);                
                animal.speed = (float)System.Math.Round(UnityEngine.Random.Range(setup.updatedSpeed - 0.5f, setup.updatedSpeed + 0.5f), 2);
                animal.size = (float)System.Math.Round(UnityEngine.Random.Range(setup.updatedSize - 0.2f, setup.updatedSize + 0.2f), 2);
                animal.sightRange = (float)System.Math.Round(UnityEngine.Random.Range(setup.updatedSight - 5f, setup.updatedSight + 5f), 2);
                animal.hearingRange = (float)System.Math.Round(UnityEngine.Random.Range(setup.updatedHearing - 5f, setup.updatedHearing + 5f), 2);
            }
        }
    }

    void SpawnFood(GameObject foodPrefab, int count, Transform parentFolder)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPoint = GetPrecomputedSpawnPoint();
            Instantiate(berryBushPrefab, randomPoint, Quaternion.identity, parentFolder);
        }
    }

    Vector3 GetPrecomputedSpawnPoint()
    {
        if (recordedSpawnPoints.Count == 0)
        {
            Debug.LogWarning("Recorded spawn points not initialized, falling back to NavMesh sampling");
            return GetRandomNavMeshPoint();
        }
        return recordedSpawnPoints[UnityEngine.Random.Range(0, recordedSpawnPoints.Count)];
    }

    Vector3 GetRandomNavMeshPoint()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;

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

        // Stop recording coroutine when simulation ends
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }

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

    private IEnumerator RecordPopulationCoroutine()
    {
        // Records population at intervals without per-frame polling
        while (simulationRunning)
        {
            yield return new WaitForSeconds(recordInterval);
            if (simulationRunning)  // Double-check in case sim ended
            {
                RecordPopulation();
            }
        }
    }

    public static bool GetSimulationStatus()
    {
        return simulationRunning;
    }

    public void toggleSummer()
    {
        SeasonManager.Instance.SetSummer(summerToggle.isOn);
    }

    public void toggleWinter()
    {
        SeasonManager.Instance.SetWinter(winterToggle.isOn);
    }

    public void togglePrecipitation()
    {
        SeasonManager.Instance.SetPercipitation(percipitationToggle.isOn);
    }
}
