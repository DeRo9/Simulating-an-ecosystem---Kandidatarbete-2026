using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : AnimalBehaviour
{
    [Header("Pack Threat")]
    float packCheckCooldown = 0f;
    float packCheckInterval = 2f;
    int dangerousPackSize = 7;

    BearHearing hearing;
    AnimalFOV fov;
    GameObject preyTarget;
    public GameObject CurrentTarget => preyTarget;
    float attackRange = 3.5f;

    [Header("Layers")]
    [SerializeField] LayerMask foodLayer;

    float huntCooldown = 5f;
    [Header("Hunting")]
    [SerializeField]
    float huntCooldownTimer = 0;

    float repathTimer = 0f;
    float repathInterval = 0.3f;

    float foodSearchingCooldown;
    float needsEvalCooldown = 0f;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f;
    bool waitingForDeathAnimation = false;

    GameObject pendingCarcass;


    [Header("Layers")]
    [SerializeField]
    LayerMask PreyLayer;

    Bear bear;

    protected override void Start()
    {
        base.Start();
        hearing = GetComponent<BearHearing>();
        fov = GetComponent<AnimalFOV>();
        bear = GetComponent<Bear>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        if (SeasonManager.Instance.IsWinter && CurrentState != State.Hibernate)
        {
            ChangeState(State.Hibernate);
            needs.hibernationMultiplier = 0.1f;
            return;
        }
        else if (!SeasonManager.Instance.IsWinter && CurrentState == State.Hibernate)
        {
            needs.hibernationMultiplier = 1f;
            anim.SetBool("isSleeping", false);
            ChangeState(State.Wander);
            return;
        }

        if (CurrentState == State.Hibernate)
            return;

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f); // "isWalking" är en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f); // "isRunning" är en bool i animator

        if (waitingForDeathAnimation)
        {
            deathWaitTimer -= Time.deltaTime;
            if (deathWaitTimer <= 0f)
            {
                waitingForDeathAnimation = false;
                preyTarget = null;
                pendingCarcass = null;
                if (CurrentState != State.SearchFood)
                {
                    ChangeState(State.SearchFood);
                }
            }
            return;
        }

        if (CheckForThreats()) return;

        if (huntCooldownTimer > 0)
            huntCooldownTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case State.Hunt:
            case State.Eat:
            case State.Drink:
            case State.SearchFood:
            case State.SearchWater:
            case State.Mating:
            case State.Defend:
            case State.Fleeing:
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
        packCheckCooldown -= Time.deltaTime;
        if (packCheckCooldown > 0f) return false;
        packCheckCooldown = packCheckInterval;

        Collider[] nearby = Physics.OverlapSphere(transform.position, animal.sightRange);

        foreach (Collider hit in nearby)
        {
            if (!hit.CompareTag("Wolf")) continue;
            if (fov != null && !fov.IsInFOV(hit.transform)) continue;

            Wolf wolfComp = hit.GetComponentInParent<Wolf>();
            if (wolfComp == null || wolfComp.pack == null) continue;

            WolfBehaviour wolfBehaviour = hit.GetComponentInParent<WolfBehaviour>();
            if (wolfBehaviour == null) continue;

            // Wolves are hunting the bear, fight back
            if (wolfBehaviour.CurrentTarget == gameObject)
            {
                Debug.Log("Bear is being hunted, fighting back!");
                if (memory != null)
                    memory.RememberDanger(transform.position);

                preyTarget = hit.gameObject;
                ChangeState(State.Hunt);
                return true;
            }

            if (wolfComp.pack.countCurrentPackSize() >= dangerousPackSize)
            {
                Debug.Log("Bear detected wolf pack of " + wolfComp.pack.countCurrentPackSize());
                if (memory != null)
                    memory.RememberDanger(transform.position);

                if (needs.howHungryInPercent < 0.25f)
                {
                    Debug.Log("Bear is starving, attacking the pack");
                    preyTarget = hit.gameObject;
                    ChangeState(State.Hunt);
                }
                else
                {
                    Debug.Log("Bear flees from large pack");
                    enemy = hit.gameObject;
                    ChangeState(State.Fleeing);
                }
                return true;
            }
        }
        return false;
    }

    private void EvaluateNeeds()
    {
        bool hungry = IsHungry();
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

    private Collider[] hits = new Collider[10];

    bool FindFood()
    {

        if (foodTarget != null)
        {
            return true;
        }

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

            if (hit == null) continue;

            IsEdible edible = hit.GetComponent<IsEdible>();
            if (edible == null || !edible.CanBeEatenBy(animal.species))
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

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrey = hit.GetComponentInParent<MooseBehaviour>()?.gameObject;
                }
            }

            if (hit.CompareTag("Wolf"))
            {
                WolfBehaviour wolf = hit.GetComponentInParent<WolfBehaviour>();
                if (wolf == null || wolf.isDead) continue;

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPrey = hit.GetComponentInParent<WolfBehaviour>()?.gameObject;
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

            MooseBehaviour moosePrey = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moosePrey != null)
            {
                moosePrey.RegisterBearAttacker(this);
                moosePrey.OnBeingHunted(gameObject);
            }

            WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
            if (wolf != null)
            {
                wolf.RegisterBearAttacker(this);
                wolf.OnBeingHunted(gameObject);
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

        agent.speed = needs.noMoreStamina ? animal.speed : animal.runningSpeed;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && moose.isDead)
        {
            notifyDeath();
            return;
        }

        WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
        if (wolf != null && wolf.isDead)
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
            return;
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
                DamageTarget();
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
            moose?.InflictDamage(bear.CalculateAttackDamage());
        }

        WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
        if (wolf != null && !wolf.isDead)
        {
            wolf?.InflictDamage(bear.CalculateAttackDamage());
        }
    }

    void LostPrey()
    {
        if (preyTarget != null)
        {
            StatisticsTableManager.instance.BearhuntFailuresCount++;

            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.UnregisterBearAttacker(this);
                moose.OnNoLongerHunted(gameObject);
            }


            WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
            if (wolf != null)
            {
                wolf.UnregisterBearAttacker(this);
                wolf.OnNoLongerHunted(gameObject);
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

        if (foodTarget == null)
        {
            if (FindPrey())
            {
                ChangeState(State.Hunt);
                return;
            }   
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

    protected override void UpdateEat()
    {
        if (foodTarget == null)
        {
            ChangeState(State.SearchFood);
            return;
        }

        agent.isStopped = true;
        eatingTimer += Time.deltaTime;
        if (eatingTimer >= eatingDuration)
        {
            Carcass carcass = foodTarget.GetComponentInParent<Carcass>();
            if (carcass != null)
            {
                float nutrition = carcass.Consume();
                needs.Eat(nutrition);
            }
            else
            {
                IsEdible edible = foodTarget.GetComponent<IsEdible>();
                
                if (edible != null && edible.CanBeEatenBy(animal.species))
                {
                    float nutrition = edible.Consume();
                    needs.Eat(nutrition);
                    StatisticsTableManager.instance.BearPlantMealsCount++;

                }
            }
    
            eatingTimer = 0f;
            agent.isStopped = false;
            foodTarget = null;

            ChangeState(needs.isHungry ? State.SearchFood : State.Wander);
        }
    }

    public void notifyDeath()
    {
        if (preyTarget == null) return;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null)
        {
            moose.UnregisterBearAttacker(this);
        }

        WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
        if (wolf != null)
        {
            wolf.UnregisterBearAttacker(this);
        }

        pendingCarcass = preyTarget.GetComponentInParent<AnimalBehaviour>().gameObject;
        foodTarget = pendingCarcass;

        preyTarget = null;
        agent.isStopped = true;
        waitingForDeathAnimation = true;
        deathWaitTimer = deathWaitDuration;

    }

    protected override void HibernationState()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        anim.SetBool("isSleeping", true);
    }

    public override void InflictDamage(float damage)
    {
        needs.TakeDamage(damage);
    }
}