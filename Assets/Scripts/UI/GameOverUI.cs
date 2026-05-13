using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverUI : MonoBehaviour
{

    public PopulationGraph bearsGraph;
    public PopulationGraph wolvesGraph;
    public PopulationGraph mooseGraph;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        bearsGraph.DrawGraph(SimulationResults.bearsHistory, SimulationResults.simulationLength);
        wolvesGraph.DrawGraph(SimulationResults.wolvesHistory, SimulationResults.simulationLength);
        mooseGraph.DrawGraph(SimulationResults.mooseHistory, SimulationResults.simulationLength);
    }

    public void RestartSimulation()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TerrainMedium");
    }
}
