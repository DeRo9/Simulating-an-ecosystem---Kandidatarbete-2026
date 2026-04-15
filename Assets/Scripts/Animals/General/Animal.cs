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

    // Speed
    public float minSpeed;
    public float maxSpeed;

    // Sight
    public float minSight;
    public float maxSight;

    // Hearing
    public float minHearing;
    public float maxHearing;
    public SphereCollider hearingCollider;

    // Strength
    public float minStrength;
    public float maxStrength;

    // Health
    public float minHealth;
    public float maxHealth;

    [Header("Forces")]
    public float speed;
    public float runningSpeed; //= animal.speed * 2f 
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

        hearingCollider.radius = hearingRange / size; // Wolf has scale of 2 , so hearing range is divided by 2 to keep it consistent with other animals


    }
    
    public virtual void SetMovementState(bool moving, float speed){
        isMoving = moving;
        currentSpeed = speed;
    }

    public virtual float GetHealth()
    {
        return needs.healthLevel;
    }

    public virtual void CalculateAttackDamage()
    {
        if (!canAttack)
        {
            attackDamage = 0f;
            return;
        }

        attackDamage = strength * Random.Range(1f, 1.2f);
        // add size in equation... somehow also affect
    }

    public virtual float CalculateHealth(float minHealth, float maxHealth)
    {
        return Random.Range(minHealth, maxHealth);
        
        
    }




}
