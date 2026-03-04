

using UnityEngine;


public class WaterSource : MonoBehaviour
{
    [SerializeField]
    
    private float chunkOfWater = 100f;

    void OnTriggerEnter(Collider other)
    {
        if (!(other is SphereCollider)) // Sphere collider is for hearing? It interferes with the animal's body
        {

            Debug.Log("Something entered water: " + other.name);

            AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();

            if (needs != null && needs.isThirsty) //Only drink if thirsty
            {
                needs.drinkFromSource(chunkOfWater);

                MooseBehaviour behaviour = other.GetComponentInParent<MooseBehaviour>();
                if (behaviour != null)
                {
                    behaviour.OnFinishedDrinking();
                }
            }
        }
    }
}