using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 100f; // The amount of nutrition this food provides
    private MushroomSpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<MushroomSpawner>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Moose") && !(other is SphereCollider)) // Add || "Bear" too?
        {
            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isHungry) // Only eat if the moose is hungry
            {
                needs.Eat(nutritionValue);

                if (spawner != null)
                {
                    spawner.DecreaseMushroomCount();
                }
                
                Destroy(gameObject);
            }
            
        }
    }

}
