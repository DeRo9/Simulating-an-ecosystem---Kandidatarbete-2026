using UnityEngine;

public class MooseNeeds : MonoBehaviour
{

    private float maxHunger = 100f;

    [SerializeField]
    private float hungerLevel;


    public float hungerDecreaseRate = 2f;

    public bool isHungry => hungerLevel < maxHunger * 0.4f;

    void Start()
    {
        hungerLevel = maxHunger; // Start fully satisfied
    }

    // Update is called once per frame
    void Update()
    {
        // Decrease hunger level over time
        hungerLevel -= hungerDecreaseRate * Time.deltaTime; 
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger); // Ensure it stays within bounds
    }

    public void Eat(float nutritionValue)
    {
        hungerLevel += nutritionValue; // Increase hunger level by the nutrition value of the food
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger); // Ensure it doesn't exceed max
    }
}
