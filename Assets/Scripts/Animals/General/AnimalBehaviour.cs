using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;



public abstract class AnimalBehaviour : MonoBehaviour
{

    public enum State
    {
        Idle,           // Resting/waiting
        Wander,         // Moving aimlessly
        SearchFood,     // Actively looking for food
        SearchWater,    // Actively looking for water
        SearchMate,     // Actively looking for a mate
        Eat,            // Consuming food (or moving to food location)
        Drink,          // Consuming water (or moving to water location)
        Mating,         // In mating process
        Hunt,           // For animals that hunt, wolves and bears
        Fleeing,        // Running away from threat
        Defend,         // Fighting back
        Hibernate,      // Winter dormancy
        Dead,           // Dead
    }

    [System.Serializable]
    public class StateTracker
    {
        public float totalTimeInState;
        public float currentSessionTime;
        
        public void Update(float deltaTime)
        {
            totalTimeInState += deltaTime;
            currentSessionTime += deltaTime;
        }
        
        public void ResetSession()
        {
            currentSessionTime = 0f;
        }
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

    public bool isPregnant;

    [Header("State Tracking")]
    protected Dictionary<State, StateTracker> stateTrackers = new Dictionary<State, StateTracker>();

    [Header("Water Layer")]
    [SerializeField] LayerMask waterLayer;

    public static event Action OnPreyDeath;
    public static event Action OnPredatorDeath;

    public GameObject foodTarget;

    public GameObject enemy; // For fleeing from wolves and bears

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f;

    protected GameObject mateTarget;
    
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

        // Initialize state trackers for all states
        foreach (State state in System.Enum.GetValues(typeof(State)))
        {
            stateTrackers[state] = new StateTracker();
        }
    }

    // Checks if the moose has reached its destination
    protected bool hasArrived()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    // Changes the state of the moose and updates behavior accordingly
    protected void ChangeState(State newState)
    {
        // Reset the session timer for the previous state
        if (stateTrackers.ContainsKey(CurrentState))
        {
            stateTrackers[CurrentState].ResetSession();
        }

        // Clear mate target when leaving mating-related states
        if (CurrentState == State.SearchMate || CurrentState == State.Mating)
        {
            mateTarget = null;
        }

        CurrentState = newState;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled) // If the animal has no agent or is dead
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
            case State.SearchFood:
                agent.isStopped = false;
                agent.SetDestination(GetRandomPoints());
                break;
            case State.SearchWater:
                agent.isStopped = false;
                if (FindWater())
                {
                    ChangeState(State.Drink);
                }
                break;
            case State.SearchMate:
                agent.isStopped = false;
                agent.SetDestination(GetRandomPoints());
                break;
            case State.Eat:
                agent.isStopped = false;
                agent.SetDestination(foodTarget.transform.position);
                break;
            case State.Drink:
                DrinkState();
                break;
            case State.Mating:
                agent.isStopped = true;
                break;
            case State.Hunt:
                HuntState();
                break;
            case State.Fleeing:
                FleeState();
                break;
            case State.Defend:
                DefendState();
                break;
            case State.Hibernate:
                HibernationState();
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
        if (stateTrackers.ContainsKey(CurrentState))
        {
            stateTrackers[CurrentState].Update(Time.deltaTime);
        }

        AnimatorStateInfo animatorState = anim.GetCurrentAnimatorStateInfo(0);

        if (isDead)
        {
            return;
        }

        ApplyPregnancyEffects();

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
        } 
        else
        {
            needs.RegenerateStamina();
        }

        switch (CurrentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Wander:
                UpdateWander();
                break;
            case State.SearchFood:
                UpdateSearchFood();
                break;
            case State.SearchWater:
                UpdateSearchWater();
                break;
            case State.SearchMate:
                UpdateSearchMate();
                break;
            case State.Eat:
                UpdateEat();
                break;
            case State.Drink:
                UpdateDrink();
                break;
            case State.Mating:
                UpdateMating();
                break;
            case State.Hunt:
                UpdateHunt();
                break;
            case State.Fleeing:
                UpdateFlee();
                break;
            case State.Hibernate:
                HibernationState();
                break;
            case State.Defend:
                UpdateDefend();
                break;
            case State.Dead:
                // Do nothing i guess? 
                break;
        }
    }

    public virtual void SetPregnant(bool value)
    {
        if (isDead) return;
        isPregnant = value;
    }

    public void StartWandering()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (!isDead)
            ChangeState(State.Wander);
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
    protected virtual void EatStateForSpecificAnimal() {return;}

    protected virtual void UpdateIdle() 
    { 
        waitTime -= Time.deltaTime; 
        if (waitTime < 0f)
        {
            ChangeState(State.Wander);
        } 

    }
    protected virtual void UpdateWander() 
    { 
        if (hasArrived())
        {
            ChangeState(State.Idle);
        } 
    }
    protected virtual void UpdateEat()
    { 
        if (foodTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        if (hasArrived())
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(foodTarget.transform.position);
        }

        if (!needs.isHungry)
        {
            foodTarget = null;
            ChangeState(State.Wander);
        }; 
    }

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

            DrinkState(); 
        }

        else
        {
            agent.isStopped = false;
        }
 
    }    

    protected virtual void ApplyPregnancyEffects()
    {
        if (agent == null || animal == null || needs == null) return;

        if (isPregnant)
        {
            agent.speed = animal.speed * 0.5f;
        }
        else
        {
            agent.speed = animal.speed;
        }
    }
    protected virtual void UpdateHunt() { return; }
    protected virtual void UpdateFlee() 
    {
        if (enemy == null)
        {
            agent.speed = animal.speed;
            ChangeState(State.Wander);
            return;
        }

        agent.speed = needs.noMoreStamina ? animal.speed : animal.runningSpeed;

        float distanceToPredator = Vector3.Distance(transform.position, enemy.transform.position);
        if (distanceToPredator > animal.sightRange * 1.2)
        {
            enemy = null;
            agent.speed = animal.speed;
            ChangeState(State.Wander);
            return;
        }

        fleeRepathTimer += Time.deltaTime;
        if (fleeRepathTimer >= fleeRepathInterval)
        {
            Vector3 fleePoint = CalculateFleePath();
            agent.SetDestination(fleePoint);
            fleeRepathTimer = 0f;
        } 
    }
    
    private Vector3 CalculateFleePath()
    {
        Vector3 dirAwayFromEnemy = (transform.position - enemy.transform.position).normalized;

        Vector3[] directionTests = new Vector3[] // Which direction is best to flee to (to avoid walking to edge of terrain)
        {
            dirAwayFromEnemy,
            Quaternion.Euler(0,45,0) * dirAwayFromEnemy,
            Quaternion.Euler(0,-45,0) * dirAwayFromEnemy,
        };

        Vector3 bestPoint = transform.position + dirAwayFromEnemy * animal.sightRange;
        float furthestEdge = -1f;

        foreach (Vector3 direction in directionTests)
        {
            Vector3 point = transform.position + direction * animal.sightRange;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, animal.sightRange, NavMesh.AllAreas))
            {
                NavMeshHit wallHit;
                if (NavMesh.FindClosestEdge(hit.position, out wallHit, NavMesh.AllAreas))
                {
                    if (wallHit.distance > furthestEdge)
                    {
                        furthestEdge = wallHit.distance;
                        bestPoint = hit.position;
                    }
                }
            }
        }
        return bestPoint;
    }
    protected virtual void HuntState() { return; }
    protected virtual void FleeState()
    {
        foodTarget = null; 
        waterTarget = null;
        agent.isStopped = false;
        agent.speed = animal.runningSpeed;
    
    }

    protected virtual void HibernationState() { return; }
    protected virtual void DefendState() { return; }
    protected virtual void UpdateDefend() { return; }

    // New state virtual methods
    protected virtual void UpdateSearchFood() { return; }
    protected virtual void UpdateSearchWater() { 
    if (FindWater())
        {
            ChangeState(State.Drink);
            return;
        }

        if (hasArrived())
        {
            agent.SetDestination(GetRandomPoints());
        } 
    }
    protected virtual void UpdateSearchMate()
    {
        if (mateTarget == null)
        {
            mateTarget = FindPotentialMate();

            if (mateTarget == null)
            {
                // No mate found - always ensure we have a wander destination
                if (hasArrived() || agent.velocity.magnitude < 0.1f)
                {
                    agent.SetDestination(GetRandomPoints());
                }
                return;
            }
        }

        agent.isStopped = false;
        agent.SetDestination(mateTarget.transform.position);

        AnimalBehaviour otherBehaviour = mateTarget.GetComponent<AnimalBehaviour>();
        if (otherBehaviour == null || otherBehaviour.isDead)
        {
            mateTarget = null;
            // Ensure we have a destination when mate becomes invalid
            if (agent.velocity.magnitude < 0.1f)
            {
                agent.SetDestination(GetRandomPoints());
            }
            return;
        }

        if (hasArrived())
        {
            Animal other = mateTarget.GetComponent<Animal>();
            Mating otherMating = mateTarget.GetComponent<Mating>();
            
            if (other != null && otherBehaviour != null && otherMating != null)
            {
                if (IsCompatibleForMating(other, otherBehaviour, otherMating))
                {
                    ChangeState(State.Mating);
                    otherBehaviour.ChangeState(State.Mating);
                    return;
                }
            }
            mateTarget = null;
            agent.SetDestination(GetRandomPoints());
        }
    }
    
    protected GameObject FindPotentialMate()
    {
        Mating mating = GetComponent<Mating>();
        if (mating == null) return null;
        
        Collider[] nearby = Physics.OverlapSphere(transform.position, animal.sightRange);
        
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            
            Animal other = col.GetComponent<Animal>();
            if (other == null) continue;
            
            if (other.species != animal.species) continue;
            
            AnimalBehaviour otherBehaviour = col.GetComponent<AnimalBehaviour>();
            if (otherBehaviour == null || otherBehaviour.isDead) continue;
            
            if (otherBehaviour.CurrentState != State.Idle && 
                otherBehaviour.CurrentState != State.Wander && 
                otherBehaviour.CurrentState != State.SearchMate) 
                continue;
            
            if (other.IsMale == animal.IsMale) continue;
            if (other.age < other.grownUpAge) continue;
            if (otherBehaviour.isPregnant) continue;
            
            Mating otherMating = col.GetComponent<Mating>();
            AnimalNeeds otherNeeds = col.GetComponent<AnimalNeeds>();
            if (otherMating == null || otherNeeds == null) continue;
            if (!otherMating.HasEnoughNeeds(otherNeeds)) continue;
            
            return col.gameObject;
        }
        
        return null;
    }
    
    protected bool IsCompatibleForMating(Animal other, AnimalBehaviour otherBehaviour, Mating otherMating)
    {
        if (other.species != animal.species) return false;
        if (other.IsMale == animal.IsMale) return false;
        if (isPregnant || otherBehaviour.isPregnant) return false;
        if (animal.age < animal.grownUpAge || other.age < other.grownUpAge) return false;
        
        AnimalNeeds otherNeeds = otherBehaviour.GetComponent<AnimalNeeds>();
        if (otherNeeds == null) return false;
    
        Mating mating = GetComponent<Mating>();
        if (mating == null || !mating.HasEnoughNeeds(otherNeeds) || !mating.HasEnoughNeeds(needs)) 
            return false;
        
        if (mating.GetCooldownTimer() > 0f || otherMating.GetCooldownTimer() > 0f) 
            return false;
        
        return true;
    }
    
    protected virtual void UpdateMating() 
    { 
        agent.isStopped = true;
        Mating mating = GetComponent<Mating>();
        if (mating == null) 
        {
            ChangeState(State.Wander);
            mateTarget = null;
            return;
        }
        
        if (mateTarget == null)
        {
            ChangeState(State.SearchMate);
            return;
        }
        
        AnimalBehaviour otherBehaviour = mateTarget.GetComponent<AnimalBehaviour>();
        if (otherBehaviour == null || otherBehaviour.CurrentState != State.Mating)
        {
            ChangeState(State.SearchMate);
            mateTarget = null;
            return;
        }
        
        float distanceToMate = Vector3.Distance(transform.position, mateTarget.transform.position);
        if (distanceToMate <= mating.matingRange)
        {
            mating.TryMate(mateTarget);
            
            ChangeState(State.Wander);
            mateTarget = null;
        }
    }

    public float GetStateTime(State state)
    {
        if (stateTrackers.ContainsKey(state))
        {
            return stateTrackers[state].currentSessionTime;
        }
        return 0f;
    }

    public float GetTotalStateTime(State state)
    {
        if (stateTrackers.ContainsKey(state))
        {
            return stateTrackers[state].totalTimeInState;
        }
        return 0f;
    }

    public Dictionary<State, StateTracker> GetAllStateTrackers()
    {
        return stateTrackers;
    }

    public void InflictDamage(float damage)
    {
        needs.TakeDamage(damage);
    }
}