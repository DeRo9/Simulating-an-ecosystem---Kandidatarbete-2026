using UnityEngine;
using System.Collections.Generic;


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

    public static List<float> bearsAvgHungerHistory = new List<float>();
    public static List<float> bearsAvgThirstHistory = new List<float>();
    public static List<float> bearsAvgStaminaHistory = new List<float>();

    public static List<float> wolvesAvgHungerHistory = new List<float>();
    public static List<float> wolvesAvgThirstHistory = new List<float>();
    public static List<float> wolvesAvgStaminaHistory = new List<float>();

    public static List<float> mooseAvgHungerHistory = new List<float>();
    public static List<float> mooseAvgThirstHistory = new List<float>();
    public static List<float> mooseAvgStaminaHistory = new List<float>();

    public static float simulationLength;

    public static void Reset()
    {
        initialBearsAmount = 0;
        finalBearsAmount = 0;
        initialWolvesAmount = 0;
        finalWolvesAmount = 0;
        initialMooseAmount = 0;
        finalMooseAmount = 0;

        bearsHistory.Clear();
        wolvesHistory.Clear();
        mooseHistory.Clear();

        bearsAvgHungerHistory.Clear();
        bearsAvgThirstHistory.Clear();
        bearsAvgStaminaHistory.Clear();

        wolvesAvgHungerHistory.Clear();
        wolvesAvgThirstHistory.Clear();
        wolvesAvgStaminaHistory.Clear();

        mooseAvgHungerHistory.Clear();
        mooseAvgThirstHistory.Clear();
        mooseAvgStaminaHistory.Clear();

        simulationLength = 0f;
    }
}
