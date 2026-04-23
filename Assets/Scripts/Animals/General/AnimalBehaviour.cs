using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;



public abstract class AnimalBehaviour : MonoBehaviour
{

    public float attackTimer = 0f;
    public float attackInterval = 1f;

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
    protected Mating mating;

    protected GameObject waterTarget;

    public bool isDead;

    public bool isPregnant;

    [Header("State Tracking")]
    protected Dictionary<State, StateTracker> stateTrackers = new Dictionary<State, StateTracker>();

    [Header("Water Layer")]
    [SerializeField] LayerMask waterLayer;

    public static event Action OnPreyDeath;
    public static event Action OnPredatorDeath;

    public float memoryDecisionCooldown = 0f;

    public GameObject foodTarget;

    public GameObject enemy;

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f;

    protected GameObject mateTarget;

    private bool isMating = false;
    
    private float wanderCooldown = 0f;

    protected float eatingTimer = 0f;
    protected float eatingDuration = 2f;

    protected float drinkingTimer = 0f;
    protected float drinkingDuration = 3f;
    
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
                anim.SetBool("isConsuming", false);

                break;
            case State.Wander:
                agent.isStopped = false;
                anim.SetBool("isConsuming", false);

                agent.SetDestination(GetRandomPoints());
                break;
            case State.SearchFood:
                agent.isStopped = false;
                break;
            case State.SearchWater:
                agent.isStopped = false;
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
                anim.SetBool("isConsuming", true);
                agent.isStopped = true;
                eatingTimer = 0f;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
                break;
            case State.Drink:
                if (waterTarget == null)
                {
                    ChangeState(State.SearchWater);
                    break;
                }
                anim.SetBool("isConsuming", true);
                agent.isStopped = true;
                drinkingTimer = 0f;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
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
        if (isDead && value) return;
        isPregnant = value;
    }

    public void StartWandering()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (!isDead)
            ChangeState(State.Wander);
    }

    float waterSearchingCooldown;
    public bool FindWater()
    {
        if (waterTarget != null)
        {
            return true;
        }

        if (waterSearchingCooldown > 0f && waterTarget != null)
        {
            waterSearchingCooldown -= Time.deltaTime;
            return true;
        }

        waterSearchingCooldown = 1.5f;

        float waterSearchingRadius = animal.sightRange * 2f;
        Collider[] hits = Physics.OverlapSphere(transform.position, waterSearchingRadius, waterLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestWater = null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Water"))
            {
                memory.RememberWater(hit.transform.position);

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
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(closestWater.transform.position, out navHit, 20f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                }
            }
            return true;
        }
        waterTarget = null;
        return false;
    }

    public virtual void OnDeath(bool killedByPredator = false) 
    {
        if (isDead) return;
        isDead = true;

        isPregnant = false;

        if (StatisticsTableManager.instance != null)
        {
            if (animal.species == Species.bear)
            {
                StatisticsTableManager.instance.BearDeathCount++;
                StatisticsTableManager.instance.BearTotalAgeAtDeath += animal.age;
                if (killedByPredator) StatisticsTableManager.instance.BearPredationCount++;
                else StatisticsTableManager.instance.BearStarvationCount++;
            }
            else if (animal.species == Species.wolf)
            {
                StatisticsTableManager.instance.WolfDeathCount++;
                StatisticsTableManager.instance.WolfTotalAgeAtDeath += animal.age;
                if (killedByPredator) StatisticsTableManager.instance.WolfPredationCount++;
                else StatisticsTableManager.instance.WolfStarvationCount++;
            }
            else if (animal.species == Species.moose)
            {
                StatisticsTableManager.instance.MooseDeathCount++;
                StatisticsTableManager.instance.MooseTotalAgeAtDeath += animal.age;
                if (killedByPredator) StatisticsTableManager.instance.MoosePredationCount++;
                else StatisticsTableManager.instance.MooseStarvationCount++;
            }
        }

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
            carcass.Initialize(Species.moose, 6, 100f);
        }

        if (animal != null && animal.species == Species.bear)
        {
            carcass.Initialize(Species.bear, 100, 100f);
        }

        if (animal != null && animal.species == Species.wolf)
        {
            carcass.Initialize(Species.wolf, 3, 80f);
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

        eatingTimer += Time.deltaTime;

        if (eatingTimer >= eatingDuration)
        {
            agent.isStopped = false;
            foodTarget = null;
            eatingTimer = 0f;
            ChangeState(State.Wander);
        }

    }

    public void UpdateDrink()
    {
        if (waterTarget == null)
        {
            ChangeState(State.SearchWater);
            return;
        }

        agent.isStopped = true;
        drinkingTimer += Time.deltaTime;

        if (drinkingTimer >= drinkingDuration)
        {
            if (needs.isThirsty)
            {
                needs.drinkFromSource(waterTarget.GetComponent<WaterSource>().Drink());
            }

            agent.isStopped = false;
            waterTarget = null;
            drinkingTimer = 0f;
             
            if (needs.isThirsty)
            {
                ChangeState(State.SearchFood);
            }
            else
            {
                ChangeState(State.Wander);
            }
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
    protected virtual void UpdateSearchWater() 
    { 
        if (waterTarget == null)
        {
            FindWater();
        }

        if (waterTarget != null)
        {
            if (hasArrived())
            {
                ChangeState(State.Drink);
                return;
            }
            return;
        }

        memoryDecisionCooldown -= Time.deltaTime;

        if (memoryDecisionCooldown <= 0f)
        {
            memoryDecisionCooldown = 2f;

            if (UnityEngine.Random.value < 0.2f)
            {
                agent.SetDestination(GetRandomPoints());
            }
            else
            {
                Vector2Int targetChunk = DecideFoodAndWaterTargetChunk();

                if (targetChunk.x != -1)
                {
                    Vector3 targetPos = memory.GetRandomPointInChunk(targetChunk);
                    agent.SetDestination(targetPos);
                }
                else
                {
                    agent.SetDestination(GetRandomPoints());
                }
            }
        }
        else if (hasArrived())
        {
            memoryDecisionCooldown = 0f;
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
        AnimalBehaviour otherBehaviour = mateTarget.GetComponentInParent<AnimalBehaviour>();
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

        Animal other = mateTarget.GetComponentInParent<Animal>();
        Mating otherMating = mateTarget.GetComponentInParent<Mating>();
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
            Animal other = col.GetComponentInParent<Animal>();
            if (other == null) continue;
            if (other.species != animal.species) continue;
            
            AnimalBehaviour otherBehaviour = col.GetComponentInParent<AnimalBehaviour>();
            if (otherBehaviour == null || otherBehaviour.isDead) continue;
            if (otherBehaviour.CurrentState != State.SearchMate) continue;
            if (other.IsMale == animal.IsMale) continue;
            if (other.age < other.grownUpAge) continue;
            if (otherBehaviour.isPregnant) continue;
            
            Mating otherMating = col.GetComponentInParent<Mating>();
            AnimalNeeds otherNeeds = col.GetComponentInParent<AnimalNeeds>();
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
        
        AnimalNeeds otherNeeds = otherBehaviour.GetComponentInParent<AnimalNeeds>();
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

        AnimalBehaviour otherBehaviour = mateTarget.GetComponentInParent<AnimalBehaviour>();
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
        if (!CanMate()) return;
    
        float distanceToRequester = Vector3.Distance(transform.position, requester.transform.position);
        Mating requesterMating = requester.GetComponentInParent<Mating>();
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

    public virtual void InflictDamage(float damage)
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