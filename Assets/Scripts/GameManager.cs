using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


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
    public TimeSetup timesetup;
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
        timesetup.SetAmount(60);
        cameraMovement.enabled = false;
        Time.timeScale = 1f;
        simulationUI.gameObject.SetActive(false);
        RenderSettings.skybox.SetFloat("_Exposure", 1f);
        RenderSettings.skybox.SetColor("_Tint", Color.white);
        summerToggle.isOn = true;
        SeasonManager.Instance.SetSummer(summerToggle.isOn);

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

        StartCoroutine(SpawnAnimalsStaggered(moosePrefab, mooseSetup.amount, herbivoresFolder));
        StartCoroutine(SpawnAnimalsStaggered(wolfPrefab, wolfSetup.amount, carnivoreFolder));
        StartCoroutine(SpawnAnimalsStaggered(bearPrefab, bearSetup.amount, omnivoreFolder));

        SpawnFood(berryBushPrefab, berryBushSetup.amount, berryBushFolder);
        
        //mushroomSpawner.SetMaxMushrooms(mushroomSetup.amount);
        //mushroomSpawner.InitializeSpawn();

        mushroomSpawner.SetMaxMushrooms(200); // current max amount
        mushroomSpawner.InitializeSpawn(mushroomSetup.amount);


        nutrientTreeSpawner.SetTreeAmount(nutrientTree.amount);
        nutrientTreeSpawner.InitializeSpawn();

        simulationTime = timesetup.amount;
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

                animal.InitializeAttributes();
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

    void ExportSimulationData()
    {
        if (StatisticsTableManager.instance == null) return;

        var sm = StatisticsTableManager.instance;
        
        // Build CSV header and data row
        string header = "BearFinalPopulation,WolfFinalPopulation,MooseFinalPopulation," +
            "BearBirths,WolfBirths,MooseBirths," +
            "BearDeaths,WolfDeaths,MooseDeaths," +
            "BearStarvation,WolfStarvation,MooseStarvation," +
            "BearPredation,WolfPredation,MoosePredation," +
            "BearPlantMeals,BearAnimalPrey,MoosePlantMeals,WolfCarcass," +
            "PacksFormed,PackHuntAttempts,PackHuntSuccess," +
            "BearAvgHunger,BearAvgThirst,BearAvgStamina," +
            "WolfAvgHunger,WolfAvgThirst,WolfAvgStamina," +
            "MooseAvgHunger,MooseAvgThirst,MooseAvgStamina," +
            "BearAvgLifespan,WolfAvgLifespan,MooseAvgLifespan";

        // Final population
        int bearPop = omnivoreFolder != null ? omnivoreFolder.childCount : 0;
        int wolfPop = carnivoreFolder != null ? carnivoreFolder.childCount : 0;
        int moosePop = herbivoresFolder != null ? herbivoresFolder.childCount : 0;

        // Pack hunt success rate
        float packHuntSuccessRate = sm.PackHuntAttemptsCount > 0 
            ? (sm.PackHuntSuccessCount / (float)sm.PackHuntAttemptsCount) * 100f 
            : 0f;

        System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;
        string data = $"{bearPop},{wolfPop},{moosePop}," +
            $"{sm.BearBirthCount},{sm.WolfBirthCount},{sm.MooseBirthCount}," +
            $"{sm.BearDeathCount},{sm.WolfDeathCount},{sm.MooseDeathCount}," +
            $"{sm.BearStarvationCount},{sm.WolfStarvationCount},{sm.MooseStarvationCount}," +
            $"{sm.BearPredationCount},{sm.WolfPredationCount},{sm.MoosePredationCount}," +
            $"{sm.BearPlantMealsCount},{sm.BearAnimalPreyCount},{sm.MoosePlantMealsCount},{sm.WolfCarcassCount}," +
            $"{sm.PacksFormedCount},{sm.PackHuntAttemptsCount},{sm.PackHuntSuccessCount}," +
            $"{SimulationResults.bearAvgHunger.ToString("F2", inv)},{SimulationResults.bearAvgThirst.ToString("F2", inv)},{SimulationResults.bearAvgStamina.ToString("F2", inv)}," +
            $"{SimulationResults.wolfAvgHunger.ToString("F2", inv)},{SimulationResults.wolfAvgThirst.ToString("F2", inv)},{SimulationResults.wolfAvgStamina.ToString("F2", inv)}," +
            $"{SimulationResults.mooseAvgHunger.ToString("F2", inv)},{SimulationResults.mooseAvgThirst.ToString("F2", inv)},{SimulationResults.mooseAvgStamina.ToString("F2", inv)}," +
            $"{CalculateAvgLifespan(Species.bear).ToString("F2", inv)},{CalculateAvgLifespan(Species.wolf).ToString("F2", inv)},{CalculateAvgLifespan(Species.moose).ToString("F2", inv)}";

        // Create output directory if it doesn't exist
        string outputDir = Path.Combine(Application.persistentDataPath, "SimulationData");
        Directory.CreateDirectory(outputDir);

        // Generate filename with timestamp
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(outputDir, $"simulation_{timestamp}.csv");

        // Write to file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(header);
            writer.WriteLine(data);
        }

        Debug.Log($"Simulation data exported to: {filePath}");
    }

    float CalculateAvgLifespan(Species species)
    {
        var sm = StatisticsTableManager.instance;
        if (sm == null) return 0f;

        return species switch
        {
            Species.bear => (sm.BearDeathCount + sm.BearSurvivorCount) > 0
                ? (sm.BearTotalAgeAtDeath + sm.BearSurvivorTotalAge) / (sm.BearDeathCount + sm.BearSurvivorCount)
                : 0f,
            Species.wolf => (sm.WolfDeathCount + sm.WolfSurvivorCount) > 0
                ? (sm.WolfTotalAgeAtDeath + sm.WolfSurvivorTotalAge) / (sm.WolfDeathCount + sm.WolfSurvivorCount)
                : 0f,
            Species.moose => (sm.MooseDeathCount + sm.MooseSurvivorCount) > 0
                ? (sm.MooseTotalAgeAtDeath + sm.MooseSurvivorTotalAge) / (sm.MooseDeathCount + sm.MooseSurvivorCount)
                : 0f,
            _ => 0f
        };
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

        SimulationResultsCalculator.CalculateStateAverages(herbivoresFolder,SimulationResults.mooseStateAverages);
        SimulationResultsCalculator.CalculateStateAverages(carnivoreFolder,SimulationResults.wolfStateAverages);
        SimulationResultsCalculator.CalculateStateAverages(omnivoreFolder,SimulationResults.bearStateAverages);

        SimulationResultsCalculator.CalculateNeedsAverages(omnivoreFolder, out SimulationResults.bearAvgHunger, out SimulationResults.bearAvgThirst, out SimulationResults.bearAvgStamina);
        SimulationResultsCalculator.CalculateNeedsAverages(carnivoreFolder, out SimulationResults.wolfAvgHunger, out SimulationResults.wolfAvgThirst, out SimulationResults.wolfAvgStamina);
        SimulationResultsCalculator.CalculateNeedsAverages(herbivoresFolder, out SimulationResults.mooseAvgHunger, out SimulationResults.mooseAvgThirst, out SimulationResults.mooseAvgStamina);

        SimulationResults.bearAvgHunger = Average(SimulationResults.bearHungerSamples);
        SimulationResults.bearAvgThirst = Average(SimulationResults.bearThirstSamples);
        SimulationResults.bearAvgStamina = Average(SimulationResults.bearStaminaSamples);
        SimulationResults.wolfAvgHunger = Average(SimulationResults.wolfHungerSamples);
        SimulationResults.wolfAvgThirst = Average(SimulationResults.wolfThirstSamples);
        SimulationResults.wolfAvgStamina = Average(SimulationResults.wolfStaminaSamples);
        SimulationResults.mooseAvgHunger = Average(SimulationResults.mooseHungerSamples);
        SimulationResults.mooseAvgThirst = Average(SimulationResults.mooseThirstSamples);
        SimulationResults.mooseAvgStamina = Average(SimulationResults.mooseStaminaSamples);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        SimulationResults.simulationLength = simulationTime;

        if (StatisticsTableManager.instance != null)
        {
            RecordSurvivorAges(omnivoreFolder, Species.bear);
            RecordSurvivorAges(carnivoreFolder, Species.wolf);
            RecordSurvivorAges(herbivoresFolder, Species.moose);
        }

        ExportSimulationData();

        SceneManager.LoadScene("SimOver");
    }

    void RecordPopulation()
    {
        SimulationResults.bearsHistory.Add(omnivoreFolder.childCount);
        SimulationResults.wolvesHistory.Add(carnivoreFolder.childCount);
        SimulationResults.mooseHistory.Add(herbivoresFolder.childCount);

        float h, t, s;
        SimulationResultsCalculator.CalculateNeedsAverages(omnivoreFolder, out h, out t, out s);
        SimulationResults.bearHungerSamples.Add(h);
        SimulationResults.bearThirstSamples.Add(t);
        SimulationResults.bearStaminaSamples.Add(s);

        SimulationResultsCalculator.CalculateNeedsAverages(carnivoreFolder, out h, out t, out s);
        SimulationResults.wolfHungerSamples.Add(h);
        SimulationResults.wolfThirstSamples.Add(t);
        SimulationResults.wolfStaminaSamples.Add(s);

        SimulationResultsCalculator.CalculateNeedsAverages(herbivoresFolder, out h, out t, out s);
        SimulationResults.mooseHungerSamples.Add(h);
        SimulationResults.mooseThirstSamples.Add(t);
        SimulationResults.mooseStaminaSamples.Add(s);
    }

    float Average(System.Collections.Generic.List<float> list)
    {
        if (list.Count == 0) return 0f;
        float sum = 0f;
        foreach (float v in list) sum += v;
        return Mathf.Round(sum / list.Count);
    }

    void RecordSurvivorAges(Transform folder, Species species)
    {
        var sm = StatisticsTableManager.instance;
        foreach (Transform child in folder)
        {
            if (child.CompareTag("carcass")) continue;
            Animal a = child.GetComponent<Animal>();
            if (a == null) continue;
            if (species == Species.bear)  { sm.BearSurvivorTotalAge += a.age; sm.BearSurvivorCount++; }
            else if (species == Species.wolf)  { sm.WolfSurvivorTotalAge += a.age; sm.WolfSurvivorCount++; }
            else if (species == Species.moose) { sm.MooseSurvivorTotalAge += a.age; sm.MooseSurvivorCount++; }
        }
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

    public void UpdateAmountText(TextMeshProUGUI amountText, string animalName, float sliderValue)
    {
        amountText.text = $"Amount of {animalName}: {Mathf.RoundToInt(sliderValue)}";
    }
}
