using UnityEngine;
using System.Collections.Generic;
using System;

public class AnimalStateAverages
{
    public Dictionary<string, float> stateAverages = new Dictionary<string, float>();
}

public static class SimulationResultsCalculator
{
    public static void CalculateStateAverages(Transform parent, AnimalStateAverages result)
    {
        Dictionary<AnimalBehaviour.State, float> totals = new Dictionary<AnimalBehaviour.State, float>();

        foreach (AnimalBehaviour.State state in Enum.GetValues(typeof(AnimalBehaviour.State)))
        {
            totals[state] = 0f;
        }

        int count = 0;

        for (int i = 0; i < parent.childCount; i++)
        {
            AnimalBehaviour animal = parent.GetChild(i).GetComponent<AnimalBehaviour>();
            if (animal == null) continue;

            count += 1;

            var states = new List<AnimalBehaviour.State>(totals.Keys);

            foreach (var state in states)
            {
                totals[state] += animal.GetTotalStateTime(state);
            }
        }

        if (count == 0) return;

        foreach (var pair in totals)
        {
            float avg = pair.Value / count;
            result.stateAverages[pair.Key.ToString()] = avg;
        }
    }
}

public static class SimulationResults
{
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
}
