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

    public float minSpeed;
    public float maxSpeed;

    public float minSight;
    public float maxSight;

    public float minHearing;
    public float maxHearing;
    public SphereCollider hearingCollider;


    public float minStrength;
    public float maxStrength;

    public float minHealth;
    public float maxHealth;

    [Header("Forces")]
    public float speed;
    public float runningSpeed;
    public float size = 1f;
    public float strength;
   

    [Header("Hearing")]
    public bool isMoving{get;private set;}
    public float currentSpeed{get; private set;}


    public AnimalNeeds needs;

    protected virtual void Awake()
    {
        IsMale = Random.value > 0.5f;
        needs = GetComponent<AnimalNeeds>();
    }

    protected virtual void Update()
    {
        age += Time.deltaTime * agingSpeed;
        hearingCollider.radius = hearingRange / size;
    }
    
    public virtual void SetMovementState(bool moving, float speed){
        isMoving = moving;
        currentSpeed = speed;
    }

    public virtual float GetHealth()
    {
        return needs.healthLevel;
    }

    float ageModifier = 1f;
    public virtual float CalculateAttackDamage()
    {
        if (!canAttack)
        {
            attackDamage = 0f;
            return attackDamage;
        }

        float sizeModifier = size;

        if (age < grownUpAge)
        {
            ageModifier = 0.8f;
        }
        else if (age > oldAge)
        {
            ageModifier = 0.8f;
        }

        float randomRange = Random.Range(0.8f, 1.2f);

        return attackDamage = strength * sizeModifier * ageModifier * randomRange;
    }

    public virtual float GetMaxHealth()
    {
        if (age < grownUpAge)
        {
            ageModifier = 0.8f;
        }
        else if (age > oldAge)
        {
            ageModifier = 0.8f; 
        }

        float sizeModifier = size;
        return health * ageModifier * sizeModifier;
    }

    public virtual float GetSpeed()
    {
        if (age < grownUpAge)
        {
            ageModifier = 0.8f;
        }
        else if (age > oldAge)
        {
            ageModifier = 0.8f; 
        }

        float sizeModifier = size;
        return speed * ageModifier * sizeModifier;
    }

    
}