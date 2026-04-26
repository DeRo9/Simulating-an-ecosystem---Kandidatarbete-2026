

using UnityEngine;


public class WaterSource : MonoBehaviour
{
    [SerializeField]
    
    private float waterAmount = 100f;

    public float Drink()
    {
        return waterAmount;
    }
}