using UnityEngine;

public class FoodTree : MonoBehaviour
{
    [SerializeField]
    private float nutritionValue = 100f; // The amount of nutrition this food provides

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
                AnimalBehaviour behaviour = other.GetComponentInParent<AnimalBehaviour>();
                if (behaviour != null)
                {
                    behaviour.ForceStopSearchFood();
                }

                if (StatisticsTableManager.instance != null)
                {
                    if (other.CompareTag("Bear")) StatisticsTableManager.instance.BearPlantMealsCount++;
                    else if (other.CompareTag("Moose")) StatisticsTableManager.instance.MoosePlantMealsCount++;
                }

            }

        }
    }

}
