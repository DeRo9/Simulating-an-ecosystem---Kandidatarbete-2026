using UnityEngine;

public class Carcass : MonoBehaviour
{
    public Species species;
    public int maxFeeds = 10;
    public float nutritionPerFeed = 50f;

    public int remainingFeeds;

    void Awake()
    {
        remainingFeeds = maxFeeds;
    }

    public void Initialize(Species species, int maxFeeds, float nutritionPerFeed)
    {
        this.species = species;
        this.maxFeeds = maxFeeds;
        this.nutritionPerFeed = nutritionPerFeed;
        this.remainingFeeds = maxFeeds;
    }

    public float ConsumeOneFeed()
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

    public bool IsEmpty => remainingFeeds <= 0;
}
