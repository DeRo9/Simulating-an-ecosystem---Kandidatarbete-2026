using TMPro;
using UnityEngine;

public class StatisticsUI : MonoBehaviour
{
    [Header("Wolf Stuff")]
    [SerializeField] TextMeshProUGUI WolfHuntAttempts;
    [SerializeField] TextMeshProUGUI WolfHuntFailures;
    [SerializeField] TextMeshProUGUI WolfSuccessfulHunts;
    [SerializeField] TextMeshProUGUI WolfHuntSuccessRate;

    [Header("Bear Stuff")]
    [SerializeField] TextMeshProUGUI BearInterference;
    [SerializeField] TextMeshProUGUI BearHuntAttempts;
    [SerializeField] TextMeshProUGUI BearHuntFailures;
    [SerializeField] TextMeshProUGUI BearSuccesfulHunts;
    [SerializeField] TextMeshProUGUI BearHuntSuccessRate;

    [Header("Moose Stuff")]
    [SerializeField] TextMeshProUGUI MooseSuccessfulEscape;


    void Update()
    {
        if (StatisticsTableManager.instance == null) return;

        UpdateWolfInfo();
        UpdateBearInfo();
        UpdateMooseInfo();
    }

    void UpdateWolfInfo()
    {
        WolfHuntAttempts.SetText("{0}", StatisticsTableManager.instance.WolfhuntAttemptsCount);
        WolfHuntFailures.SetText("{0}", StatisticsTableManager.instance.WolfhuntFailuresCount);
        WolfSuccessfulHunts.SetText("{0}", StatisticsTableManager.instance.WolfSuccessfulHuntsCount);

        float WolfSuccessRate = StatisticsTableManager.instance.WolfhuntAttemptsCount > 0
                              ? Mathf.Round(((float)StatisticsTableManager.instance.WolfSuccessfulHuntsCount / StatisticsTableManager.instance.WolfhuntAttemptsCount) * 100f)
                              : 0f;
        WolfHuntSuccessRate.SetText("{0}%", WolfSuccessRate);
    }

    void UpdateBearInfo()
    {
        BearInterference.SetText("{0}", StatisticsTableManager.instance.BearInterferenceCount);
        BearHuntAttempts.SetText("{0}", StatisticsTableManager.instance.BearhuntAttemptsCount);
        BearHuntFailures.SetText("{0}", StatisticsTableManager.instance.BearhuntFailuresCount);
        BearSuccesfulHunts.SetText("{0}", StatisticsTableManager.instance.BearSuccessfulHuntsCount);

        float BearSuccessRate = StatisticsTableManager.instance.BearhuntAttemptsCount > 0
                      ? Mathf.Round(((float)StatisticsTableManager.instance.BearSuccessfulHuntsCount / StatisticsTableManager.instance.BearhuntAttemptsCount) * 100f)
                      : 0f;

        BearHuntSuccessRate.SetText("{0}%", BearSuccessRate);
    }

    void UpdateMooseInfo()
    {
        MooseSuccessfulEscape.SetText("{0}", StatisticsTableManager.instance.MooseSuccessfulEscapeCount);
    }
}
