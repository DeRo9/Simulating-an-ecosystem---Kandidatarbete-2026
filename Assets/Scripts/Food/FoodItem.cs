using UnityEngine;

public class FoodItem : MonoBehaviour, IsEdible
{
    [SerializeField]
    public float nutritionValue = 80f;
    [SerializeField]
    private Species[] allowedSpecies = {Species.moose, Species.bear};
    private MushroomSpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<MushroomSpawner>();
    }

    public float Consume()
    {
        if (spawner != null)
        {
            spawner.RemoveMushroom(gameObject);
        }
        Destroy(gameObject);
        return nutritionValue;
    }

    public bool CanBeEatenBy(Species species)
    {
        foreach (Species allowed in allowedSpecies)
        {
            if (allowed == species)
            {
                return true;
            }
        }
        return false;
    }
}
