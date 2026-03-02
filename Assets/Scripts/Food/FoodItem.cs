using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 100f; // The amount of nutrition this food provides
    private MushroomSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<MushroomSpawner>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Moose")) // Add || "Bear" too?
        {
            AnimalNeeds mooseNeeds = other.GetComponentInParent<AnimalNeeds>();

            if (mooseNeeds != null && mooseNeeds.isHungry) // Only eat if the moose is hungry
            {
                mooseNeeds.Eat(nutritionValue);

                if (spawner != null)
                {
                    spawner.DecreaseMushroomCount();
                }
                
                Destroy(gameObject);
            }
            
        }
    }

}
