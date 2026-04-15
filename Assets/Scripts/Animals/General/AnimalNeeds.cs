

using System;
using UnityEngine;

public class AnimalNeeds : MonoBehaviour

{

    public float maxHunger = 100f; //so InformationUI works...?
    public float maxThirst = 100f;
    public float maxHealth = 100f;
    public float maxStamina = 100f;

    public float hungerLevel; //so InformationUI works...?
    public float thirstLevel;
    public float healthLevel;
    public float staminaLevel;
    
    [SerializeField] private float hungerDecreaseRate = 2f;
    [SerializeField] private float thirstDecreaseRate = 1f;
    [SerializeField] public float staminaDecreaseRate = 1f; // Made public so it can be modified by the animals classes
    [SerializeField] private float staminaIncreaseRate = 1.5f;

    public float hibernationMultiplier = 1f;

    public bool isHungry => hungerLevel < maxHunger * 0.8f;
    public bool isHungryBearH => hungerLevel < maxHunger * 0.5f; 
    public bool isThirsty => thirstLevel < maxThirst * 0.5f;
    public bool isTired => staminaLevel < maxStamina * 0.5f;
    public bool noMoreStamina { get; private set; }
    public bool isDead => healthLevel <= 0f;

    // 0 is very hungry/thirsty, 1 is full
    public float howHungryInPercent => hungerLevel/maxHunger;
    public float howThirstyInPercent => thirstLevel/maxThirst;
    private bool IsStarving => howHungryInPercent <= 0f;
    private bool IsDehydrated => howThirstyInPercent <= 0f;


    
    void Start()
    {
        hungerLevel = maxHunger; // Start fully satisfied
        thirstLevel = maxThirst; // Start fully hydrated
        healthLevel = maxHealth; // Start at full health
        staminaLevel = maxStamina; // Start at full stamina
        healthLevel = maxHealth; // Set health
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return; 

        float hungerMultiplier = 1f;
        if (SeasonManager.Instance.IsWinter)
        {
            hungerMultiplier = 1.5f;
        }

        hungerLevel -= hungerDecreaseRate * Time.deltaTime * hungerMultiplier * hibernationMultiplier;
        hungerLevel = Mathf.Clamp(hungerLevel, 0f, maxHunger);


        float thirstMultiplier = 1f;

        if (SeasonManager.Instance.IsSummer)
        {
            thirstMultiplier = 1.5f;
        }
        if (SeasonManager.Instance.IsRaining)
        {
            thirstMultiplier = 0.5f;
        }

        survivalDamage();

        if(staminaLevel <= 0.1f)
        {
            noMoreStamina = true;
        }

        if(noMoreStamina && !isTired)
        {
            noMoreStamina = false;
        }

        thirstLevel -= thirstDecreaseRate * Time.deltaTime * thirstMultiplier * hibernationMultiplier;
        thirstLevel = Mathf.Clamp(thirstLevel, 0f, maxThirst);


    }

    private void survivalDamage()
    {
        if(IsStarving || IsDehydrated)
        {
            healthLevel -= 0.5f * Time.deltaTime;
            healthLevel = Mathf.Clamp(healthLevel, 0f, maxHealth);

        }

        if ((IsStarving || IsDehydrated) && SeasonManager.Instance.IsWinter)
        {
            healthLevel -= 1f * Time.deltaTime;
            healthLevel = Mathf.Clamp(healthLevel, 0f, maxHealth);
        }
        
        if(IsStarving && IsDehydrated)
        {
            healthLevel -= Time.deltaTime;
            healthLevel = Mathf.Clamp(healthLevel, 0f, maxHealth);
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

        float staminaMultiplier = 1f;
        if (SeasonManager.Instance.IsSnowing)
        {
            staminaMultiplier = 1.5f;
        }
        
        staminaLevel -= staminaDecreaseRate * Time.deltaTime * staminaMultiplier;
        staminaLevel = Mathf.Clamp(staminaLevel, 0f, maxStamina);
    }

    public void RegenerateStamina()
    {
        staminaLevel += staminaIncreaseRate * Time.deltaTime;
        staminaLevel = Mathf.Clamp(staminaLevel, 0f, maxStamina);
    }

    // Called after eating or drinking to regenerate some health
    public void RegenerateHealth(float amount)
    {
        healthLevel += amount;
        healthLevel = Mathf.Clamp(healthLevel, 0f, maxHealth);
    }
    


}