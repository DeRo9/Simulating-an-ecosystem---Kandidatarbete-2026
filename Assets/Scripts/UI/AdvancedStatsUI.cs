using UnityEngine;
using TMPro;

public class AdvancedStatsUI : MonoBehaviour
{
    [Header("Final Population")]
    public TextMeshProUGUI bearsLabel;
    public TextMeshProUGUI wolvesLabel;
    public TextMeshProUGUI mooseLabel;

    [Header("Births")]
    public TextMeshProUGUI bearBirthsLabel;
    public TextMeshProUGUI wolfBirthsLabel;
    public TextMeshProUGUI mooseBirthsLabel;

    [Header("Deaths")]
    public TextMeshProUGUI bearDeathsLabel;
    public TextMeshProUGUI wolfDeathsLabel;
    public TextMeshProUGUI mooseDeathsLabel;

    [Header("Death by Starvation")]
    public TextMeshProUGUI bearStarvationLabel;
    public TextMeshProUGUI wolfStarvationLabel;
    public TextMeshProUGUI mooseStarvationLabel;

    [Header("Death by Predation")]
    public TextMeshProUGUI bearPredationLabel;
    public TextMeshProUGUI wolfPredationLabel;
    public TextMeshProUGUI moosePredationLabel;

    [Header("Feeding")]
    public TextMeshProUGUI bearPlantMealsLabel;
    public TextMeshProUGUI bearAnimalPreyLabel;
    public TextMeshProUGUI moosePlantMealsLabel;
    public TextMeshProUGUI wolfCarcassLabel;

    void Start()
    {
        int finalBears = SimulationResults.bearsHistory.Count > 0 ? SimulationResults.bearsHistory[SimulationResults.bearsHistory.Count - 1] : 0;
        int finalWolves = SimulationResults.wolvesHistory.Count > 0 ? SimulationResults.wolvesHistory[SimulationResults.wolvesHistory.Count - 1] : 0;
        int finalMoose = SimulationResults.mooseHistory.Count > 0 ? SimulationResults.mooseHistory[SimulationResults.mooseHistory.Count - 1] : 0;

        if (bearsLabel != null) bearsLabel.SetText("{0}", finalBears);
        if (wolvesLabel != null) wolvesLabel.SetText("{0}", finalWolves);
        if (mooseLabel != null) mooseLabel.SetText("{0}", finalMoose);

        if (StatisticsTableManager.instance == null) return;

        if (bearBirthsLabel != null) bearBirthsLabel.SetText("{0}", StatisticsTableManager.instance.BearBirthCount);
        if (wolfBirthsLabel != null) wolfBirthsLabel.SetText("{0}", StatisticsTableManager.instance.WolfBirthCount);
        if (mooseBirthsLabel != null) mooseBirthsLabel.SetText("{0}", StatisticsTableManager.instance.MooseBirthCount);

        if (bearDeathsLabel != null) bearDeathsLabel.SetText("{0}", StatisticsTableManager.instance.BearDeathCount);
        if (wolfDeathsLabel != null) wolfDeathsLabel.SetText("{0}", StatisticsTableManager.instance.WolfDeathCount);
        if (mooseDeathsLabel != null) mooseDeathsLabel.SetText("{0}", StatisticsTableManager.instance.MooseDeathCount);

        if (bearStarvationLabel != null) bearStarvationLabel.SetText("{0}", StatisticsTableManager.instance.BearStarvationCount);
        if (wolfStarvationLabel != null) wolfStarvationLabel.SetText("{0}", StatisticsTableManager.instance.WolfStarvationCount);
        if (mooseStarvationLabel != null) mooseStarvationLabel.SetText("{0}", StatisticsTableManager.instance.MooseStarvationCount);

        if (bearPredationLabel != null) bearPredationLabel.SetText("{0}", StatisticsTableManager.instance.BearPredationCount);
        if (wolfPredationLabel != null) wolfPredationLabel.SetText("{0}", StatisticsTableManager.instance.WolfPredationCount);
        if (moosePredationLabel != null) moosePredationLabel.SetText("{0}", StatisticsTableManager.instance.MoosePredationCount);

        if (bearPlantMealsLabel != null) bearPlantMealsLabel.SetText("{0}", StatisticsTableManager.instance.BearPlantMealsCount);
        if (bearAnimalPreyLabel != null) bearAnimalPreyLabel.SetText("{0}", StatisticsTableManager.instance.BearAnimalPreyCount);
        if (moosePlantMealsLabel != null) moosePlantMealsLabel.SetText("{0}", StatisticsTableManager.instance.MoosePlantMealsCount);
        if (wolfCarcassLabel != null) wolfCarcassLabel.SetText("{0}", StatisticsTableManager.instance.WolfCarcassCount);
    }
}
