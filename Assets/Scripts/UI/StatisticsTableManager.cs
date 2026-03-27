using TMPro;
using UnityEngine;

public class StatisticsTableManager : MonoBehaviour
{
    [Header("Wolf Stuff")]
    [SerializeField] TextMeshProUGUI WolfHuntAttempts;
    [SerializeField] TextMeshProUGUI WolfHuntFailures;
    [SerializeField] TextMeshProUGUI WolfSuccessfulHunts;
    [SerializeField] TextMeshProUGUI WolfHuntSuccessRate;

    public int WolfhuntAttemptsCount;
    public int WolfhuntFailuresCount;
    public int WolfSuccessfulHuntsCount;
    float WolfHuntSuccessRatesPercentage;

    [Header("Bear Stuff")]
    [SerializeField] TextMeshProUGUI BearInterference;
    [SerializeField] TextMeshProUGUI BearHuntAttempts;
    [SerializeField] TextMeshProUGUI BearHuntFailures;
    [SerializeField] TextMeshProUGUI BearSuccesfulHunts;
    [SerializeField] TextMeshProUGUI BearHuntSuccessRate;

    public int BearInterferenceCount;
    public int BearhuntAttemptsCount;
    public int BearhuntFailuresCount;
    public int BearSuccessfulHuntsCount;
    float BearHuntSuccessRates;

    [Header("Moose Stuff")]
    [SerializeField] TextMeshProUGUI MooseSuccessfulEscape;

    public int MooseSuccessfulEscapeCount;


    public static StatisticsTableManager instance { get; private set; } // Persists across scene so that animals can report information.

    void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        WolfhuntAttemptsCount = 0;
        WolfhuntFailuresCount = 0;
        WolfSuccessfulHuntsCount = 0;
        WolfHuntSuccessRatesPercentage = 0f;

        BearInterferenceCount = 0;
        BearhuntAttemptsCount = 0;
        BearhuntFailuresCount = 0;
        BearSuccessfulHuntsCount = 0;
        BearHuntSuccessRates = 0f;

        MooseSuccessfulEscapeCount = 0;
    }

    void Update()
    {
        if (GameManager.GetSimulationStatus())
        {
            UpdateWolfInfo();
            UpdateBearInfo();
            UpdateMooseInfo();
        }
    }

    void UpdateWolfInfo()
    {
        WolfHuntAttempts.SetText("{0}", WolfhuntAttemptsCount);
        WolfHuntFailures.SetText("{0}", WolfhuntFailuresCount);
        WolfSuccessfulHunts.SetText("{0}", WolfSuccessfulHuntsCount);

        WolfHuntSuccessRatesPercentage = Mathf.Round(WolfhuntFailuresCount / WolfSuccessfulHuntsCount);
        WolfHuntSuccessRate.SetText("{0}%", WolfHuntSuccessRatesPercentage);
    }

    void UpdateBearInfo()
    {
        BearInterference.SetText("{0}", BearInterferenceCount);
        BearHuntAttempts.SetText("{0}", BearhuntAttemptsCount);
        BearHuntFailures.SetText("{0}", BearhuntFailuresCount);
        BearSuccesfulHunts.SetText("{0}", BearSuccessfulHuntsCount);

        BearHuntSuccessRates = Mathf.Round(BearhuntFailuresCount / BearSuccessfulHuntsCount);
        BearHuntSuccessRate.SetText("{0}%", BearHuntSuccessRates);
    }

    void UpdateMooseInfo()
    {
        MooseSuccessfulEscape.SetText("{0}", MooseSuccessfulEscapeCount);
    }
}
