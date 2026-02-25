using UnityEngine;
using UnityEngine.AI;


// This is an abstract class that defines the general behaviour of an animal. More specific implementations will inherit this class and build upon it.
public abstract class AnimalBehaviour : MonoBehaviour
{
    // The home area of the animal
    public Area HomeArea;

    // Internal states of the animal
    protected enum State
    {
        Idle,
        Wander,
        Eat,
        Drink,
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


    protected Rigidbody rb;
    protected Animator anim;
    protected NavMeshAgent agent;
    protected AnimalNeeds needs;


    protected virtual void Start()
    {

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        needs = GetComponent<AnimalNeeds>();

    }

    // Checks if the moose has reached its destination
    protected bool hasArrived()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
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
                agent.SetDestination(HomeArea.GetRandomPoints());
                break;
            case State.Eat:
                EatStateForSpecificAnimal();
                break;
            case State.Drink:
                DrinkStateForSpecificAnimal();
                break;
        }

    }

    protected virtual void Update()
    {
        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f); // "isWalking" Ã¤r en bool i animator
        
        if(CurrentState != State.Eat && CurrentState != State.Drink){
     
            if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
            {
                ChangeState(State.Drink);
            }
            else if (needs.howThirstyInPercent > needs.howHungryInPercent && IsHungry())
            {
                ChangeState(State.Eat);
            }
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
        }
    }

    protected virtual bool IsHungry() { return false; }
    protected virtual bool IsThirsty() { return false; }

    protected virtual void EatStateForSpecificAnimal() { }

    protected virtual void DrinkStateForSpecificAnimal() { }
    protected abstract void UpdateIdle();
    protected abstract void UpdateWander();
    protected abstract void UpdateEat();
    protected abstract void UpdateDrink();


}