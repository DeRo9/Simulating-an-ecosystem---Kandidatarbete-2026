using System.Drawing;
using UnityEngine;

public enum Species
{
    wolf,
    bear,
    moose
}

public class Animal : MonoBehaviour
{
    [Header("Species")]
    public Species species;

    [Header("Combat")]
    public bool canAttack = true;
    public float attackDamage;  

    [Header("Aging")]
    public float age = 0f;
    public float agingSpeed = 0.1f;
    public float startingMaxAge = 15f;

    [Header("Life Stages")]
    public float grownUpAge = 10f;
    public float oldAge = 20f;

    [Header("Sex")]
    public bool IsMale;

    [Header("Senses")]
    public float sightRange;
    public float hearingRange;

    public float baseSpeed;
    public float baseStrength;
    public float baseHealth;
    public float baseSight;
    public float baseHearing;
    public SphereCollider hearingCollider;


    [Header("Forces")]
    public float speed;
    public float runningSpeed;
    public float size = 1f;
    public float strength;
    public float health;

    public float staminaDecreaseRate;
   

    [Header("Hearing")]
    public bool isMoving{get;private set;}
    public float currentSpeed{get; private set;}


    public AnimalNeeds needs;

    protected virtual void Awake()
    {
        IsMale = Random.value > 0.5f;
        needs = GetComponent<AnimalNeeds>();
        needs.staminaDecreaseRate = staminaDecreaseRate;
        if (!IsMale)
        {
            size *= 0.8f;
            baseStrength *= 0.8f;
            baseHealth *= 0.8f;
            baseSpeed *= 0.9f;
        }
    }

    float attributeUpdateTimer;

    protected virtual void Update()
    {
        age += Time.deltaTime * agingSpeed;
        attributeUpdateTimer += Time.deltaTime;

        if(attributeUpdateTimer >= 3f)
        {
            UpdateAttributes();

            if(hearingCollider != null && size > 0f && !float.IsNaN(hearingRange))
                hearingCollider.radius = hearingRange / size;

            attributeUpdateTimer = 0f;
        }
    }

    public void InitializeAttributes()
    {
        baseHealth *= getVariation();
        baseStrength *= getVariation();
        baseSpeed *= getVariation();
        hearingRange = baseHearing * getVariation();
        sightRange = baseSight * getVariation();
        needs.staminaDecreaseRate = staminaDecreaseRate;

        UpdateAttributes();

        needs.healthLevel = health;
        needs.maxHealth = health;
    }

    public float getVariation()
    {
        return Random.Range(0.85f, 1.15f);
    }

    public virtual void UpdateAttributes()
    {
        float ageModifier = GetAgeModifier();

        strength = baseStrength * ageModifier * size;
        health = baseHealth * ageModifier * size;
        speed = baseSpeed * ageModifier * size;
        runningSpeed = speed * 1.75f;
        needs.staminaDecreaseRate = staminaDecreaseRate * size; //smaller animal does net get as tired

        float healthRatio = needs.healthLevel / needs.maxHealth;
        needs.maxHealth = health;
        needs.healthLevel = health * healthRatio;
    }

    protected float GetAgeModifier()
    {
        if (age < grownUpAge)
        {
            return Mathf.Lerp(0.5f, 1f, age / grownUpAge);
        }
        else if (age > oldAge)
        {
            return 0.8f; 
        }
        return 1f;
    }

    public virtual void SetMovementState(bool moving, float speed){
        isMoving = moving;
        currentSpeed = speed;
    }

    public virtual float GetHealth()
    {
        return needs.healthLevel;
    }

    public virtual float CalculateAttackDamage()
    {
        if (!canAttack)
        {
            attackDamage = 0f;
            return attackDamage;
        }

        return attackDamage = strength;
    }
}