using UnityEngine;

public class FoodTree : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 100f; // The amount of nutrition this food provides

    void OnTriggerEnter(Collider other)
    {
        bool canEatFood = other.CompareTag("Moose");

        if (canEatFood && !(other is SphereCollider))
        {
            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isHungry)
            {
                needs.Eat(nutritionValue);
                
            }
            
        }
    }

}
