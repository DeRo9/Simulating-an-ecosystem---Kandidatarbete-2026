using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;


// This is an abstract class that defines the general behaviour of an animal. More specific implementations will inherit this class and build upon it.
public abstract class AnimalBehaviour : MonoBehaviour
{
    // The home area of the animal
    //public Area HomeArea;

    // Internal states of the animal
    protected enum State
    {
        Idle, // General
        Wander, // General
        Eat, // General, different implementations for each animal
        Drink, // General
        Hunt, // For animals that hunt, wolves and bears
        Fleeing, // For animals that flee (moose)
        Dead,
    }

    // Current state of the animal
    [SerializeField]
    protected State CurrentState = State.Idle;

    // Waiting timers
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


    protected virtual void Start()
    {
        animal = GetComponent<Animal>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        needs = GetComponent<AnimalNeeds>();

        if (agent != null && animal != null)
        {
            agent.speed = animal.speed;
        }

    }

    // Checks if the moose has reached its destination
    protected bool hasArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    // Changes the state of the moose and updates behavior accordingly
    protected void ChangeState(State newState)
    {
        CurrentState = newState;
        switch (CurrentState)
        {
            case State.Idle:
                waitTime = Random.Range(minTimeWaiting, maxTimeWaiting); // Random waiting time between min and max
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
                DrinkStateForSpecificAnimal();
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
        Vector3 randomDirection = Random.insideUnitSphere * 20f;
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
        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f ); // "isWalking" Ã¤r en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f); // "isRunning" Ã¤r en bool i animator
        if (animal != null)
        {
            bool moving = agent.velocity.magnitude > 0.1f;
            animal.SetMovementState (moving,agent.velocity.magnitude);
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

    public virtual void OnDeath() { return; }
    protected virtual bool IsHungry() { return false; }
    protected virtual bool IsThirsty() { return false; }

    protected virtual void EatStateForSpecificAnimal() { }

    protected virtual void DrinkStateForSpecificAnimal() { }
    protected virtual void UpdateIdle() { return; }
    protected virtual void UpdateWander() { return; }
    protected virtual void UpdateEat() { return; }
    protected virtual void UpdateDrink() { return;  }
    protected virtual void UpdateHunt() { return; }
    protected virtual void UpdateFlee() { return; }
    protected virtual void HuntState() { return; }
    protected virtual void FleeState() { return; }

}