using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;


public class StateStatsUI : MonoBehaviour
{
    public TextMeshProUGUI mooseStatesInfo;

    public TextMeshProUGUI wolfStatesInfo;

    public TextMeshProUGUI bearStatesInfo;

    void Update()
    {
        UpdateMooseStatesInfo();
        UpdateWolfStatesInfo();
        UpdateBearStatesInfo();
    }

    private void UpdateMooseStatesInfo()
    {
        var data = SimulationResults.mooseStateAverages.stateAverages;

        if (data.Count == 0)
        {
            mooseStatesInfo.SetText("No Moose in simulation");
            return;
        }

        string stateInfo = "Moose state times:\n";

        foreach (var pair in data)
        {
            stateInfo += $"{pair.Key}: {pair.Value:F2}s\n";
        }

        mooseStatesInfo.SetText(stateInfo);
    }

    private void UpdateWolfStatesInfo()
    {
        var data = SimulationResults.wolfStateAverages.stateAverages;

        if (data.Count == 0)
        {
            wolfStatesInfo.SetText("No Wolves in simulation");
            return;
        }

        string stateInfo = "Wolf state times:\n";

        foreach (var pair in data)
        {
            stateInfo += $"{pair.Key}: {pair.Value:F2}s\n";
        }

        wolfStatesInfo.SetText(stateInfo);
    }

    private void UpdateBearStatesInfo()
    {
        var data = SimulationResults.bearStateAverages.stateAverages;

        if (data.Count == 0)
        {
            bearStatesInfo.SetText("No Bears in simulation");
            return;
        }

        string stateInfo = "Bear state times:\n";

        foreach (var pair in data)
        {
            stateInfo += $"{pair.Key}: {pair.Value:F2}s\n";
        }

        bearStatesInfo.SetText(stateInfo);
    }
}