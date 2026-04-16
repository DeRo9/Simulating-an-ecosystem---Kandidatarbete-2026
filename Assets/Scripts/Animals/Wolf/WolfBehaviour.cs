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
    float repathInterval = 0.5f; // Time interval for recalculating path to prey (increased from 0.3f for smoother movement)

    float attackTimer = 0f;
    float attackInterval = 1f; // Time interval for attacking, to prevent multiple attacks in quick succession

    float foodSearchingCooldown;
    float needsEvalCooldown = 0f;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f; // Timer to wait before eating, otherwise the moose will instantly be eaten (Not leting the animation finish).

    float memoryDecisionCooldown = 0f;

    bool waitingForDeathAnimation = false;

    const int DefendThreshold = 5; // If the pack has 5 or more members, they will choose to defend against bears instead of fleeing

    GameObject pendingCarcass;

    private List<BearBehaviour> bearAttackers = new List<BearBehaviour>();

    [Header("Layers")]
    [SerializeField]
    LayerMask PreyLayer;

    [Header("Pack")]
    Wolf wolf;
    WolfPackManager pack;

    public GameObject preyTarget;

    public GameObject CurrentTarget => preyTarget;

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
                if(moose != null)
                {
                    moose.RegisterWolfAttacker(this); // Register as attacker to the moose
                }


            }

            if(CurrentState != State.Hunt)
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

        if (isDead) return;

        if (waitingForDeathAnimation)
        {
            deathWaitTimer -= Time.deltaTime;
            if (deathWaitTimer <= 0f)
            {
                waitingForDeathAnimation = false;
                preyTarget = null; // Clear prey target
                ChangeState(State.SearchFood); // Go search for food (finds the carcass)
            }
            return;
        }

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < 3f);
        anim.SetBool("isRunning", agent.velocity.magnitude > 3f);

        if (pack != null && wolf != null && !wolf.isLeader && pack.leader != null)
        {
            if (CurrentState == State.Idle || CurrentState == State.Wander)
            {
                FollowLeader();
                return;
            }
        }

        if (CheckForThreats()) return;

        switch (CurrentState)
        {
            case State.Hunt:
            case State.Eat:
            case State.Drink:
            case State.SearchFood:
            case State.SearchWater:
            case State.Mating:
            case State.Fleeing:
            case State.Defend:
                return;
        }

        needsEvalCooldown -= Time.deltaTime;
        if (needsEvalCooldown <= 0f)
        {
            needsEvalCooldown = 1f;
            EvaluateNeeds();
        }
    }

    private bool CheckForThreats()
    {
        if (hearing != null && hearing.HeardSomething)
        {
            Animal heard = hearing.HeardAnimal;
            if (heard != null && heard.species == Species.bear)
            {
                BearBehaviour bear = heard.GetComponent<BearBehaviour>();
                if (bear != null && bear.CurrentTarget == gameObject)
                {
                    enemy = heard.gameObject;
                    DecideFightOrFlee();
                    return true;
                }
            }
        }
        return false;
    }


    private void EvaluateNeeds()
    {
        bool hungry  = IsHungry();
        bool thirsty = IsThirsty();
        if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
        {
            if (FindWater())
                ChangeState(State.Drink);
            else
                ChangeState(State.SearchWater);
            return;
        }

        if (hungry)
        {
            if (FindFood())
                ChangeState(State.Eat);
            else if (FindPrey())
                ChangeState(State.Hunt);
            else
                ChangeState(State.SearchFood);
            return;
        }

        if (mating != null && CanMate())
        {
            if (CurrentState != State.SearchMate && CurrentState != State.Mating)
            {
                ChangeState(State.SearchMate);
            }
            return;
        }

        if (CurrentState != State.Wander && CurrentState != State.Idle)
        {
            ChangeState(State.Wander);
        }
    }

    bool FindPrey()
    {
        if (preyTarget != null) return true;

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, PreyLayer);
        float closestDistance = Mathf.Infinity;
        GameObject closestPrey = null;

        foreach (Collider hit in hits)
        {

            if (!fov.IsInFOV(hit.transform)) continue;


            if (hit.CompareTag("Moose"))
            {
                MooseBehaviour moose = hit.GetComponentInParent<MooseBehaviour>();
                if (moose == null || moose.isDead) continue; // Skip if moose is already dead.

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrey = hit.GetComponentInParent<MooseBehaviour>()?.gameObject;
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
            agent.SetDestination(preyTarget.transform.position); // Always set destination when entering hunt
        }
        else
        {
            agent.speed = animal.speed; // Reset speed to normal
            ChangeState(State.Wander); // If the prey is lost, switch to wandering
        }
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

    protected override void UpdateHunt()
    {

        if (needs.noMoreStamina)
        {
            LostPrey();
            ChangeState(State.Idle);
            return;
        }

        if (preyTarget == null)
        {
            if (waitingForDeathAnimation)
                return;
            
            ChangeState(State.Wander);
            return;
        }

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && moose.isDead)
        {
            notifyDeath();
            return;
        }

        if (preyTarget != null)
        {
            agent.speed = animal.runningSpeed;
            float distance = Vector3.Distance(transform.position, preyTarget.transform.position);

            if (distance > animal.sightRange)
            {
                LostPrey();
                return;
            }

            if (distance <= attackRange)
            {
                agent.SetDestination(preyTarget.transform.position);
                AttackOnContact();
            }
            else
            {
                agent.isStopped = false;
                repathTimer += Time.deltaTime;

                if (repathTimer >= repathInterval)
                {
                    agent.SetDestination(preyTarget.transform.position);
                    repathTimer = 0f;
                }

            }

        }

        if (needs.noMoreStamina)
        {
            LostPrey();
        }

    }

    void AttackOnContact()
    {
        if (preyTarget == null) return;

        float distance = Vector3.Distance(transform.position, preyTarget.transform.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true;

            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                anim.SetTrigger("Attack");
                attackTimer = 0f;
            }
        }
        else
        {
            agent.isStopped = false;
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

        if(wolf.isLeader || pack == null || pack.countCurrentPackSize() <= 1)
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

    protected override void UpdateSearchFood()
    {
        if (FindFood())
        {
            ChangeState(State.Eat);
            return;
        }

        if (FindPrey())
        {
            ChangeState(State.Hunt);
            return;
        }

        memoryDecisionCooldown -= Time.deltaTime;

        if (memoryDecisionCooldown <= 0f)
        {
            memoryDecisionCooldown = 2f;

            if (Random.value < 0.2f)
            {
                agent.SetDestination(GetRandomPoints());
            }
            else
            {
                Vector2Int targetChunk = DecideFoodTargetChunk();

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

    private Collider[] hits = new Collider[10];
    bool FindFood()
    {
        if (foodSearchingCooldown > 0f)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return foodTarget != null;
        }

        foodSearchingCooldown = 1.5f;

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, animal.sightRange, hits);

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

        foodTarget = null;
        return false;
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
        foodTarget = pendingCarcass; 
        preyTarget = null;
        agent.isStopped = true;
        waitingForDeathAnimation = true;
        deathWaitTimer = deathWaitDuration;

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

    Vector2Int DecideFoodTargetChunk()
    {
        if (memory == null) return new Vector2Int(-1, -1);

        float hunger = 1f - needs.howHungryInPercent;
        Vector2Int bestChunk = new Vector2Int(-1, -1);
        float bestScore = float.MinValue;
        Vector2Int currentChunk = memory.GetChunk(transform.position);

        float winterModifier = SeasonManager.Instance.IsWinter ? 0.5f : 1f;
        float dangerWeight = Mathf.Lerp(3f, 0.3f, hunger);

        for (int x = 0; x < memory.GetGridSizeX(); x++)
        {
            for (int z = 0; z < memory.GetGridSizeZ(); z++)
            {
                float prey = memory.GetPreyValue(x, z);
                float food = memory.GetFoodValue(x, z); // Carcass locations
                float danger = memory.GetDangerValue(x, z);

                if (prey <= 0f && food <= 0f)
                    continue;

                float distance = Vector2.Distance(
                    new Vector2(x, z),
                    new Vector2(currentChunk.x, currentChunk.y)
                );

                float reward = prey + food;
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

        foreach(BearBehaviour bear in bearAttackers.ToList())
        {
            if(bear != null)
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
        } else if (pack != null)
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
