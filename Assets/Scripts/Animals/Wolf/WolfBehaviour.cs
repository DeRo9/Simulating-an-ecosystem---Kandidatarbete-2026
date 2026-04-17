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
    float huntCooldown = 6f;
    [Header("Hunting")]
    [SerializeField]
    float huntCooldownTimer = 0;

    float attackRange = 3.5f; 

    float repathTimer = 0f;
    float repathInterval = 0.5f;

    float attackTimer = 0f;
    float attackInterval = 1f;

    float foodSearchingCooldown;
    float needsEvalCooldown = 0f;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f; 

    float memoryDecisionCooldown = 0f;

    bool waitingForDeathAnimation = false;

    const int DefendThreshold = 5;

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

    Vector3 followOffset = new Vector3(0, 0, -5f);

    void FollowLeader()
    {
        if (pack == null || pack.leader == null)
            return;

        WolfBehaviour leaderBehaviour = pack.leader.GetComponent<WolfBehaviour>();

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
            if (preyTarget != leaderBehaviour.preyTarget) 
            {  
                preyTarget = leaderBehaviour.preyTarget;

                MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
                if (moose != null)
                {
                    moose.RegisterWolfAttacker(this);
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
            agent.isStopped = true;
        }

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f);
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f);

        ApplySeperation();
    }

    public void ApplySeperation()
    {
        if (pack == null || pack.members == null) return;

        Vector3 separation = Vector3.zero;
        foreach (Wolf member in pack.members)
        {
            if (member != null && member != wolf)
            {
                float distance = Vector3.Distance(transform.position, member.transform.position);

                if (distance < 0.0001f) continue;
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
                preyTarget = null;
                if (CurrentState != State.SearchFood)
                {
                    ChangeState(State.SearchFood);
                }
            }
            return;
        }

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
                if (moose == null || moose.isDead) continue;
                if ((pack == null || pack.countCurrentPackSize() <= 1) && moose.GetAge() >= moose.GetGrownUpAge()) continue;

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
            agent.speed = animal.runningSpeed;
            agent.SetDestination(preyTarget.transform.position); 
        }
        else
        {
            agent.speed = animal.speed;
            ChangeState(State.Wander);
        }
    }

    public void OnBeingHunted(GameObject predator)
    {
        if (memory != null)
        {
            memory.RememberDanger(transform.position);
        }
        if (CurrentState == State.Hunt)
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
            agent.speed = animal.speed; 
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

        if (preyTarget != null)
        {
            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null && moose.isDead)
            {
                notifyDeath();
                return;
            }
        }

        if (preyTarget == null)
        {
            if (waitingForDeathAnimation && deathWaitTimer <= 0f)
            {
                waitingForDeathAnimation = false;
            }

            if (waitingForDeathAnimation)
                return;
            
            ChangeState(State.Wander); 
            return;
        }

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

    public void DamageTarget()
    {
        if (preyTarget == null) return;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && !moose.isDead)
        {
            moose?.InflictDamage(animal.attackDamage);
        }

        BearBehaviour bear = preyTarget.GetComponentInParent<BearBehaviour>();
        if (bear != null && !bear.isDead)
        {
            bear?.InflictDamage(animal.attackDamage);
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
                moose.OnNoLongerHunted(gameObject);
            }
        }

        preyTarget = null;
        huntCooldownTimer = huntCooldown;
        agent.speed = animal.speed;
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
                Vector2Int targetChunk = DecideFoodAndWaterTargetChunk();

                if (targetChunk.x != -1)
                {
                    Vector3 targetPos = memory.GetRandomPointInChunk(targetChunk);
                    agent.SetDestination(targetPos);
                    needs.Eat(nutrition);
                    needs.RegenerateHealth(20f); // Regenerate some health upon eating carcass
                }
                else
                {
                    agent.SetDestination(GetRandomPoints());
                }
            }
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
                closestFood = hit.gameObject;
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

        preyTarget = null;
        agent.isStopped = true;

        waitingForDeathAnimation = true;
        deathWaitTimer = deathWaitDuration;

        if (pendingCarcass != null)
        {
           foodTarget = pendingCarcass;
        }
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
                DamageTarget();
                attackTimer = 0;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(enemy.transform.position);
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