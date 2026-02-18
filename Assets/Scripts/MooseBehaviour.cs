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
    }

    [SerializeField]
    State CurrentState = State.Idle;

    // Waiting timers
    [SerializeField]
    float minTimeWaiting = 2f;

    [SerializeField]
    float maxTimeWaiting = 5f;

    [SerializeField]
    float waitTime = 0f;

    void Start()
    {
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
        }

    }

    void Update()
    {
        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f); // "isWalking" är en bool i animator

        switch (CurrentState)
        {
            case State.Idle:
                waitTime -= Time.deltaTime; // Decrease waiting time
                if (waitTime < 0f) // To occasionally switch to wandering
                {
                    ChangeState(State.Wander);
                }
                break;
            case State.Wander:
                if (hasArrived())
                {
                    ChangeState(State.Idle);
                }
                break;
        }
    }

}
