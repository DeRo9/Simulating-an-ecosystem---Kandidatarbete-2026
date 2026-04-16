using UnityEngine;

public class Carcass : MonoBehaviour
{
    public Species species;
    public int maxFeeds = 10;
    public float nutritionPerFeed = 100f;

    public int remainingFeeds;

    public float expireTime = 30f;

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
    }

    void OnTriggerEnter(Collider other)
    {
        bool canEatCarcass = other.CompareTag("Wolf") || other.CompareTag("Bear");

        if (canEatCarcass && !(other is SphereCollider))
        {
            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isHungry)
            {
                float nutrition = ConsumeOneFeed();
                if (nutrition > 0f)
                {
                    needs.Eat(nutrition);
                }
            }
        }
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
