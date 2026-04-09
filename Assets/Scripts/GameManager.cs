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
    public GameObject startMenu;

    
    [Header("Animal Setup Sliders")]
    public Slider mooseSlider;
    public Slider wolfSlider;
    public Slider bearSlider;

    public TextMeshProUGUI mooseText;
    public TextMeshProUGUI wolfText;

    public TextMeshProUGUI bearText;

    [Header("Food Setup Sliders")]
    public Slider berrySlider;
    public Slider mushroomSlider;
    public Slider nutrientTreeSlider; 

    public TextMeshProUGUI berryText;
    public TextMeshProUGUI mushroomText;

    public TextMeshProUGUI nutrientTreeText;

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
    private const int recordedSpawnPointsCount = 5000;
    public float recordInterval = 5f;
    public float spawnSpacing = 5f;
    
    private Coroutine recordingCoroutine;
    private bool spawnPointsInitialized = false;


    private void Start()
    {
        simulationLengthSlider.value = 60f;
        cameraMovement.enabled = false;
        Time.timeScale = 1f;
        simulationUI.gameObject.SetActive(false);
        RenderSettings.skybox.SetFloat("_Exposure", 1f);
        RenderSettings.skybox.SetColor("_Tint", Color.white);
        summerToggle.isOn = true;
        SeasonManager.Instance.SetSummer(summerToggle.isOn);

        mooseSlider.onValueChanged.AddListener(delegate { UpdateAmountText(mooseText, "Moose", mooseSlider.value); });
        wolfSlider.onValueChanged.AddListener(delegate { UpdateAmountText(wolfText, "Wolves", wolfSlider.value); });
        bearSlider.onValueChanged.AddListener(delegate { UpdateAmountText(bearText, "Bears", bearSlider.value); });
        berrySlider.onValueChanged.AddListener(delegate { UpdateAmountText(berryText, "Berry Bushes", berrySlider.value); });
        mushroomSlider.onValueChanged.AddListener(delegate { UpdateAmountText(mushroomText, "Mushrooms", mushroomSlider.value); });
        nutrientTreeSlider.onValueChanged.AddListener(delegate { UpdateAmountText(nutrientTreeText, "Nutrient Trees", nutrientTreeSlider.value); });
        simulationLengthSlider.onValueChanged.AddListener(delegate { UpdateTimeText(); });

        InitializeSpawnPoints();
    }

    private void InitializeSpawnPoints()
    {
        if (spawnPointsInitialized) return;
        
        HashSet<Vector3> uniquePoints = new HashSet<Vector3>();
        int attempts = 0;
        int maxAttempts = recordedSpawnPointsCount * 3; // Try 3x to get enough unique points
        
        while (uniquePoints.Count < recordedSpawnPointsCount && attempts < maxAttempts)
        {
            Vector3 point = GetRandomNavMeshPoint();
            
            // Only add if it's not the fallback position and not a duplicate
            if (point != transform.position && !IsPointTooClose(point, uniquePoints))
            {
                uniquePoints.Add(point);
            }
            attempts++;
        }
        
        recordedSpawnPoints.AddRange(uniquePoints);
        spawnPointsInitialized = true;
    }

    private bool IsPointTooClose(Vector3 point, HashSet<Vector3> existingPoints)
    {
        foreach (Vector3 existing in existingPoints)
        {
            if (Vector3.Distance(point, existing) < 5f)
            {
                return true;
            }
        }
        return false;
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

        StartCoroutine(SpawnAnimalsStaggered(moosePrefab, (int)mooseSlider.value, herbivoresFolder));
        StartCoroutine(SpawnAnimalsStaggered(wolfPrefab, (int)wolfSlider.value, carnivoreFolder));
        StartCoroutine(SpawnAnimalsStaggered(bearPrefab, (int)bearSlider.value, omnivoreFolder));

        SpawnFood(berryBushPrefab, (int)berrySlider.value, berryBushFolder);
        
        mushroomSpawner.SetMaxMushrooms((int)mushroomSlider.value);
        mushroomSpawner.InitializeSpawn();

        nutrientTreeSpawner.SetTreeAmount((int)nutrientTreeSlider.value);
        nutrientTreeSpawner.InitializeSpawn();

        simulationTime = simulationLengthSlider.value;
        timer = 0f;
        simulationRunning = true;

        startMenu.SetActive(false);
        simulationUI.gameObject.SetActive(true);

        // Start population recording coroutine instead of using Update timer
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
        }
        recordingCoroutine = StartCoroutine(RecordPopulationCoroutine());
    }

    //private IEnumerator SpawnAnimalsStaggered(GameObject animalPrefab, AnimalSetupPanel setup, Transform parentFolder)
    private IEnumerator SpawnAnimalsStaggered(GameObject animalPrefab, int amount, Transform parentFolder)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPoint = GetPrecomputedSpawnPoint();
            randomPoint += new Vector3(UnityEngine.Random.Range(-spawnSpacing, spawnSpacing), 0f, UnityEngine.Random.Range(-spawnSpacing, spawnSpacing));

            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas)) // Ensure the point is on the NavMesh and adjust height,
                                                                                    // otherwise might spawn outside of the terrain
            {
                randomPoint = hit.position + Vector3.up * 2f;
            }
            
            GameObject animalObj = Instantiate(animalPrefab, randomPoint, Quaternion.identity, parentFolder);
            Animal animal = animalObj.GetComponent<Animal>();

            if (animal != null)
            {
            
                animal.age = (float)System.Math.Round(UnityEngine.Random.Range(0f, animal.startingMaxAge), 2);               

                // Randomize stats from species ranges
                animal.speed = UnityEngine.Random.Range(animal.minSpeed, animal.maxSpeed);
                animal.runningSpeed = animal.speed * 1.5f; //What would make sense?
                animal.sightRange = UnityEngine.Random.Range(animal.minSight, animal.maxSight);
                animal.hearingRange = UnityEngine.Random.Range(animal.minHearing, animal.maxHearing);


                animal.strength = UnityEngine.Random.Range(animal.minStrength, animal.maxStrength);

                // Attack damage depends on strength
                animal.CalculateAttackDamage();



            }

            yield return new WaitForEndOfFrame();
        }
    }

    void SpawnFood(GameObject foodPrefab, int count, Transform parentFolder)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPoint = GetPrecomputedSpawnPoint();
            Instantiate(foodPrefab, randomPoint, Quaternion.identity, parentFolder);
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
            
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 1000f, randomCircle.y);

            NavMeshHit hit;
            // Search downward with reasonable distance to find the surface
            if (NavMesh.SamplePosition(randomPoint, out hit, 2000f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        Debug.LogWarning("Failed to find a valid NavMesh point after 100 attempts");
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

    public void ToggleSummer()
    {
        summerToggle.SetIsOnWithoutNotify(true);
        winterToggle.SetIsOnWithoutNotify(false);
        SeasonManager.Instance.SetSummer(true);
    }

    public void ToggleWinter()
    {
        winterToggle.SetIsOnWithoutNotify(true);
        summerToggle.SetIsOnWithoutNotify(false);
        SeasonManager.Instance.SetWinter(true);
    }

    public void togglePrecipitation()
    {
        SeasonManager.Instance.SetPercipitation(percipitationToggle.isOn);
    }

    public void UpdateTimeText()
    {
        simulationTimeText.text = $"Simulation Length: {simulationLengthSlider.value} seconds";
    }

    public void UpdateAmountText(TextMeshProUGUI amountText, string animalName, float sliderValue)
    {
        amountText.text = $"Amount of {animalName}: {Mathf.RoundToInt(sliderValue)}";
    }
}
