using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;


public class GameOverUI : MonoBehaviour
{

    public PopulationGraph bearsGraph;
    public PopulationGraph wolvesGraph;
    public PopulationGraph mooseGraph;

    [Header("Average Needs Summary")]
    public TextMeshProUGUI bearsNeedsText;
    public TextMeshProUGUI wolvesNeedsText;
    public TextMeshProUGUI mooseNeedsText;

    void Start()
    {
        bearsGraph.DrawGraph(SimulationResults.bearsHistory, SimulationResults.simulationLength);
        wolvesGraph.DrawGraph(SimulationResults.wolvesHistory, SimulationResults.simulationLength);
        mooseGraph.DrawGraph(SimulationResults.mooseHistory, SimulationResults.simulationLength);

        SetNeedsSummary(
            bearsNeedsText,
            "Bears",
            SimulationResults.bearsAvgHungerHistory,
            SimulationResults.bearsAvgThirstHistory,
            SimulationResults.bearsAvgStaminaHistory
        );

        SetNeedsSummary(
            wolvesNeedsText,
            "Wolves",
            SimulationResults.wolvesAvgHungerHistory,
            SimulationResults.wolvesAvgThirstHistory,
            SimulationResults.wolvesAvgStaminaHistory
        );

        SetNeedsSummary(
            mooseNeedsText,
            "Moose",
            SimulationResults.mooseAvgHungerHistory,
            SimulationResults.mooseAvgThirstHistory,
            SimulationResults.mooseAvgStaminaHistory
        );
    }

    void SetNeedsSummary(
        TextMeshProUGUI label,
        string speciesName,
        List<float> hungerHistory,
        List<float> thirstHistory,
        List<float> staminaHistory
    )
    {
        if (label == null)
            return;

        float hunger = GetLastValue(hungerHistory) * 100f;
        float thirst = GetLastValue(thirstHistory) * 100f;
        float stamina = GetLastValue(staminaHistory) * 100f;

        label.text = string.Format(
            "{0} Avg Needs (end): H {1:0}% | T {2:0}% | S {3:0}%",
            speciesName,
            hunger,
            thirst,
            stamina
        );
    }

    float GetLastValue(List<float> values)
    {
        if (values == null || values.Count == 0)
            return 0f;

        return values[values.Count - 1];
    }

    public void RestartSimulation()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TerrainSmall");
    }
}
