using System;
using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;


public class MooseBehaviour : AnimalBehaviour
{

    GameObject foodTarget;
    AnimalFOV fov;
    AnimalHearing hearing;

    float foodSearchingCooldown;

    GameObject enemy; // For fleeing from wolves and bears

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f; // Time interval for recalculating path to prey

    float memoryDecisionCooldown = 0f;

    [Header("Layers")]
    [SerializeField] LayerMask foodLayer;

    private List<WolfBehaviour> wolfAttackers = new List<WolfBehaviour>();
    private List<BearBehaviour> bearAttackers = new List<BearBehaviour>();

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<AnimalHearing>();
    }


    protected override void Update()
    {

        if (!agent.isOnNavMesh)
            return;
        

        base.Update();

        if (isDead)
            return;

        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f); // "isWalking" Ã¤r en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f); // "isRunning" Ã¤r en bool i animator

        if (hearing != null && hearing.HeardSomething)
        {
            Animal heard = hearing.HeardAnimal;
            if (heard.species == Species.bear || heard.species == Species.wolf)
            {
                memory.RememberDanger(heard.transform.position); //AnimalMemory Danger

                WolfBehaviour wolf = heard.GetComponent<WolfBehaviour>(); // Wolf heard
                if(wolf != null && wolf.CurrentTarget == gameObject)
                {
                    enemy = heard.gameObject;
                    ChangeState(State.Fleeing);
                    return;
                }

                BearBehaviour bear = heard.GetComponent<BearBehaviour>(); // Bear heard
                if (bear != null && bear.CurrentTarget == gameObject)
                {
                    enemy = heard.gameObject;
                    ChangeState(State.Fleeing);
                    return;
                }

            }
        }

        // If not currently eating, drinking or fleeing, check if the moose needs to eat or drink and switch to the appropriate state
        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Fleeing)
        {

            // If the moose is more thirsty than hungry, switch to drink state, if more hungry than thirsty, switch to eat state
            if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
            {
                if (FindWater())
                {
                    ChangeState(State.Drink);
                    return;
                }
            }


            if (IsHungry())
            {
                memoryDecisionCooldown -= Time.deltaTime;

                if (FindFood())
                {
                    ChangeState(State.Eat);
                    return;
                }

                if (memoryDecisionCooldown <= 0f)
                {
                    memoryDecisionCooldown = 2f; // decide every 2 sec

                    if (UnityEngine.Random.value < 0.2f) // 20% chance for exploration instead of memory
                    {
                        ChangeState(State.Wander);
                        return;
                    }

                    Vector2Int targetChunk = DecideFoodTargetChunk();

                    if (targetChunk.x != -1)
                    {
                        Vector3 targetPos = memory.GetRandomPointInChunk(targetChunk);

                        if (agent.isOnNavMesh)
                        {
                            agent.SetDestination(targetPos);
                            ChangeState(State.Wander);
                        }
                    }
                    else
                    {
                        ChangeState(State.Wander); 
                    }
                }

            }
        }   

    }


    // Finds the closest food item within the detection radius and sets it as the target

    private Collider[] hits = new Collider[10];
    bool FindFood()
    {
        if(foodSearchingCooldown > 0f)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return foodTarget != null;
        }

        foodSearchingCooldown = 1.5f;

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, animal.sightRange, hits, foodLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hits[i];

            if (hit == null)
                continue;
            if (!hit.CompareTag("Plant"))
                continue;
            if (!fov.IsInFOV(hit.transform))
                continue;
            
            memory.RememberFood(hit.transform.position);

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFood = hit.gameObject;
            }
        }

        if(closestFood != null)
        {
            foodTarget = closestFood;
            return true;
        }

        return false;
    }
        
    


    protected override void EatStateForSpecificAnimal()
    {
        if (foodTarget != null && agent.isOnNavMesh) //added && agent.isOnNavMesh
        {
            agent.isStopped = false;
            agent.SetDestination(foodTarget.transform.position);
        }
    }  


    protected override void UpdateWander()
    {
        if (hasArrived()) // If the moose has reached its destination, switch to idle state
        {
            ChangeState(State.Idle);
        }
    }

    protected override void UpdateIdle()
    {

        waitTime -= Time.deltaTime; // Decrease waiting time
        if (waitTime < 0f) // To occasionally switch to wandering
        {
            ChangeState(State.Wander);
        }
    }

    protected override void UpdateEat()
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

    protected override void UpdateFlee()
    {
        // If there is no enemy, switch back to wandering
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

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(fleePoint);
            }
            //agent.SetDestination(fleePoint);
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

    protected override void FleeState()
    {
        // set food and water to null to stop the moose from trying to eat or drink while fleeing
        foodTarget = null; 
        waterTarget = null;

        agent.isStopped = false;
        agent.speed = animal.runningSpeed; // Increase speed while fleeing
    }

    public void OnBeingHunted(GameObject predator)
    {
        enemy = predator;
        ChangeState(State.Fleeing);
    }

    public void OnNoLongerHunted(GameObject predator)
    {
        if(enemy == predator)
        {
            StatisticsTableManager.instance.MooseSuccessfulEscapeCount++;

            enemy = null;
            agent.speed = animal.speed; // Reset speed to normal
            if (CurrentState == State.Fleeing)
            {
                ChangeState(State.Wander);
            }
        }
    }

    public void InflictDamage(float damage)
    {
        needs.TakeDamage(damage);
    }

    public override void OnDeath()
    {
        bool wolfKill = wolfAttackers.Count > 0;

        foreach (WolfBehaviour wolf in wolfAttackers.ToList())
        {
            if(wolf != null)
            {
                wolf.notifyDeath(); 
            }
        }

        wolfAttackers.Clear();

        if (wolfKill)
            StatisticsTableManager.instance.WolfSuccessfulHuntsCount++;

        bool bearKill = bearAttackers.Count > 0; 
        
        foreach(BearBehaviour bear in bearAttackers.ToList())
        {
            if (bear != null)
            {
                bear.notifyDeath();
            }
        }

        bearAttackers.Clear();
        // If bear killed the moose, then increase counter in the statistics table
        if (bearKill) 
            StatisticsTableManager.instance.BearSuccessfulHuntsCount++;

        base.OnDeath();
    }

    Vector2Int DecideFoodTargetChunk()
    {
        // 0 = full, 1 = starving
        float hunger = 1f - needs.howHungryInPercent;

        Vector2Int bestChunk = new Vector2Int(-1, -1);

        float bestScore = float.MinValue;

        // World pos to chunk
        Vector2Int currentChunk = memory.GetChunk(transform.position);

        // Hunger affects risk tolerance

        float dangerWeight = Mathf.Lerp(3f, 0.3f, hunger);

        if (SeasonManager.Instance.IsWinter)
        {
            dangerWeight *= 1.5f;
        }

        for (int x = 0; x < memory.GetGridSizeX(); x++)
        {
            for (int z = 0; z < memory.GetGridSizeZ(); z++)
            {
                float food = memory.GetFoodValue(x, z);
                float danger = memory.GetDangerValue(x, z);

                /*
                if (food <= 0f)
                    continue;
                */

                float distance = Vector2.Distance(
                    new Vector2(x, z),
                    new Vector2(currentChunk.x, currentChunk.y)
                );

                float reward = food;
                float risk = danger * dangerWeight;
                float effort = distance * 0.3f;

                // more random when not so hungry, less random when hungry
                float randomness = UnityEngine.Random.Range(-1f, 1f) * (1f - hunger);
                //float randomness = UnityEngine.Random.Range(-0.2f, 0.2f);  // old

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
            return new Vector2Int(-1, -1); // force exploration
        }

        return bestChunk;
    }

    public void RegisterWolfAttacker(WolfBehaviour wolf)
    {
        if (wolf == null) return;

        if (!wolfAttackers.Contains(wolf))
            wolfAttackers.Add(wolf);
    }

    public void UnregisterWolfAttacker(WolfBehaviour wolf)
    {
        if (wolf == null) return;
        wolfAttackers.Remove(wolf);
    }

    public void RegisterBearAttacker(BearBehaviour bear)
    {
        if (bear == null) return;

        if (!bearAttackers.Contains(bear))
            bearAttackers.Add(bear);
    }

    public void UnregisterBearAttacker(BearBehaviour bear)
    {
        if (bear == null) return;
        bearAttackers.Remove(bear);
    }

}