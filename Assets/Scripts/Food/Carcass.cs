using UnityEngine;

public class Carcass : MonoBehaviour, IsEdible
{
    public Species species;
    public int maxFeeds = 10;
    public float nutritionPerFeed = 100f;
    public int remainingFeeds;
    public float expireTime = 30f;

    public float expireTimeWinter = 60f;

    [SerializeField] private Species[] allowedConsumers = { Species.wolf, Species.bear };


    void Awake()
    {
        remainingFeeds = maxFeeds;
    }

    void Update()
    {
        if (SeasonManager.Instance.IsSummer)
        {
            expireTime -= Time.deltaTime;
            if (expireTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
        else if (SeasonManager.Instance.IsWinter)
        {
            expireTimeWinter -= Time.deltaTime;
            if (expireTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public float Consume()
    {
        if (remainingFeeds <= 0)
            return 0f;

        remainingFeeds--;
        float nutrition = nutritionPerFeed;

        if (remainingFeeds <= 0)
        {
            Destroy(gameObject);
        }

        return nutrition;
    }

    public bool CanBeEatenBy(Species species)
    {
        return System.Array.Exists(allowedConsumers, element => element == species);
    }

    public void Initialize(Species species, int maxFeeds, float nutritionPerFeed)
    {
        this.species = species;
        this.maxFeeds = maxFeeds;
        this.nutritionPerFeed = nutritionPerFeed;
    }

    public bool IsEmpty => remainingFeeds <= 0;
}
