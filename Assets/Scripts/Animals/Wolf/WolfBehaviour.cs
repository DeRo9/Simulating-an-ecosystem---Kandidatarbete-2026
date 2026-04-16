using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WolfBehaviour : AnimalBehaviour
{

    AnimalFOV fov;
    AnimalHearing hearing;
    float huntCooldown = 6f; // Time the wolf must wait after giving up on a hunt before it can hunt again
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

    float memoryDecisionCooldown = 0f;

    bool waitingForDeathAnimation = false;

    const int DefendThreshold = 5; // If the pack has 5 or more members, they will choose to defend against bears instead of fleeing

    GameObject foodTarget;
    GameObject pendingCarcass;

    private List<BearBehaviour> bearAttackers = new List<BearBehaviour>();

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
        hearing = GetComponent<AnimalHearing>();

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
            if (preyTarget != leaderBehaviour.preyTarget) // If the prey target of the leader has changed, update it for the follower as well
            {
                preyTarget = leaderBehaviour.preyTarget;

                MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
                if (moose != null)
                {
                    moose.RegisterWolfAttacker(this); // Register as attacker to the moose
                }


            }

            if (CurrentState != State.Hunt)
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

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f);
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f);

        ApplySeperation();
    }

    public void ApplySeperation()
    {
        if (pack == null || pack.members == null) return; // null check to avoid errors if the wolf is not in a pack for some reason

        Vector3 separation = Vector3.zero;
        foreach (Wolf member in pack.members)
        {
            if (member != null && member != wolf) // Does not create this force upon itself
            {
                float distance = Vector3.Distance(transform.position, member.transform.position);

                if (distance < 0.0001f) continue; // Avoid division by zero

                if (distance < 2f && SeasonManager.Instance.IsSummer)
                {
                    separation += (transform.position - member.transform.position).normalized / distance;
                }
                else if (distance < 1f && SeasonManager.Instance.IsWinter)
                {
                    separation += (transform.position - member.transform.position).normalized / distance;
                }
            }
        }

        if (agent != null && agent.isOnNavMesh)
            agent.Move(separation * Time.deltaTime);
    }


    protected override void Update()
    {
        pack = wolf.pack;

        base.Update();

        if (isDead)
            return;

        if (pack != null && wolf != null && !wolf.isLeader && pack.leader != null)
        {
            if (CurrentState != State.Hunt && CurrentState != State.Eat)
            {
                FollowLeader();
                return; // Only return if we are actually in "follow mode"
            }
        }

        if (SeasonManager.Instance.IsWinter)
        {
            huntCooldown = huntCooldown * 0.5f;
        }

        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f); // "isWalking" är en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f); // "isRunning" är en bool i animator

        if (waitingForDeathAnimation)
        {
            deathWaitTimer -= Time.deltaTime;
            if (deathWaitTimer <= 0f)
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
                    enemy = heard.gameObject;
                    DecideFightOrFlee();
                }
            }
        }

        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Hunt && CurrentState != State.Fleeing && CurrentState != State.Defend)
        {
            bool needsSomething = IsHungry() || IsThirsty();

            if (needsSomething)
            {
                memoryDecisionCooldown -= Time.deltaTime;

                // Try to satisfy the most urgent need first
                if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
                {
                    if (FindWater())
                    {
                        ChangeState(State.Drink);
                        return;
                    }
                }
                else if (IsHungry() && huntCooldownTimer <= 0 && !needs.isTired)
                {
                    if (FindPrey())
                    {
                        ChangeState(State.Hunt);
                        return;
                    }
                    else if (FindFood())
                    {
                        ChangeState(State.Eat);
                        return;
                    }
                }

                // Nothing nearby — use memory fallback
                if (memoryDecisionCooldown <= 0f)
                {
                    memoryDecisionCooldown = 2f;

                    if (UnityEngine.Random.value < 0.2f)
                    {
                        ChangeState(State.Wander);
                        return;
                    }

                    Vector2Int bestChunk = DecideTargetChunk();
                    if (bestChunk.x != -1 && agent.isOnNavMesh)
                    {
                        agent.SetDestination(memory.GetRandomPointInChunk(bestChunk));
                        ChangeState(State.Wander);
                    }
                    else
                    {
                        ChangeState(State.Wander);
                    }
                }
            }
        }
        if (huntCooldownTimer > 0)
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
                if ((pack == null || pack.countCurrentPackSize() <= 1) && moose.GetAge() >= moose.GetGrownUpAge()) continue; // If the wolf is alone, only target moose calves, not grown up moose,
                                                                                                                             // to balance the difficulty of hunting alone vs in a pack

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
            if (memory != null)
            {
                memory.RememberPrey(closestPrey.transform.position);
            }

            if (wolf.isLeader || pack == null || pack.countCurrentPackSize() <= 1)
                StatisticsTableManager.instance.WolfhuntAttemptsCount++;

            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.RegisterWolfAttacker(this);
                moose.OnBeingHunted(gameObject);
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
        if (memory != null)
        {
            memory.RememberDanger(transform.position);
        }
        if (CurrentState == State.Hunt) // Lose prey if being hunted by a bear
        {
            StatisticsTableManager.instance.BearInterferenceCount++;
            LostPrey();
        }
        enemy = predator;
        DecideFightOrFlee();
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

        agent.speed = needs.noMoreStamina ? animal.speed : animal.runningSpeed;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && moose.isDead)
        {
            notifyDeath();
            return;
        }

        if (preyTarget != null)
        {
            float distance = Vector3.Distance(transform.position, preyTarget.transform.position);

            if (!fov.IsInFOV(preyTarget.transform)) // Wolf FOV accidentally losing sight of prey when it is very close, this is a fix for that
            {
                LostPrey(); // If the prey is no longer in the wolf's field of view, give up
                return;

            }

            // Check if the prey is still within sight range
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
            }
            else
            {
                agent.isStopped = false;
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

        if (wolf.isLeader || pack == null || pack.countCurrentPackSize() <= 1)
            StatisticsTableManager.instance.WolfhuntFailuresCount++;

        if (preyTarget != null)
        {
            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.UnregisterWolfAttacker(this);
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
                    needs.RegenerateHealth(20f); // Regenerate some health upon eating carcass
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
        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null)
        {
            moose.UnregisterWolfAttacker(this);
        }
        pendingCarcass = preyTarget.GetComponentInParent<AnimalBehaviour>().gameObject;
        preyTarget = null;
        agent.isStopped = true;
        waitingForDeathAnimation = true;
        deathWaitTimer = deathWaitDuration;

    }

    // Finds the closest food item within the detection radius and sets it as the target

    private Collider[] hits = new Collider[10];
    bool FindFood()
    {
        if (foodSearchingCooldown > 0f)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return foodTarget != null;
        }

        foodSearchingCooldown = 1.5f;

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, animal.sightRange, hits, carcassLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hits[i];

            if (hit == null)
                continue;
            if (!hit.CompareTag("carcass"))
                continue;
            if (!fov.IsInFOV(hit.transform))
            {
                continue;
            }
            memory.RememberFood(hit.transform.position);

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFood = GetCarcassRoot(hit.gameObject);
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

        if (carcass != null)
        {
            return carcass.gameObject;
        }

        return obj;

    }

    Vector2Int DecideTargetChunk()
    {
        if (memory == null) return new Vector2Int(-1, -1);

        float hunger = 1f - needs.howHungryInPercent;
        float thirst = 1f - needs.howThirstyInPercent;

        Vector2Int bestChunk = new Vector2Int(-1, -1);
        float bestScore = float.MinValue;
        Vector2Int currentChunk = memory.GetChunk(transform.position);

        float totalNeed = hunger + thirst;
        if (totalNeed <= 0.1f)
            return new Vector2Int(-1, -1);

        float hungerWeight = hunger / totalNeed;
        float thirstWeight = thirst / totalNeed;

        float urgency = Mathf.Max(hunger, thirst);
        float dangerWeight = Mathf.Lerp(3f, 0.3f, urgency);

        if (SeasonManager.Instance.IsWinter)
            dangerWeight *= 0.7f; // Wolves are bolder in winter when food is scarce

        for (int x = 0; x < memory.GetGridSizeX(); x++)
        {
            for (int z = 0; z < memory.GetGridSizeZ(); z++)
            {
                float prey = memory.GetPreyValue(x, z);
                float food = memory.GetFoodValue(x, z); // carcasses
                float water = memory.GetWaterValue(x, z);
                float danger = memory.GetDangerValue(x, z);

                float distance = Vector2.Distance(
                    new Vector2(x, z),
                    new Vector2(currentChunk.x, currentChunk.y)
                );

                float foodReward = (prey + food) * hungerWeight;
                float waterReward = water * thirstWeight;
                float reward = foodReward + waterReward;

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
            return new Vector2Int(-1, -1);

        return bestChunk;
    }

    void DecideFightOrFlee()
    {
        if (pack == null || pack.countCurrentPackSize() < DefendThreshold)
        {
            ChangeState(State.Fleeing);
            return;
        }

        AlertPackDefend();
        ChangeState(State.Defend);
    }

    void AlertPackDefend()
    {
        if (pack == null) return;

        foreach (Wolf member in pack.members)
        {
            if (member == null || member == wolf) continue;

            WolfBehaviour wb = member.GetComponent<WolfBehaviour>();
            if (wb != null)
            {
                wb.JoinDefendAgainst(enemy);
            }
        }
    }

    void JoinDefendAgainst(GameObject bear)
    {
        if (bear == null) return;
        enemy = bear;
        ChangeState(State.Defend);
    }

    protected override void DefendState()
    {
        agent.isStopped = false;
        agent.speed = animal.runningSpeed;
    }

    protected override void UpdateDefend()
    {
        // If there is no enemy, switch back to wandering
        if (enemy == null)
        {
            ChangeState(State.Wander);
            return;
        }

        BearBehaviour bear = enemy.GetComponentInParent<BearBehaviour>();
        if (bear != null && bear.isDead)
        {
            enemy = null;
            agent.speed = animal.speed;
            ChangeState(State.Wander);
            return;
        }

        // If the pack is too small, switch to fleeing instead of defending
        if (pack == null || pack.countCurrentPackSize() < DefendThreshold)
        {
            ChangeState(State.Fleeing);
            return;
        }

        float distanceToBear = Vector3.Distance(transform.position, enemy.transform.position);

        if (distanceToBear < attackRange)
        {
            agent.isStopped = true;
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                anim.SetTrigger("Attack");
                DamageBear();
                attackTimer = 0;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(enemy.transform.position);
        }

    }

    void DamageBear()
    {
        if (enemy != null)
        {
            BearBehaviour bear = enemy.GetComponentInParent<BearBehaviour>();
            if (bear != null && !bear.isDead)
            {
                bear.InflictDamage(animal.attackDamage);
            }
        }
    }

    public override void OnDeath()
    {
        bool bearKill = bearAttackers.Count > 0;

        foreach (BearBehaviour bear in bearAttackers.ToList())
        {
            if (bear != null)
            {
                bear.notifyDeath();
            }
        }

        bearAttackers.Clear();

        if (bearKill)
            StatisticsTableManager.instance.BearSuccessfulHuntsCount++;

        if (wolf.isLeader && pack != null)
        {
            pack.OnLeaderDeath();
        }
        else if (pack != null)
        {
            pack.members.Remove(wolf);
        }

        wolf.pack = null;
        wolf.isLeader = false;

        base.OnDeath();
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
