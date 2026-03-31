using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class WolfBehaviour : AnimalBehaviour
{

    AnimalFOV fov;
    WolfHearing hearing;
    float huntCooldown = 5f; // Time the wolf must wait after giving up on a hunt before it can hunt again
    [Header("Hunting")]
    [SerializeField]
    float huntCooldownTimer = 0;

    float attackRange = 3.5f; // Range within which the wolf can attack prey

    float repathTimer = 0f;
    float repathInterval = 0.3f; // Time interval for recalculating path to prey

    float attackTimer = 0f;
    float attackInterval = 1f; // Time interval for attacking, to prevent multiple attacks in quick succession

    float foodSearchingCooldown;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f; // Timer to wait before eating, otherwise the moose will instantly be eaten (Not leting the animation finish).
    bool waitingForDeathAnimation = false;

    GameObject foodTarget;
    GameObject pendingCarcass;

    [Header("Layers")]
    [SerializeField]
    LayerMask carcassLayer;

    [SerializeField]
    LayerMask PreyLayer;

    [Header("Pack")]
    Wolf wolf;
    WolfPackManager pack;

    public GameObject preyTarget;

    public GameObject CurrentTarget => preyTarget;

    GameObject enemy; // For fleeing from bears

    float fleeRepathTimer = 0f;
    float fleeRepathInterval = 2f; // Time interval for recalculating path to prey

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<WolfHearing>();
        
        wolf = GetComponent<Wolf>();
        if (wolf != null)
            pack = wolf.pack;
    }

    Vector3 followOffset = new Vector3(0, 0, -5f); // Offset to maintain behind the leader

    void FollowLeader()
    {
        if (pack == null || pack.leader == null)
            return;

        WolfBehaviour leaderBehaviour = pack.leader.GetComponent<WolfBehaviour>();

        // If leader is eating a carcass, followers should eat the same target
        if (wolf != null && !wolf.isLeader && leaderBehaviour != null && leaderBehaviour.CurrentState == State.Eat && leaderBehaviour.foodTarget != null)
        {
            foodTarget = leaderBehaviour.foodTarget;
            ChangeState(State.Eat);
            return;
        }

        Vector3 targetPosition = pack.leader.transform.position + pack.leader.transform.TransformDirection(followOffset);

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (leaderBehaviour != null && leaderBehaviour.preyTarget != null)
        {
            preyTarget = leaderBehaviour.preyTarget;
            ChangeState(State.Hunt);
            return;
        }

        if (distance > 2f)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.isStopped = true; // Stop moving if close enough to the target position
        }

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f);
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f);

        ApplySeperation();
    }

    public void ApplySeperation()
    {
        Vector3 separation = Vector3.zero;
        foreach (Wolf member in pack.members)
        {
            if (member != wolf) //Does not create this force upon itself
            {
                float distance = Vector3.Distance(transform.position, member.transform.position);
                if (distance < 2f) 
                {
                    separation += (transform.position - member.transform.position).normalized / distance;
                }
            }
        }

        agent.Move(separation * Time.deltaTime); 
    }


    protected override void Update()
    {
        pack = wolf.pack;
        if (pack != null && wolf != null && !wolf.isLeader && pack.leader != null)
        {
            if (CurrentState != State.Hunt && CurrentState != State.Eat)
            {
                FollowLeader();
                return; // Only return if we are actually in "follow mode"
            }
        }

        base.Update();

        if (isDead)
            return;

        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f); // "isWalking" är en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f); // "isRunning" är en bool i animator

        if (waitingForDeathAnimation)
        {
            deathWaitTimer -= Time.deltaTime;
            if(deathWaitTimer <= 0f)
            {
                waitingForDeathAnimation = false;
                foodTarget = pendingCarcass;
                pendingCarcass = null;
                agent.speed = animal.speed;
                ChangeState(State.Eat);
            }
            return;
        }


        if (hearing != null && hearing.HeardSomething)
        {

            Animal heard = hearing.HeardAnimal;
            if (heard.species == Species.bear)
            {
                BearBehaviour bear = heard.GetComponent<BearBehaviour>(); // bear 
                if (animal != null && bear.CurrentTarget == gameObject)
                {
                    Debug.Log("Wolf heard a bear");
                    enemy = heard.gameObject;
                    ChangeState(State.Fleeing);
                }
            }
        }

        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Hunt && CurrentState != State.Fleeing)
        {
            // If the wolf is more thirsty than hungry, switch to drink state, if more hungry than thirsty, switch to eat state
            if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
            {
                if (FindWater())
                {
                    ChangeState(State.Drink);
                    return;
                }
            }

            if (IsHungry() && huntCooldownTimer <= 0 && !needs.isTired) 
            {
                if (FindPrey())
                    ChangeState(State.Hunt);
                else if (FindCarcass())
                    ChangeState(State.Eat);
            }
        }

        if(huntCooldownTimer > 0)
            huntCooldownTimer -= Time.deltaTime;
        
    }

    bool FindPrey()
    {

        if (preyTarget != null)
            return true; // If the wolf already has a target, don't change prey

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, PreyLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestPrey = null;

        foreach (Collider hit in hits)
        {

            if (!fov.IsInFOV(hit.transform))
                continue; // Skip if the collider is not in the wolf's field of view


            if (hit.CompareTag("Moose"))
            {
                MooseBehaviour moose = hit.GetComponentInParent<MooseBehaviour>();
                if (moose == null || moose.isDead) continue; // Skip if moose is already dead.

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrey = hit.gameObject;
                }

            }

        }

        if (closestPrey != null)
        {
            preyTarget = closestPrey;
            StatisticsTableManager.instance.WolfhuntAttemptsCount++;

            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.OnBeingHunted(gameObject); // Notify the moose that it is being hunted
            }

            return true;
        }

        return false;
    }

    protected override void HuntState()
    {
        if (preyTarget != null)
        {
            agent.isStopped = false;
            agent.speed = animal.runningSpeed; // Set speed to hunt speed
            agent.SetDestination(preyTarget.transform.position);
        }
        else
        {
            agent.speed = animal.speed; // Reset speed to normal
            ChangeState(State.Wander); // If the prey is lost, switch to wandering
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
        if(CurrentState == State.Hunt) // Lose prey if being hunted by a bear
        {
            StatisticsTableManager.instance.BearInterferenceCount++;
            LostPrey();
        }
        enemy = predator;
        ChangeState(State.Fleeing);
    }

    public void OnNoLongerHunted(GameObject predator)
    {
        if (enemy == predator)
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

    protected override void UpdateHunt()
    {

        if (preyTarget == null || huntCooldownTimer > 0)
        {
            ChangeState(State.Wander);
            return; // If the wolf has no prey, switch to wandering
        }

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && moose.isDead)
        {
            notifyDeath();
            return;
        }

        if (preyTarget != null)
        {
            if (!fov.IsInFOV(preyTarget.transform))
            {
                LostPrey(); // If the prey is no longer in the wolf's field of view, give up
                return;
            }

            // Check if the prey is still within sight range
            float distance = Vector3.Distance(transform.position, preyTarget.transform.position);
            if (distance > animal.sightRange)
            {
                LostPrey(); // If the prey is too far away, give up
                return;
            }

            // Attack if within range
            if (distance <= attackRange)
            {
                agent.SetDestination(preyTarget.transform.position);
                AttackOnContact();
            }
            else
            {

                // Keep moving towards the prey
                agent.isStopped = false;
                repathTimer += Time.deltaTime;

                if (repathTimer >= repathInterval) // Stuttering prevention: Recalculate path to prey at regular intervals
                {
                    agent.SetDestination(preyTarget.transform.position);
                    repathTimer = 0f;
                }

            }

        }

        if (needs.noMoreStamina)
        {   
            LostPrey(); // If the wolf has no more stamina to run after, give up
            return;
        }

    }

    void AttackOnContact()
    {
        if (preyTarget != null)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                agent.isStopped = true;
                anim.SetTrigger("Attack");
                attackTimer = 0f;
                Debug.Log("Wolf attacked prey");
            }
        }
    }

    public void DamageMoose()
    {
        if (preyTarget != null)
        {
            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.InflictDamage(animal.attackDamage);
            }
        }
    }

    void LostPrey()
    {
        if (preyTarget == null) return;

        if (huntCooldownTimer > 0) return;

        StatisticsTableManager.instance.WolfhuntFailuresCount++;

        if (preyTarget != null)
        {
            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.OnNoLongerHunted(gameObject); // Notify the moose that it is no longer being hunted
            }
        }

        preyTarget = null; // Give up on the prey after hunting for too long
        huntCooldownTimer = huntCooldown; // Start cooldown timer
        agent.speed = animal.speed; // Reset speed to normal
        ChangeState(State.Wander);
    }

    protected override bool IsHungry()
    {
        return needs.isHungry;
    }

    protected override void EatStateForSpecificAnimal()
    {
        if (foodTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(foodTarget.transform.position);
        }
    }


    protected override void UpdateEat()
    {
        if (foodTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        if (hasArrived())
        {
            agent.isStopped = true;
            Carcass carcass = foodTarget.GetComponent<Carcass>();
            if (carcass != null)
            {
                float nutrition = carcass.ConsumeOneFeed();
                if (nutrition > 0f)
                {
                    needs.Eat(nutrition);
                }

                if (carcass.IsEmpty)
                {
                    foodTarget = null;
                    ChangeState(State.Wander);
                    return;
                }
            }
            else
            {
                foodTarget = null;
                ChangeState(State.Wander);
                return;
            }
        } 

        if (!needs.isHungry)
        {
            foodTarget = null;
            ChangeState(State.Wander);
        }
        else
        {
            agent.isStopped = false;
        }
    }

    protected override void UpdateWander()
    {
        if (hasArrived()) // If the wolf has reached its destination, switch to idle state
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

    public void notifyDeath()
    {
        if (preyTarget == null) return;

        pendingCarcass = preyTarget.GetComponentInParent<AnimalBehaviour>().gameObject;
        preyTarget = null;
        agent.isStopped = true;
        waitingForDeathAnimation = true;
        deathWaitTimer = deathWaitDuration;

    }

    // Finds the closest food item within the detection radius and sets it as the target
    bool FindCarcass()
    {

        if (foodSearchingCooldown > 0f)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return foodTarget != null;
        }
        foodSearchingCooldown = 0.5f;

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, carcassLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {


            if (!fov.IsInFOV(hit.transform))
            {
                continue; // Skip if not in FOV
            }

            
            if (hit.CompareTag("carcass"))
            {
                Debug.Log("Wolf found carcass.");
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFood = GetCarcassRoot(hit.gameObject);
                }

            }

        }

        if (closestFood != null)
        {
            foodTarget = closestFood;
            return true;
        }

        return false;
    }

    GameObject GetCarcassRoot(GameObject obj)
    {
        Carcass carcass = obj.GetComponentInParent<Carcass>();

        if(carcass != null)
        {
            return carcass.gameObject;
        }

        return obj;

    }

}
