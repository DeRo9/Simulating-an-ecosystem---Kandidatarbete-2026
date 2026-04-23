using UnityEngine;

public class FoodTree : MonoBehaviour, IsEdible
{
    [SerializeField]
    public float nutritionValue = 100f;
    [SerializeField]
    private Species[] allowedSpecies = {Species.moose};
    private MushroomSpawner spawner;

    public float Consume()
    {
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
