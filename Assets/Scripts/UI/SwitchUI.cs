using UnityEngine;

public class SwitchUI : MonoBehaviour
{
    public GameObject PopulationStatsPanel;
    public GameObject ProbabilityStatsPanel;

    public void ShowMore()
    {
        PopulationStatsPanel.SetActive(false);
        ProbabilityStatsPanel.SetActive(true);
    }

    public void GoBack()
    {
        PopulationStatsPanel.SetActive(true);
        ProbabilityStatsPanel.SetActive(false);
    }
}
