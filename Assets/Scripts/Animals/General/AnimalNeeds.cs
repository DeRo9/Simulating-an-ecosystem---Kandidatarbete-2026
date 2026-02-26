

using UnityEngine;

public class AnimalNeeds : MonoBehaviour

{

    [SerializeField] public float maxHunger = 100f; //so InformationUI works...?
    [SerializeField] private float maxThirst = 100f;

    public float hungerLevel; //so InformationUI works...?
    public float thirstLevel;
    
    [SerializeField] private float hungerDecreaseRate = 2f;
    [SerializeField] private float thirstDecreaseRate = 1f;

    public bool isHungry => hungerLevel < maxHunger * 0.8f;

    public bool isThirsty => thirstLevel < maxThirst * 0.5f;

    // 0 is very hungry/thirsty, 1 is full
    public float howHungryInPercent => hungerLevel/maxHunger;
    public float howThirstyInPercent => thirstLevel/maxThirst;


    
    void Start()
    {
        hungerLevel = maxHunger; // Start fully satisfied
        thirstLevel = maxThirst;
    }

    // Update is called once per frame
    void Update()
    {
        // Decrease hunger level over time
        hungerLevel -= hungerDecreaseRate * Time.deltaTime; 
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger); // Ensure it stays within bounds

        // Decrease thirst level over time
        thirstLevel -= thirstDecreaseRate * Time.deltaTime;
        thirstLevel = Mathf.Clamp(thirstLevel, 0f, maxThirst);
        
    }


    public void Eat(float nutritionValue)
    {
        hungerLevel += nutritionValue; // Increase hunger level by the nutrition value of the food
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger); // Ensure it doesn't exceed max
    }

    
    public void drinkFromSource(float chunkOfWater)
    {
        thirstLevel += chunkOfWater;
        thirstLevel = Mathf.Clamp(thirstLevel, 0f, maxThirst);
    }
    





}