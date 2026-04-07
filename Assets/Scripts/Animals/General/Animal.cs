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

    /*
    [Header("Senses")]
    public float sightRange = 40f;
    public float hearingRange = 10f;

    [Header("Stat Ranges")]
    */

    // Speed
    public float minSpeed;
    public float maxSpeed;

    // Sight
    public float minSight;
    public float maxSight;

    // Hearing
    public float minHearing;
    public float maxHearing;

    // Strength
    public float minStrength;
    public float maxStrength;

    //public float attackDamage;
    

    /*
    [Header("Forces")]
    public float speed = 2f;
    public float runningSpeed = 4f;
    //public float size = 1f; //I guess this well be equivalent to hp in the future... right now scale
    public float strength = 1f;
    public float attackDamage = 20f;
    */

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

        //UpdateSize(); //??
    }

    /*
    
    public virtual void GetStats(out float speed, out float size, out float sight, out float hearing)
    {
        speed = this.speed;
        size = this.size;
        sight = this.sightRange;
        hearing = this.hearingRange;
    }
    */
    
    public virtual void GetStats(out float speed, out float sight, out float hearing)
    {
    speed = this.speed;
    sight = this.sightRange;
    hearing = this.hearingRange;
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

        attackDamage = strength * UnityEngine.Random.Range(1f, 2f);
    }




}
