using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 100f; // The amount of nutrition this food provides

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Moose"))
        {
            AnimalNeeds mooseNeeds = other.GetComponentInParent<AnimalNeeds>();

            if (mooseNeeds != null && mooseNeeds.isHungry) // Only eat if the moose is hungry
            {
                mooseNeeds.Eat(nutritionValue);
                Destroy(gameObject);
            }
            
        }
    }

}
