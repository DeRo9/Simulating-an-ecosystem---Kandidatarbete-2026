using UnityEngine;
using System.Collections.Generic;
using System;

public class AnimalStateAverages
{
    public Dictionary<AnimalBehaviour.State, float> stateAverages = new Dictionary<AnimalBehaviour.State, float>();
}


public static class SimulationResultsCalculator
{
    public static void CalculateStateAverages(Transform parent, AnimalStateAverages result, Species species)
    {
    
        foreach (AnimalBehaviour.State state in System.Enum.GetValues(typeof(AnimalBehaviour.State)))
        {
            if (!result.stateAverages.ContainsKey(state))
                result.stateAverages[state] = 0f;
        }
        
        int totalCount = 0;
        
        for (int i = 0; i < parent.childCount; i++)
        {
            AnimalBehaviour animal = parent.GetChild(i).GetComponent<AnimalBehaviour>();
            if (animal == null || animal.isDead) continue;
            
            foreach (var kvp in animal.stateTrackers)
            {
                result.stateAverages[kvp.Key] += kvp.Value.timeInState;
            }
            totalCount++;
        }
        
        if (SimulationResults.accumulatedStateTimes.ContainsKey(species))
        {
            var deadCount = SimulationResults.totalAnimalsProcessed.ContainsKey(species) 
            ? SimulationResults.totalAnimalsProcessed[species] 
            : 0;
        
            if (deadCount > 0)
            {
                foreach (var kvp in SimulationResults.accumulatedStateTimes[species])
                {
                    result.stateAverages[kvp.Key] += kvp.Value;
                }
                totalCount += deadCount;
            }
        }
        
        if (totalCount > 0)
        {
            var stateKeys = new List<AnimalBehaviour.State>(result.stateAverages.Keys);
            foreach (var state in stateKeys)
            {
                result.stateAverages[state] /= totalCount;
            }
        }
    }

    public static void CalculateNeedsAverages(Transform parent, out float avgHunger, out float avgThirst, out float avgStamina)
    {
        float totalHunger = 0f, totalThirst = 0f, totalStamina = 0f;
        int count = 0;

        for (int i = 0; i < parent.childCount; i++)
        {
            AnimalBehaviour ab = parent.GetChild(i).GetComponent<AnimalBehaviour>();
            if (ab == null || ab.isDead) continue;
            AnimalNeeds needs = parent.GetChild(i).GetComponent<AnimalNeeds>();
            if (needs == null) continue;

            totalHunger += needs.hungerLevel / needs.maxHunger;
            totalThirst += needs.thirstLevel / needs.maxThirst;
            totalStamina += needs.staminaLevel / needs.maxStamina;
            count++;
        }

        avgHunger = count > 0 ? Mathf.Round((totalHunger / count) * 100f) : 0f;
        avgThirst = count > 0 ? Mathf.Round((totalThirst / count) * 100f) : 0f;
        avgStamina = count > 0 ? Mathf.Round((totalStamina / count) * 100f) : 0f;
    }
}

public static class SimulationResults
{

    public static Dictionary<Species, Dictionary<AnimalBehaviour.State, float>> accumulatedStateTimes = new Dictionary<Species, Dictionary<AnimalBehaviour.State, float>>();

    public static Dictionary<Species, int> totalAnimalsProcessed = new Dictionary<Species, int>();
    public static int initialBearsAmount;
    public static int finalBearsAmount;

    public static int initialWolvesAmount;
    public static int finalWolvesAmount;

    public static int initialMooseAmount;
    public static int finalMooseAmount;

    public static List<int> bearsHistory = new List<int>();
    public static List<int> wolvesHistory = new List<int>();
    public static List<int> mooseHistory = new List<int>();

    public static float simulationLength;

    public static AnimalStateAverages mooseStateAverages = new AnimalStateAverages();
    public static AnimalStateAverages wolfStateAverages = new AnimalStateAverages();
    public static AnimalStateAverages bearStateAverages = new AnimalStateAverages();

    // Average needs at end of simulation
    public static float bearAvgHunger, bearAvgThirst, bearAvgStamina;
    public static float wolfAvgHunger, wolfAvgThirst, wolfAvgStamina;
    public static float mooseAvgHunger, mooseAvgThirst, mooseAvgStamina;

    // Needs sampled over time for averaging
    public static List<float> bearHungerSamples = new List<float>();
    public static List<float> bearThirstSamples = new List<float>();
    public static List<float> bearStaminaSamples = new List<float>();
    public static List<float> wolfHungerSamples = new List<float>();
    public static List<float> wolfThirstSamples = new List<float>();
    public static List<float> wolfStaminaSamples = new List<float>();
    public static List<float> mooseHungerSamples = new List<float>();
    public static List<float> mooseThirstSamples = new List<float>();
    public static List<float> mooseStaminaSamples = new List<float>();
}
