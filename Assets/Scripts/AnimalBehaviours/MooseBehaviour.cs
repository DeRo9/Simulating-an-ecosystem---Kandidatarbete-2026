using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class MooseBehaviour : MonoBehaviour
{
    // Components
    private Rigidbody rb;
    private Animator anim;
    private NavMeshAgent agent;

    // Home area for the moose
    public Area HomeArea;

    // Internal state of the moose
    enum State
    {
        Idle,
        Wander,
        Eat,
    }

    [SerializeField]
    State CurrentState = State.Idle;

    MooseNeeds needs;

    // Waiting timers
    [SerializeField]
    float minTimeWaiting = 2f;

    [SerializeField]
    float maxTimeWaiting = 5f;

    [SerializeField]
    float waitTime = 0f;

    GameObject foodTarget;
    float foodDetectionRadius = 20f;

    void Start()
    {
        needs = GetComponent<MooseNeeds>();

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Checks if the moose has reached its destination
    bool hasArrived()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    // Changes the state of the moose and updates behavior accordingly
    void ChangeState(State newState)
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
                if (foodTarget != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(foodTarget.transform.position);
                }

                break;
        }

    }

    void Update()
    {
        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f); // "isWalking" är en bool i animator


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
        }
    }

    // Finds the closest food item within the detection radius and sets it as the target
    bool FindFood()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, foodDetectionRadius);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Plant"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFood = hit.gameObject;
                }

            }

        }

        if(closestFood != null)
        {
            foodTarget = closestFood;
            return true;
        }

        return false;
    }

    void UpdateWander()
    {
        if (hasArrived()) // If the moose has reached its destination, switch to idle state
        {
            ChangeState(State.Idle);
        }
    }

    void UpdateIdle()
    {
        // Moose is hungry, find food
        if (needs.isHungry && FindFood())
        {
            ChangeState(State.Eat);
        }

        waitTime -= Time.deltaTime; // Decrease waiting time
        if (waitTime < 0f) // To occasionally switch to wandering
        {
            ChangeState(State.Wander);
        }
    }

    void UpdateEat()
    {
        // If the food target is null, switch back to wandering
        if (foodTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        // If the moose has reached the food, stop moving
        if (hasArrived())
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }

        // If the moose is no longer hungry, stop eating and switch back to wandering
        if (!needs.isHungry)
        {
            foodTarget = null;
            ChangeState(State.Wander);
        }
    }

}
