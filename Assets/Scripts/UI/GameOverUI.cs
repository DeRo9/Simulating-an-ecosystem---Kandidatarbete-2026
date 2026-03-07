using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverUI : MonoBehaviour
{

    public PopulationGraph bearsGraph;
    public PopulationGraph wolvesGraph;
    public PopulationGraph mooseGraph;

    void Start()
    {
        bearsGraph.UpdateGraph(SimulationResults.initialBearsAmount, SimulationResults.finalBearsAmount);
        wolvesGraph.UpdateGraph(SimulationResults.initialWolvesAmount, SimulationResults.finalWolvesAmount);
        mooseGraph.UpdateGraph(SimulationResults.initialMooseAmount, SimulationResults.finalMooseAmount);
    }

    public void RestartSimulation()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TerrainSmall");
    }
}
