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

    public static float simulationLength;
}
