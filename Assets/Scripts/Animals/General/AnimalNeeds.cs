

using System;
using UnityEngine;

public class AnimalNeeds : MonoBehaviour

{

    [SerializeField] public float maxHunger = 100f; //so InformationUI works...?
    [SerializeField] public float maxThirst = 100f;
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float maxStamina = 100f;

    public float hungerLevel; //so InformationUI works...?
    public float thirstLevel;
    public float healthLevel;
    public float staminaLevel;
    
    [SerializeField] private float hungerDecreaseRate = 2f;

    [SerializeField] private float thirstDecreaseRate = 1f;

    [SerializeField] private float staminaDecreaseRate = 1f;
    [SerializeField] private float staminaIncreaseRate = 1.5f;

    public bool isHungry => hungerLevel < maxHunger * 0.8f;

    public bool isHungryBearH => hungerLevel < maxHunger * 0.5f;
    
    public bool isThirsty => thirstLevel < maxThirst * 0.5f;

    public bool isTired => staminaLevel < maxStamina * 0.5f;
    public bool noMoreStamina { get; private set; }

    public bool isDead => healthLevel <= 0f;

    // 0 is very hungry/thirsty, 1 is full
    public float howHungryInPercent => hungerLevel/maxHunger;
    public float howThirstyInPercent => thirstLevel/maxThirst;



    
    void Start()
    {
        hungerLevel = maxHunger; // Start fully satisfied
        thirstLevel = maxThirst; // Start fully hydrated
        healthLevel = maxHealth; // Start at full health
        staminaLevel = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return; // If dead, then we don't need to update anything anymore

        // Decrease hunger level over time
        hungerLevel -= hungerDecreaseRate * Time.deltaTime; 
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger); // Ensure it stays within bounds

        // Decrease thirst level over time
        thirstLevel -= thirstDecreaseRate * Time.deltaTime;
        thirstLevel = Mathf.Clamp(thirstLevel, 0f, maxThirst); // Ensure it stays within bounds

        if(staminaLevel <= 0)
        {
            noMoreStamina = true;
        }

        if(noMoreStamina && !isTired)
        {
            noMoreStamina = false;
        }
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
    
    public void TakeDamage(float damage)
    {
        healthLevel -= damage;
        healthLevel = Mathf.Clamp(healthLevel, 0f, maxHealth); // Ensure it doesn't go below 0
    }

    public void DrainStamina()
    {
        staminaLevel -= staminaDecreaseRate * Time.deltaTime;
        staminaLevel = Mathf.Clamp(staminaLevel, 0f, maxStamina);
    }

    public void RegenerateStamina()
    {
        staminaLevel += staminaIncreaseRate * Time.deltaTime;
        staminaLevel = Mathf.Clamp(staminaLevel, 0f, maxStamina);
    }
    


}