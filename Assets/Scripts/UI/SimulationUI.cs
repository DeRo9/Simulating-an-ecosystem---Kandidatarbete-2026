using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationUI : MonoBehaviour
{
    public static SimulationUI Instance;

    [Header("FPS Text")]
    [SerializeField] TextMeshProUGUI FPS;
    private float fps;

    [Header("Simulation Time")]
    [SerializeField] TextMeshProUGUI SimTime;
    private float time;

    [Header("Count Animals And Plants")]
    [SerializeField] TextMeshProUGUI Wolves;
    [SerializeField] TextMeshProUGUI Bears;
    [SerializeField] TextMeshProUGUI Moose;
    [SerializeField] TextMeshProUGUI Plants;

    [Header("Deaths")]
    [SerializeField] TextMeshProUGUI PreyDeaths;
    [SerializeField] TextMeshProUGUI PredatorDeaths;
    private int PreyDeathsCount;
    private int PredatorDeathsCount;

    [Header("Matings")]
    [SerializeField] TextMeshProUGUI wolfMatings;
    [SerializeField] TextMeshProUGUI bearMatings;
    [SerializeField] TextMeshProUGUI mooseMatings;
    private int wolfMatingCount;
    private int bearMatingCount;
    private int mooseMatingCount;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Listen to mating, prey deaths, predator death events to increase
        // count in the simulation
        Mating.OnMating += UpdateMatingCount;
        AnimalBehaviour.OnPreyDeath += UpdatePreyDeaths;
        AnimalBehaviour.OnPredatorDeath += UpdatePredatorDeaths;

        FPS.SetText("FPS: {0}", 0);
        InvokeRepeating("UpdateFPS", 1, 1);

        time = 0f;
        SimTime.SetText("Time: {0}", time);

        Wolves.SetText("|| Wolfs: {0}", 0);
        Bears.SetText("|| Bears: {0}", 0);
        Moose.SetText("|| Moose: {0}", 0);
        Plants.SetText("|| Plants: {0}", 0);
        
        PreyDeaths.SetText("|| Prey Deaths: {0}", 0);
        PredatorDeaths.SetText("|| Predator Deaths: {0}", 0);

        wolfMatings.SetText("|| Wolf Matings: {0}", 0);
        bearMatings.SetText("|| Bear Matings: {0}", 0);
        mooseMatings.SetText("|| Moose Matings: {0}", 0);
    }


    void Update()
    {
        if (GameManager.GetSimulationStatus())
            SimTime.SetText("Time: {0}", (float)Math.Round(time += Time.deltaTime,2));

        UpdateWolves();
        UpdateBears();
        UpdateMoose();
        UpdatePlantsCount();
    }

    public int GetPopCount(Species species)
    {
        return species switch
        {
            Species.wolf => CountWolves(),
            Species.bear => CountBears(),
            Species.moose => CountMoose(),
            _ => 0,
        };
    }

    void UpdateWolves()
    {
        Wolves.SetText("|| Wolfs: {0}", CountWolves());
    }

    void UpdateBears()
    {
        Bears.SetText("|| Bears: {0}", CountBears());
    }
    void UpdateMoose()
    {
        Moose.SetText("|| Moose: {0}", CountMoose());
    }

    void UpdateFPS()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);
        FPS.SetText("FPS: {0}", fps);
    }

    int CountWolves()
    {
        return FindObjectsByType<Wolf>(FindObjectsSortMode.None).Length;
    }

    int CountBears()
    {
        return FindObjectsByType<Bear>(FindObjectsSortMode.None).Length;
    }

    int CountMoose()
    {
        return FindObjectsByType<Moose>(FindObjectsSortMode.None).Length;
    }

    int CountPlants()
    {
        return FindObjectsByType<FoodItem>(FindObjectsSortMode.None).Length + FindObjectsByType<FoodTree>(FindObjectsSortMode.None).Length;
    }

    void UpdatePlantsCount()
    {
        Plants.SetText("|| Plants: {0}", CountPlants());
    }

    void UpdateMatingCount(string species)
    {
        switch (species)
        {
            case "wolf":
                wolfMatingCount++;
                wolfMatings.SetText("|| Wolf Matings: {0}", wolfMatingCount);
                break;
            case "bear":
                bearMatingCount++;
                bearMatings.SetText("|| Bear Matings: {0}", bearMatingCount);
                break;
            case "moose":
                mooseMatingCount++;
                mooseMatings.SetText("|| Moose Matings: {0}", mooseMatingCount);
                break;
        }
    }

    void UpdatePreyDeaths()
    {
        PreyDeathsCount++;
        PreyDeaths.SetText("|| Prey Deaths: {0}", PreyDeathsCount);
    }

    void UpdatePredatorDeaths()
    {
        PredatorDeathsCount++;
        PredatorDeaths.SetText("|| Predator Deaths: {0}", PredatorDeathsCount);
    }
}
