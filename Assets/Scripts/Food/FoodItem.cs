using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 80f; // The amount of nutrition this food provides
    private MushroomSpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<MushroomSpawner>();
    }

    void OnTriggerEnter(Collider other)
    {
        bool canEatFood = other.CompareTag("Moose") || other.CompareTag("Bear");

        if (canEatFood && !(other is SphereCollider))
        {
            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isHungry)
            {
                needs.Eat(nutritionValue);
                needs.RegenerateHealth(20f);

                if (spawner != null)
                {
                    spawner.RemoveMushroom(gameObject);
                }
                
                Destroy(gameObject);
            }
            
        }
    }

}
