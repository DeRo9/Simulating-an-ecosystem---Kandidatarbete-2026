using TMPro;
using UnityEngine;

public class StatisticsTableManager : MonoBehaviour
{
    [Header("Wolf Stuff")]
    [SerializeField] TextMeshProUGUI WolfHuntAttempts;
    [SerializeField] TextMeshProUGUI WolfHuntFailures;
    [SerializeField] TextMeshProUGUI WolfSuccesfulHunts;
    [SerializeField] TextMeshProUGUI WolfHuntSuccessRate;

    int huntAttempts;
    int huntFailures;
    int SuccessfulHunts;
    int HuntSuccessRate;

    [Header("Bear Stuff")]
    [SerializeField] TextMeshProUGUI BearInterference;

    int BearInterferenceCount;

    [Header("Moose Stuff")]
    [SerializeField] TextMeshProUGUI MooseSuccessfulEscape;

    int MooseSuccessfulEscapeCount;

    void Start()
    {
        huntAttempts = 0;
        huntFailures = 0;
        SuccessfulHunts = 0;
        HuntSuccessRate = 0;

        BearInterferenceCount = 0;

        MooseSuccessfulEscapeCount = 0;
    }

    void Update()
    {
        if(GameManager.GetSimulationStatus())
            UpdateWolfInfo();
            UpdateBearInfo();
            UpdateMooseInfo();
    }

    void UpdateWolfInfo()
    {
        WolfHuntAttempts.SetText("{0}", huntAttempts);
        WolfHuntFailures.SetText("{0}", huntFailures);
        WolfSuccesfulHunts.SetText("{0}", SuccessfulHunts);
        WolfHuntSuccessRate.SetText("{0}", HuntSuccessRate);
    }

    void UpdateBearInfo()
    {
        BearInterference.SetText("{0}", BearInterferenceCount);
    }

    void UpdateMooseInfo()
    {
        MooseSuccessfulEscape.SetText("{0}", MooseSuccessfulEscapeCount);
    }
}
