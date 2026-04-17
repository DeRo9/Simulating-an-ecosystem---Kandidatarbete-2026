using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;



public abstract class AnimalBehaviour : MonoBehaviour
{

    public enum State
    {
        Idle,     
        Wander,   
        SearchFood,   
        SearchWater,   
        SearchMate,    
        Eat,            
        Drink,   
        Mating,      
        Hunt,    
        Fleeing,
        Defend, 
        Hibernate,
        Dead, 
    }

    public class StateTracker
    {
        public float timeInState;        
        public void Update(float deltaTime)
        {
            timeInState += deltaTime;
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

    public GameObject enemy;

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f;

    protected GameObject mateTarget;

    private bool isMating = false;

    public Mating mating;
    
    private float wanderCooldown = 0f;
    
    protected virtual void Start()
    {
        animal = GetComponent<Animal>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        needs = GetComponent<AnimalNeeds>();
        memory = GetComponent<AnimalMemory>();
        mating = GetComponent<Mating>();

        if (agent != null && animal != null)
        {
            agent.speed = animal.speed;
        }

        foreach (State state in System.Enum.GetValues(typeof(State)))
        {
            stateTrackers[state] = new StateTracker();
        }
    }

    protected bool hasArrived()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    protected void ChangeState(State newState)
    {
        if (CurrentState == State.Wander)
        {
            wanderCooldown = 0f;
        }

        if ((CurrentState == State.SearchMate || CurrentState == State.Mating) && newState != State.Mating)
        {
            mateTarget = null;
        }

        CurrentState = newState;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled) 
            return;

        agent.ResetPath();

        switch (CurrentState)
        {
            case State.Idle:
                waitTime = UnityEngine.Random.Range(minTimeWaiting, maxTimeWaiting);
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
                if (foodTarget == null)
                {
                    ChangeState(State.SearchFood);
                    break;
                }
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
                break;
        }
    }

    public Vector3 GetRandomPoints()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 20f;
        randomDirection.y = 0f;

        if (randomDirection.magnitude < 10f)
        {
            randomDirection = randomDirection.normalized * 10f;
        }

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
            animal.SetMovementState(moving,agent.velocity.magnitude);
        }

        if (agent.velocity.magnitude > animal.runningSpeed * 0.95f)
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
            carcass.Initialize(Species.moose, 10, 100f);
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
        wanderCooldown -= Time.deltaTime;
        if (wanderCooldown <= 0f)
        {
            agent.SetDestination(GetRandomPoints());
            wanderCooldown = 3f;
        }
        
        if (hasArrived())
        {
            ChangeState(State.Idle);
        } 
    }
    protected virtual void UpdateEat()
    { 
        if (foodTarget == null)
        {
            ChangeState(State.SearchFood);
            return;
        }

        if (!hasArrived())
        {
            agent.isStopped = false;
            agent.SetDestination(foodTarget.transform.position);
        }
        else
        {
            agent.isStopped = true;
            
            Carcass carcass = foodTarget.GetComponent<Carcass>();
            if (carcass != null && needs.isHungry)
            {
                float nutrition = carcass.ConsumeOneFeed();
                if (nutrition > 0f)
                {
                    needs.Eat(nutrition);
                    foodTarget = null;
                    needs.RegenerateHealth(20f);
                    ChangeState(State.Wander);
                    return;
                }
                else
                {
                    foodTarget = null;
                    ChangeState(State.SearchFood);
                    return;
                }
            }

            //Unsure if this is good but handles cases when the animal for some reason cannot eat food!!!!
            else if (carcass == null && needs.isHungry && foodTarget != null)
            {
                needs.Eat(20f);
                Destroy(foodTarget);
                foodTarget = null;
                needs.RegenerateHealth(20f);
                ChangeState(State.Wander);
                return;
            }
            else
            {
                foodTarget = null;
                ChangeState(State.SearchFood);
                return;
            }
        }
        
        if (!needs.isHungry)
        {
            foodTarget = null;
            ChangeState(State.Wander);
        }
    }

    public void OnFinishedDrinking()
    {
        waterTarget = null;
        agent.isStopped = true;
        ChangeState(State.Idle);
    }

    public void UpdateDrink()
    {
        if (waterTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        if (hasArrived())
        {
            agent.isStopped = true;

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
            agent.speed = animal.speed * 0.8f;
        }
        else
        {
            agent.speed = animal.speed;
        }
    }
    protected virtual void UpdateHunt() { return; }
    protected virtual void UpdateFlee() 
    {
        if (needs.noMoreStamina)
        {
            agent.speed = animal.speed;
            ChangeState(State.Idle); 
            return;
        }

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

        Vector3[] directionTests = new Vector3[]
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

    public bool CanMate()
    {
        if (mating == null || isDead) return false;
        if (isPregnant) return false;
        if (mating.GetPregnancyTimer() > 0f) return false;
        if (animal.age < animal.grownUpAge) return false;
        if (mating.GetCooldownTimer() > 0f) return false;
        return true;
    }

protected float searchMateCooldown = 0f;

private bool hasSearchDestination = false;
protected virtual void UpdateSearchMate()
{
    if (mateTarget == null)
    {
        mateTarget = FindPotentialMate();

        if (mateTarget != null)
        {
            hasSearchDestination = false; 
        }
    }

    if (mateTarget != null)
    {
        AnimalBehaviour otherBehaviour = mateTarget.GetComponent<AnimalBehaviour>();
        if (otherBehaviour == null || otherBehaviour.isDead)
        {
            mateTarget = null;
            return;
        }

        float distance = Vector3.Distance(transform.position, mateTarget.transform.position);
        if (distance > mating.matingRange)
        {
            agent.isStopped = false;
            agent.SetDestination(mateTarget.transform.position);
            return;
        }

        Animal other = mateTarget.GetComponent<Animal>();
        Mating otherMating = mateTarget.GetComponent<Mating>();
        if (other != null && otherMating != null && IsCompatibleForMating(other, otherBehaviour, otherMating))
        {
            otherBehaviour.AcceptMateRequest(gameObject);
            ChangeState(State.Mating);
        }
        return;
    }

    if (!hasSearchDestination)
    {
        Vector3 destination = GetRandomPoints();
        agent.SetDestination(destination);
        hasSearchDestination = true;
    }

    if (hasArrived())
    {
        hasSearchDestination = false;
    }
}
    
    protected GameObject FindPotentialMate()
    {
        Mating mating = GetComponent<Mating>();
        if (mating == null) return null;
        
        Collider[] nearby = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            Animal other = col.GetComponent<Animal>();
            if (other == null) continue;
            if (other.species != animal.species) continue;
            
            AnimalBehaviour otherBehaviour = col.GetComponent<AnimalBehaviour>();
            if (otherBehaviour == null || otherBehaviour.isDead) continue;
            if (otherBehaviour.CurrentState != State.SearchMate) continue;
            if (other.IsMale == animal.IsMale) continue;
            if (other.age < other.grownUpAge) continue;
            if (otherBehaviour.isPregnant) continue;
            
            Mating otherMating = col.GetComponent<Mating>();
            AnimalNeeds otherNeeds = col.GetComponent<AnimalNeeds>();
            if (otherMating == null || otherNeeds == null) continue;
            if (!otherMating.HasEnoughNeeds(otherNeeds)) continue;
            if (mating.IsRejectedMate(col.gameObject)) continue;
            
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
            return;
        }

        if (mateTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        AnimalBehaviour otherBehaviour = mateTarget.GetComponent<AnimalBehaviour>();
        if (otherBehaviour == null || otherBehaviour.isDead)
        {
            mateTarget = null;
            ChangeState(State.Wander);
            return;
        }

        float distanceToMate = Vector3.Distance(transform.position, mateTarget.transform.position);
        if (distanceToMate > mating.matingRange)
        {
            agent.isStopped = false;
            agent.SetDestination(mateTarget.transform.position);
            return;
        }

        if (animal.IsMale)
        {
            mating.TryMate(mateTarget);
        }

        mateTarget = null;
        ChangeState(State.Wander);
    }

    public void AcceptMateRequest(GameObject requester)
    {
        if (requester == null || isDead) return;
    
        float distanceToRequester = Vector3.Distance(transform.position, requester.transform.position);
        Mating requesterMating = requester.GetComponent<Mating>();
        if (requesterMating == null || distanceToRequester > requesterMating.matingRange * 2f)
            return;
        
        mateTarget = requester;
        ChangeState(State.Mating);
    }

    public float GetTotalStateTime(State state)
    {
        if (stateTrackers.ContainsKey(state))
        {
            return stateTrackers[state].timeInState;
        }
        return 0f;
    }

    public void InflictDamage(float damage)
    {
        needs.TakeDamage(damage);
    }

    public Vector2Int DecideFoodAndWaterTargetChunk()
    {

        // 0 = full, 1 = starving
        float hunger = 1f - needs.howHungryInPercent;
        float thirst = 1f - needs.howThirstyInPercent;

        Vector2Int bestChunk = new Vector2Int(-1, -1);
        float bestScore = float.MinValue;

        Vector2Int currentChunk = memory.GetChunk(transform.position);

        float totalNeed = hunger + thirst;

        if (totalNeed <= 0.1f)
        {
            return new Vector2Int(-1, -1); 
        }

        float hungerWeight = hunger / totalNeed;
        float thirstWeight = thirst / totalNeed;

        // Risk taking based on strongest need
        float urgency = Mathf.Max(hunger, thirst);

        float dangerWeight = Mathf.Lerp(3f, 0.3f, urgency);

        if (SeasonManager.Instance.IsWinter)
        {
            dangerWeight *= 1.5f;
        }

        for (int x = 0; x < memory.GetGridSizeX(); x++)
        {
            for (int z = 0; z < memory.GetGridSizeZ(); z++)
            {
                float food = memory.GetFoodValue(x, z);
                float water = memory.GetWaterValue(x, z);
                float danger = memory.GetDangerValue(x, z);

                float distance = Vector2.Distance(
                    new Vector2(x, z),
                    new Vector2(currentChunk.x, currentChunk.y)
                );


                float reward = (food * hungerWeight) + (water * thirstWeight);

                float risk = danger * dangerWeight;
                float effort = distance * 0.3f;

        
                float randomness = UnityEngine.Random.Range(-1f, 1f) * (1f - urgency);

                float score = reward - risk - effort + randomness;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestChunk = new Vector2Int(x, z);
                }
            }
        }

        if (bestScore < 1f)
        {
            return new Vector2Int(-1, -1);
        }

        return bestChunk;

    }

}