using System;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;


public abstract class AnimalBehaviour : MonoBehaviour
{
 
    public enum State
    {
        Idle, // General
        Wander, // General
        Eat, // General, different implementations for each animal
        Drink, // General
        Hunt, // For animals that hunt, wolves and bears
        Fleeing, // For animals that flee (moose)
        Dead,
    }

    [Header("Other")]
    [SerializeField]
    public State CurrentState = State.Idle;

    [SerializeField]
    protected float minTimeWaiting = 2f;

    [SerializeField]
    protected float maxTimeWaiting = 5f;

    [SerializeField]
    protected float waitTime = 0f;


    protected Animal animal;
    protected Rigidbody rb;
    protected Animator anim;
    protected NavMeshAgent agent;
    protected AnimalNeeds needs;
    protected AnimalMemory memory;

    protected GameObject waterTarget;

    public bool isDead;

    [Header("Water Layer")]
    [SerializeField]
    LayerMask waterLayer;

    public static event Action OnPreyDeath;
    public static event Action OnPredatorDeath;
    protected virtual void Start()
    {
        animal = GetComponent<Animal>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        needs = GetComponent<AnimalNeeds>();
        memory = GetComponent<AnimalMemory>();

        if (agent != null && animal != null)
        {
            agent.speed = animal.speed;
        }

    }

    // Checks if the moose has reached its destination
    protected bool hasArrived()
    {
        if(!agent.enabled) return false;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    // Changes the state of the moose and updates behavior accordingly
    protected void ChangeState(State newState)
    {
        CurrentState = newState;

        if (!agent.enabled) // If the animal is dead
            return;

        agent.ResetPath();

        switch (CurrentState)
        {
            case State.Idle:
                waitTime = UnityEngine.Random.Range(minTimeWaiting, maxTimeWaiting); // Random waiting time between min and max
                agent.isStopped = true;
                break;
            case State.Wander:
                agent.isStopped = false;
                agent.SetDestination(GetRandomPoints());
                break;
            case State.Eat:
                EatStateForSpecificAnimal();
                break;
            case State.Drink:
                DrinkState();
                break;
            case State.Hunt:
                HuntState();
                break;
            case State.Fleeing:
                FleeState();
                break;
            case State.Dead:
                // Do nothing
                break;
        }

    }


    public Vector3 GetRandomPoints()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 20f;
        randomDirection.y = 0f;
        Vector3 randomPoint = transform.position + randomDirection;

        NavMeshHit navMeshHit;
        Vector3 finalPosition = transform.position;

        if (NavMesh.SamplePosition(randomPoint, out navMeshHit, 20f, NavMesh.AllAreas))
        {
            finalPosition = navMeshHit.position;
        }

        return finalPosition;
    }

    protected virtual void Update()
    {
        AnimatorStateInfo animatorState = anim.GetCurrentAnimatorStateInfo(0);

        if (isDead)
        {
            return;
        }

        if (animal.GetHealth() <= 0f)
        {
            OnDeath();
            return;
        }


        if (animal != null)
        {
            bool moving = agent.velocity.magnitude > 0.1f;
            animal.SetMovementState (moving,agent.velocity.magnitude);
        }

        if (animatorState.IsName("Running"))
        {
            needs.DrainStamina();
        } else
        {
            needs.RegenerateStamina();
        }


        // State machine logic
        switch (CurrentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Wander:
                UpdateWander();
                break;
            case State.Eat:
                UpdateEat();
                break;
            case State.Drink:
                UpdateDrink();
                break;
            case State.Hunt:
                UpdateHunt();
                break;
            case State.Fleeing:
                UpdateFlee();
                break;
            case State.Dead:
                // Do nothing i guess? 
                break;
        }
    }

    public bool FindWater()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, waterLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestWater = null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Water"))
            {
                Debug.Log("Detected water collider");
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWater = hit.gameObject;
                }

            }

        }

        if(closestWater != null)
        {
            waterTarget = closestWater;
            return true;
        }
        return false;

    }

    protected virtual void DrinkState()
    {
        if (waterTarget != null)
        {
            agent.isStopped = false;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(waterTarget.transform.position, out hit, 20f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

        }
    }

    public virtual void OnDeath() 
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.enabled = false;

        if(animal.species == Species.moose)
        {
            OnPreyDeath?.Invoke();   
        } else if (animal.species == Species.wolf || animal.species == Species.bear)
        {
            OnPredatorDeath?.Invoke();
        }

        gameObject.tag = "carcass";
        gameObject.layer = LayerMask.NameToLayer("carcass"); ;
        anim.SetBool("isWalking", false);
        anim.SetBool("isRunning", false);
        anim.SetTrigger("isDead");

        animal.agingSpeed = 0f;
        Carcass carcass = gameObject.GetComponent<Carcass>();
        if (carcass == null)
        {
            carcass = gameObject.AddComponent<Carcass>();
        }

        if (animal != null && animal.species == Species.moose)
        {
            carcass.Initialize(Species.moose, 10, 10f);
        }
        
        ChangeState(State.Dead);

    }
    protected virtual bool IsHungry() 
    {
        return needs.isHungry; 
    }
    
    protected bool IsThirsty()
    {
        return needs.isThirsty;
    }
    protected virtual void EatStateForSpecificAnimal() { }

    protected virtual void UpdateIdle() { return; }
    protected virtual void UpdateWander() { return; }
    protected virtual void UpdateEat() { return; }

    public void OnFinishedDrinking()
    {
        waterTarget = null;
        agent.isStopped = true;
        ChangeState(State.Idle);
    }

    public void UpdateDrink()
    {
        // If the water target is null, switch back to wandering
        if (waterTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        // If the moose has reached the water, stop moving
        if (hasArrived())
        {
            agent.isStopped = true;


            // No longer thirsty
            if (!needs.isThirsty)
            {
                ChangeState(State.Wander);
                return;
            }
        }

        else
        {
            agent.isStopped = false;
        }
 
    }    
    protected virtual void UpdateHunt() { return; }
    protected virtual void UpdateFlee() { return; }
    protected virtual void HuntState() { return; }
    protected virtual void FleeState() { return; }

}