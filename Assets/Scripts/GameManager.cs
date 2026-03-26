using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;


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

    [Header("information UI")]
    public InformationUI informationUI;

    [Header("Simulation UI")]
    public SimulationUI simulationUI;

    public float spawnRadius = 1000f;

    [Header("Weather")]
    public GameObject raining;
    public Toggle rainToggle;

    public GameObject snowing;
    public Toggle snowToggle;

    [Header("Terrain")]
    public Terrain terrain;
    public TerrainLayer grassLayer;
    public TerrainLayer snowLayer;

    private void Start()
    {
        simulationLengthSlider.value = 60f;
        cameraMovement.enabled = false;
        Time.timeScale = 1f;
        simulationUI.gameObject.SetActive(false);
        RenderSettings.skybox.SetFloat("_Exposure", 1f);
        RenderSettings.skybox.SetColor("_Tint", Color.white);
        terrain.terrainData.terrainLayers = new TerrainLayer[] {grassLayer};

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

        nutrientTreeSpawner.SetTreeAmount(nutrientTree.amount);
        nutrientTreeSpawner.InitializeSpawn();

        simulationTime = simulationLengthSlider.value;
        timer = 0f;
        simulationRunning = true;

        startMenuPanel.SetActive(false);
        simulationUI.gameObject.SetActive(true);
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
            Vector3 randomPoint = GetRandomNavMeshPoint();
            Instantiate(berryBushPrefab, randomPoint, Quaternion.identity, parentFolder);
        }
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

    public static bool GetSimulationStatus()
    {
        return simulationRunning;
    }

    public void toggleRain()
    {
        if (snowing.activeSelf)
        {
            snowToggle.isOn = false;
            snowing.SetActive(false);
            RenderSettings.skybox.SetFloat("_Exposure", 1f);
            RenderSettings.skybox.SetColor("_Tint", Color.white);
            RenderSettings.fogDensity = 0.01f;
        }
        raining.SetActive(!raining.activeSelf);
        RenderSettings.skybox.SetFloat("_Exposure", raining.activeSelf ? 0.5f : 1f);
        RenderSettings.skybox.SetColor("_Tint", raining.activeSelf ? new Color(0.48f, 0.49f, 0.54f) : Color.white);
        RenderSettings.fogDensity = raining.activeSelf ? 0.02f : 0.01f;
        SetTerrainLayer(snowing.activeSelf);
    }

    public void toggleSnow()
    {
        if (raining.activeSelf)
        {
            rainToggle.isOn = false;
            raining.SetActive(false);
            RenderSettings.skybox.SetFloat("_Exposure", 1f);
            RenderSettings.skybox.SetColor("_Tint", Color.white);
            RenderSettings.fogDensity = 0.01f;
        }
        snowing.SetActive(!snowing.activeSelf);
        RenderSettings.skybox.SetFloat("_Exposure", snowing.activeSelf ? 0.8f : 1f);
        RenderSettings.skybox.SetColor("_Tint", snowing.activeSelf ? new Color(0.75f, 0.78f, 0.85f) : Color.white);
        RenderSettings.fogDensity = snowing.activeSelf ? 0.015f : 0.01f;
        SetTerrainLayer(snowing.activeSelf);

    }

    void SetTerrainLayer(bool snow)
{
    if (snow)
    {
        terrain.terrainData.terrainLayers = new TerrainLayer[] { snowLayer };
    }
    else
    {
        terrain.terrainData.terrainLayers = new TerrainLayer[] { grassLayer };
    }
}
}
