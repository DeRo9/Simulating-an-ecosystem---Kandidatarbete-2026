using TMPro;
using UnityEngine;

public class StatisticsTableManager : MonoBehaviour
{

    [Header("Wolf Stuff")]
    public int WolfhuntAttemptsCount;
    public int WolfhuntFailuresCount;
    public int WolfSuccessfulHuntsCount;

    [Header("Bear Stuff")]
    public int BearInterferenceCount;
    public int BearhuntAttemptsCount;
    public int BearhuntFailuresCount;
    public int BearSuccessfulHuntsCount;

    [Header("Moose Stuff")]
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

        BearInterferenceCount = 0;
        BearhuntAttemptsCount = 0;
        BearhuntFailuresCount = 0;
        BearSuccessfulHuntsCount = 0;

        MooseSuccessfulEscapeCount = 0;
    }

}
