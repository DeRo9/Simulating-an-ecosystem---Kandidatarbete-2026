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
    float foodSearchingCooldown;
    float needsEvalCooldown = 0f;

    private float nonLeaderNeedsEvalCooldown = 0f;
    [SerializeField] private float nonLeaderNeedsEvalInterval = 3f;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f; 
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

    public bool breakFormation = false;

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<AnimalHearing>();
        wolf = GetComponent<Wolf>();
        if (wolf != null)
            pack = wolf.pack;
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

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f);
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f);

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
        }

        if (CheckForThreats()) return;

        if (huntCooldownTimer > 0)
        {
            huntCooldownTimer -= Time.deltaTime;
        }

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


        if (!wolf.isLeader)
        {
            if (HasCriticalNeed())
            {
                EvaluateNeeds();
                return;
            }
 
            nonLeaderNeedsEvalCooldown -= Time.deltaTime;
            if (nonLeaderNeedsEvalCooldown <= 0f)
            {
                nonLeaderNeedsEvalCooldown = nonLeaderNeedsEvalInterval;
                
                if (CanMate())
                {
                    breakFormation = true;
                }
                else
                {
                    breakFormation = false;
                }
                
                if (CurrentState == State.Idle || CurrentState == State.Wander)
                {
                    EvaluateNeeds();
                }
            }
 
            if (pack != null && wolf != null && pack.leader != null)
            {
                FollowLeader();
                return;
            }
        }
        
        if (wolf.isLeader)
        {
            needsEvalCooldown -= Time.deltaTime;
            if (needsEvalCooldown <= 0f)
            {
                needsEvalCooldown = 1f;
                EvaluateNeeds();
            }
        }

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


        if (!wolf.isLeader && breakFormation && CanMate())
        {
            if (pack.countCurrentPackSize() < 3)
            {
                ChangeState(State.SearchMate);
                return;
            }
        }

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

    private bool HasCriticalNeed()
    {
        if (needs.howThirstyInPercent < 0.2f)
            return true;
        
        if (needs.howHungryInPercent < 0.15f)
            return true;
        
        return false;
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
            ChangeState(State.SearchWater);
            return;
        }

        if (hungry)
        {
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
        if (huntCooldownTimer > 0f) return false;
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

        if (isDead) return;

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

            BearBehaviour bear = preyTarget.GetComponentInParent<BearBehaviour> ();
            if ( bear != null && bear.isDead)
            {
                notifyDeath();
                return;
            }
        }

        if (preyTarget == null)
        {
            if (waitingForDeathAnimation && deathWaitTimer > 0f)
                return;
 
            ChangeState(State.SearchFood);
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
        if (isDead) return;
        if (preyTarget == null) return;


        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && !moose.isDead)
        {
            moose?.InflictDamage(wolf.CalculateAttackDamage());
        }

        BearBehaviour bear = preyTarget.GetComponentInParent<BearBehaviour>();
        if (bear != null && !bear.isDead)
        {
            bear.RegisterWolfAttacker(this);
            bear?.InflictDamage(wolf.CalculateAttackDamage());
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

            BearBehaviour bear = preyTarget.GetComponentInParent<BearBehaviour>();
            if (bear != null)
            {
                bear.UnregisterWolfAttacker(this);
            }
        }

        preyTarget = null;
        huntCooldownTimer = huntCooldown;
        agent.speed = animal.speed;
        ChangeState(State.Wander);
    }

    protected override void UpdateSearchFood()
    {

        if (foodTarget == null) 
        {
            FindFood();
        }

        if (foodTarget != null)
        {
            if (hasArrived())
            {
                ChangeState(State.Eat);
                return;
            }
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
            memoryDecisionCooldown = 4f;

            if (Random.value < 0.2f)
            {
                agent.SetDestination(GetRandomPoints());
            }
            else
            {
                Vector2Int targetChunk = memory.GetBestPreyChunk();

                if (targetChunk.x != -1)
                {
                    Vector3 targetPos = memory.GetRandomPointInChunk(targetChunk);
                    agent.SetDestination(targetPos);
                }
                else
                {
                    targetChunk = DecideFoodAndWaterTargetChunk();
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
        }
        else if (hasArrived())
        {
            memoryDecisionCooldown = 0f;
        }
    }

    private Collider[] hits = new Collider[10];
    bool FindFood()
    {

        if (foodTarget != null)
        {
            return true;
        }

        if (foodSearchingCooldown > 0f && foodTarget != null)
        {
            foodSearchingCooldown -= Time.deltaTime;
            return true;
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
            IsEdible edible = hit.GetComponent<IsEdible>();
            if (edible == null || !edible.CanBeEatenBy(animal.species))
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
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(closestFood.transform.position);
            }
            return true;
        }

        foodTarget = null;
        return false;
    }

    protected override void UpdateEat()
    {
        if (foodTarget == null)
        {
            ChangeState(State.SearchFood);
            return;
        }

        Carcass carcass = foodTarget.GetComponent<Carcass>();

        if (carcass == null)
        {
            foodTarget = null;
            ChangeState(State.SearchFood);
            return;
        }

        agent.isStopped = true;
        eatingTimer += Time.deltaTime;
        if (eatingTimer >= eatingDuration)
        {
            needs.Eat(carcass.Consume());
            StatisticsTableManager.instance.WolfCarcassCount++;
            eatingTimer = 0f;

            if (carcass.IsEmpty || foodTarget == null)
            {
                foodTarget = null;
                agent.isStopped = false;

                if (needs.isHungry)
                    ChangeState(State.SearchFood);
                else
                    ChangeState(State.Wander);
                return;
            }

            if (!needs.isHungry)
            {
                foodTarget = null;
                agent.isStopped = false;
                ChangeState(State.Wander);
            }
        }
    }

    public void notifyDeath()
    {
        if (isDead) return;
        if (preyTarget == null) return;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null)
        {
            moose.UnregisterWolfAttacker(this);
        }

        BearBehaviour bear = preyTarget.GetComponentInParent<BearBehaviour>();
        if (bear != null)
        {
            bear.UnregisterWolfAttacker(this);
        }
        pendingCarcass = preyTarget.GetComponentInParent<AnimalBehaviour>().gameObject;

        preyTarget = null;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
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
            preyTarget = null;
            ChangeState(State.Wander);
            return;
        }

        BearBehaviour bear = enemy.GetComponentInParent<BearBehaviour>();
        if (bear != null && bear.isDead)
        {
            enemy = null;
            preyTarget = null;
            agent.speed = animal.speed;
            ChangeState(State.Wander);
            return;
        }

        if (pack == null || pack.countCurrentPackSize() < DefendThreshold)
        {
            ChangeState(State.Fleeing);
            return;
        }

        preyTarget = enemy;

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
    public override void OnDeath(bool killedByPredator = false)
    {
        if (preyTarget != null)
        {
            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.UnregisterWolfAttacker(this);
            }
            preyTarget = null;
        }

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

        base.OnDeath(killedByPredator: bearKill);
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