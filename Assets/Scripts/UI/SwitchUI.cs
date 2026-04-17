using UnityEngine;

public class SwitchUI : MonoBehaviour
{
    public GameObject PopulationStatsPanel;
    public GameObject ProbabilityStatsPanel;

    public GameObject StatesPanel;

    public void ShowMore()
    {
        PopulationStatsPanel.SetActive(false);
        ProbabilityStatsPanel.SetActive(true);
    }

    public void ShowStates()
    {
        StatesPanel.SetActive(true);
        PopulationStatsPanel.SetActive(false);
    }

    public void GoBack()
    {
        PopulationStatsPanel.SetActive(true);
        ProbabilityStatsPanel.SetActive(false);
        StatesPanel.SetActive(false);
    }
}
