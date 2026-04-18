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

    [Header("Births")]
    public int BearBirthCount;
    public int WolfBirthCount;
    public int MooseBirthCount;

    [Header("Deaths")]
    public int BearDeathCount;
    public int WolfDeathCount;
    public int MooseDeathCount;

    [Header("Death by Starvation")]
    public int BearStarvationCount;
    public int WolfStarvationCount;
    public int MooseStarvationCount;

    [Header("Death by Predation")]
    public int BearPredationCount;
    public int WolfPredationCount;
    public int MoosePredationCount;

    [Header("Feeding")]
    public int BearPlantMealsCount;
    public int BearAnimalPreyCount;
    public int MoosePlantMealsCount;
    public int WolfCarcassCount;

    [Header("Pack Behavior")]
    public int PacksFormedCount;
    public int PackHuntAttemptsCount;
    public int PackHuntSuccessCount;


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

    }

    public void ResetStats()
    {
        WolfhuntAttemptsCount = 0;
        WolfhuntFailuresCount = 0;
        WolfSuccessfulHuntsCount = 0;

        BearInterferenceCount = 0;
        BearhuntAttemptsCount = 0;
        BearhuntFailuresCount = 0;
        BearSuccessfulHuntsCount = 0;

        MooseSuccessfulEscapeCount = 0;

        BearBirthCount = 0;
        WolfBirthCount = 0;
        MooseBirthCount = 0;

        BearDeathCount = 0;
        WolfDeathCount = 0;
        MooseDeathCount = 0;

        BearStarvationCount = 0;
        WolfStarvationCount = 0;
        MooseStarvationCount = 0;

        BearPredationCount = 0;
        WolfPredationCount = 0;
        MoosePredationCount = 0;

        BearPlantMealsCount = 0;
        BearAnimalPreyCount = 0;
        MoosePlantMealsCount = 0;
        WolfCarcassCount = 0;

        PacksFormedCount = 0;
        PackHuntAttemptsCount = 0;
        PackHuntSuccessCount = 0;
    }

}
