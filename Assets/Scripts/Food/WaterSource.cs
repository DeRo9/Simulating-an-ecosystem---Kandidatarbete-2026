

using UnityEngine;


public class WaterSource : MonoBehaviour
{
    [SerializeField]
    
    private float chunkOfWater = 50f;

    void OnTriggerEnter(Collider other)
    {
        
        AnimalNeeds needs = other.GetComponentInParent<AnimalNeeds>();
             
        if (needs != null && needs.isThirsty) //Only drink if thirsty
        {
            needs.drinkFromSource(chunkOfWater);
            
            
        }
            
        
    }

}