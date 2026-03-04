

using UnityEngine;


public class WaterSource : MonoBehaviour
{
    [SerializeField]
    
    private float chunkOfWater = 100f;

    void OnTriggerStay(Collider other)
    {
        if (!(other is SphereCollider))
        {

            Debug.Log("Something entered water: " + other.name);

            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isThirsty) //Only drink if thirsty
            {
                needs.drinkFromSource(chunkOfWater);

                if (!needs.isThirsty)
                {
                    AnimalBehaviour behaviour = other.GetComponentInParent<AnimalBehaviour>();
                    if (behaviour != null)
                    {
                        behaviour.OnFinishedDrinking();
                    }
                }
            }
        }
    }
}