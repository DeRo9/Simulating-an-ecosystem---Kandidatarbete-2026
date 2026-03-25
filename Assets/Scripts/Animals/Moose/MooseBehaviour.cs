using System;
using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class MooseBehaviour : AnimalBehaviour
{

    GameObject foodTarget;
    MooseFOV fov;
    MooseHearing hearing;

    float foodSearchingCooldown;

    GameObject enemy; // For fleeing from wolves and bears

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f; // Time interval for recalculating path to prey

    [Header("Layers")]
    [SerializeField]
    LayerMask foodLayer;

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<MooseFOV>();
        hearing = GetComponent<MooseHearing>();
    }


    protected override void Update()
    {
        

        base.Update();

        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f); // "isWalking" Ã¤r en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f); // "isRunning" Ã¤r en bool i animator

        if (hearing != null && hearing.HeardSomething)
        {
            Animal heard = hearing.HeardAnimal;
            if (heard.species == Species.bear || heard.species == Species.wolf)
            {
                memory.RememberDanger(heard.transform.position); //AnimalMemory Danger
                Debug.Log("Moose remember a dangerous chunk");

                WolfBehaviour wolf = heard.GetComponent<WolfBehaviour>(); // Wolf heard
                if(animal != null && wolf.CurrentTarget == gameObject)
                {
                    Debug.Log("Moose heard a wolf");
                    enemy = heard.gameObject;
                    ChangeState(State.Fleeing);
                    return;
                }

                BearBehaviour bear = heard.GetComponent<BearBehaviour>(); // Bear heard
                if (animal != null && bear.CurrentTarget == gameObject)
                {
                    Debug.Log("Moose heard a bear");
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
                if (FindFood())
                {
                    ChangeState(State.Eat);
                    return;
                }

            // TODO (AnimalMemory)
            // Implement some way to decide wheter to go to another place
            // from memory. 
            // Could be risky to go to a certain chunk, should it then go to a lower
            // food chunk, or go around the dangrous chunks to reach the best food
            // chunk, or should it just stay or explore new areas and try to find food?
            }
        }

    }




    // Finds the closest food item within the detection radius and sets it as the target
    bool FindFood()
    {

        if(foodSearchingCooldown > 0f)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return foodTarget != null;
        }
        foodSearchingCooldown = 0.5f;

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, foodLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {


            if (!fov.IsInFOV(hit.transform))
            {
                continue; // Skip if not in FOV
            }

            Debug.Log("Moose found plant.");
            if (hit.CompareTag("Plant"))
            {
                memory.RememberFood(hit.transform.position); //AnimalMemory Food
                Debug.Log("Moose remembered food");
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


    protected override void EatStateForSpecificAnimal()
    {
        if (foodTarget != null)
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
            Debug.Log("Moose ate.");
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
        base.OnDeath();

        WolfBehaviour[] wolves = FindObjectsByType<WolfBehaviour>(FindObjectsSortMode.None); // All wolves hunting the moose
        foreach (WolfBehaviour wolf in wolves)
        {
            if(wolf.CurrentTarget == gameObject)
            {
                wolf.notifyDeath();
            }
        }

        BearBehaviour[] bears = FindObjectsByType <BearBehaviour> (FindObjectsSortMode.None);
        foreach(BearBehaviour bear in bears)
        {
            if (bear.CurrentTarget == gameObject)
            {
                bear.notifyDeath();
            }
        }

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

    for (int x = 0; x < memory.GetGridSizeX(); x++)
    {
        for (int z = 0; z < memory.GetGridSizeZ(); z++)
        {
            float food = memory.GetFoodValue(x, z);
            float danger = memory.GetDangerValue(x, z);

            // Skip empty memory?
            if (food <= 0f)
                continue;

            float distance = Vector2.Distance(
                new Vector2(x, z),
                new Vector2(currentChunk.x, currentChunk.y)
            );

            float reward = food;
            float risk = danger * dangerWeight;
            float effort = distance * 0.3f;

            float score = reward - risk - effort;

            if (score > bestScore)
            {
                bestScore = score;
                bestChunk = new Vector2Int(x, z);
            }
        }
    }

    return bestChunk;
}


/*
    Vector2Int DecideFoodTargetChunk()
    {

    // 0 = full, 1 = starving
    float hunger = 1f - needs.howHungryInPercent; 

    Vector2Int bestChunk = new Vector2Int(-1, -1);

    float bestScore = float.MinValue;

    // World pos to chunk
    Vector2Int currentChunk = memory.GetChunk(transform.position);


    for (int x = 0; x < memory.GetGridSizeX(); x++) // limit search (performance!)
    {
        for (int z = 0; memory.GetGridSizeZ() < 20; z++)
        {
            float food = memory.GetFoodValue(x, z);
            float danger = memory.GetDangerValue(x, z);

            // bigger number = further away
            float distance = Vector2.Distance(new Vector2(x,z), new Vector2(currentChunk.x, currentChunk.y));

            float dangerWeight;

            if (hunger < 0.3f)        // not very hungry
                dangerWeight = 3f;    // avoid danger strongly
            else if (hunger < 0.7f)   // medium hunger
                dangerWeight = 1.5f;
            else                      // starving
                dangerWeight = 0.3f;  // ignore danger

            float score = food - (danger * dangerWeight) - (distance * 0.5f);

            if (score > bestScore)
            {
                bestScore = score;
                bestChunk = new Vector2Int(x, z);
            }
        }
    }

    return bestChunk;
}
*/



}