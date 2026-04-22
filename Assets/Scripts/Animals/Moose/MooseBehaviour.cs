using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class MooseBehaviour : AnimalBehaviour
{

    AnimalFOV fov;
    AnimalHearing hearing;

    Moose moose;
    float foodSearchingCooldown;

    float memoryDecisionCooldown = 0f;

    private float needsEvalCooldown = 0f;

    [Header("Avoidance")]
    float avoidanceCheckCooldown = 0f;
    float avoidanceCheckInterval = 2f;
    float avoidanceRange = 25f;

    [Header("Layers")]
    [SerializeField] LayerMask foodLayer;

    private List<WolfBehaviour> wolfAttackers = new List<WolfBehaviour>();
    private List<BearBehaviour> bearAttackers = new List<BearBehaviour>();

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<AnimalHearing>();
        moose = GetComponent<Moose>();
    }

    protected override void Update()
    {
        if (!agent.isOnNavMesh)
            return;
        
        base.Update();

        if (isDead) return;

        if (CheckForThreats()) return;

        
        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animal.runningSpeed * 0.95f); // "isWalking" Ã¤r en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > animal.runningSpeed * 0.95f); // "isRunning" Ã¤r en bool i animator

        switch (CurrentState)
        {
            case State.Fleeing:
            case State.Mating:
            case State.Eat:
            case State.Drink:
            case State.SearchFood:
            case State.SearchWater:
            case State.Defend:
                return;
        }

        AvoidPredators();
        needsEvalCooldown -= Time.deltaTime;
        if (needsEvalCooldown <= 0f)
        {
            needsEvalCooldown = 2f;
            EvaluateNeeds();
        }
    }

    private bool CheckForThreats()
    {
        if (isDead) return false;

        if (hearing == null || !hearing.HeardSomething) return false;

        Animal heard = hearing.HeardAnimal;
        if (heard == null) return false;

        if (heard.species != Species.bear && heard.species != Species.wolf) return false;

        memory.RememberDanger(heard.transform.position);

        WolfBehaviour wolf = heard.GetComponent<WolfBehaviour>();
        if (wolf != null && wolf.CurrentTarget == gameObject)
        {
            enemy = heard.gameObject;
            ChangeState(State.Fleeing);
            return true;
        }

        BearBehaviour bear = heard.GetComponent<BearBehaviour>();
        if (bear != null && bear.CurrentTarget == gameObject)
        {
            enemy = heard.gameObject;
            ChangeState(State.Fleeing);
            return true;
        }
        return false;
    }

    private void EvaluateNeeds()
    {
        bool hungry  = IsHungry();
        bool thirsty = IsThirsty();

        if (thirsty && (!hungry || needs.howThirstyInPercent < needs.howHungryInPercent))
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
            else
                ChangeState(State.SearchFood);
            return;
        }

        if (mating != null && CanMate())
        {
            ChangeState(State.SearchMate);
            return;
        }

        if (CurrentState != State.Wander && CurrentState != State.Idle)
        {
            ChangeState(State.Wander);
        }
    }
    protected override void UpdateSearchFood()
    {
        if (FindFood())
        {
            ChangeState(State.Eat);
            return;
        }

        memoryDecisionCooldown -= Time.deltaTime;

        if (memoryDecisionCooldown <= 0f && hasArrived())
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

                    if (UnityEngine.Random.value < 0.2f) // 20% chance for exploration instead of memory
                    {
                        ChangeState(State.Wander);
                        return;
                    }     
                }
            }
        }
    }

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

            if (hit == null) continue;
            if (!hit.CompareTag("Plant")) continue;
            if (!fov.IsInFOV(hit.transform)) continue;
            
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

        foodTarget = null;
        return false;
    }

    public void OnBeingHunted(GameObject predator)
    {
        enemy = predator;
        ChangeState(State.Fleeing);
    }

    public override void InflictDamage(float damage)
    {
        if (isDead) return;

        base.InflictDamage(damage);

        if (animal.age < animal.grownUpAge) return; // Calves should not attack back
        if (wolfAttackers.Count >= 5) return; // If there are multiple wolves attacking, the moose should focus on escaping rather than fighting back

        if (CurrentState == State.Fleeing) // Only fight back if there is under 5 wolf attacking, if there are more, focus on escaping
            ChangeState(State.Defend);
    }

    protected override void DefendState()
    {
        agent.isStopped = false;
        agent.speed = animal.runningSpeed;
    }

    protected override void UpdateDefend()
    {
        if(wolfAttackers.Count >= 5) // More than 5 wolves attacking, focus on fleeing
        {
            ChangeState(State.Fleeing);
            return;
        }

        if (enemy == null || wolfAttackers.Count == 0) // No more attackers, go back to normal behavior
        {
            ChangeState(State.Wander);
            return;
        }

        WolfBehaviour wolf = enemy.GetComponentInParent<WolfBehaviour>();
        if (wolf != null && wolf.isDead) 
        {
            enemy = null;
            ChangeState(State.Wander);
            return;
        }


        float distanceToWolf = Vector3.Distance(transform.position, enemy.transform.position);

        if (distanceToWolf < 3.5f)  // Hard coded attack range
        { 
            agent.isStopped = true;
            attackTimer += Time.deltaTime;

            if(attackTimer >= attackInterval)
            {
                anim.SetTrigger("Attack");
                DamageTarget();
                attackTimer = 0f;
            }
        } else
        {
            agent.isStopped = false;
            agent.SetDestination(enemy.transform.position);
        }

    }

    public void DamageTarget()
    {
        if (isDead) return;
        if (enemy == null) return;

        WolfBehaviour wolf = enemy.GetComponentInParent<WolfBehaviour>();
        if (wolf != null && !wolf.isDead)
        {
            wolf?.InflictDamage(moose.CalculateAttackDamage());
        }
    }

    public void OnNoLongerHunted(GameObject predator)
    {
        if(enemy == predator)
        {
            StatisticsTableManager.instance.MooseSuccessfulEscapeCount++;

            enemy = null;
            agent.speed = animal.speed;
            if (CurrentState == State.Fleeing)
            {
                ChangeState(State.Wander);
            }
        }
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

    public override void OnDeath(bool killedByPredator = false)
    {
        bool wolfKill = wolfAttackers.Count > 0;
        bool packKill = wolfAttackers.Exists(w => {
            Wolf wolfComp = w?.GetComponent<Wolf>();
            return wolfComp != null && wolfComp.pack != null && wolfComp.pack.countCurrentPackSize() > 1;
        });

        foreach (WolfBehaviour wolf in wolfAttackers.ToList())
        {
            if(wolf != null)
            {
                wolf.notifyDeath(); 
            }
        }

        wolfAttackers.Clear();

        if (wolfKill)
        {
            StatisticsTableManager.instance.WolfSuccessfulHuntsCount++;
            if (packKill) StatisticsTableManager.instance.PackHuntSuccessCount++;
        }

        bool bearKill = bearAttackers.Count > 0; 
        
        foreach(BearBehaviour bear in bearAttackers.ToList())
        {
            if (bear != null)
            {
                bear.notifyDeath();
            }
        }

        bearAttackers.Clear();
        if (bearKill) 
            StatisticsTableManager.instance.BearSuccessfulHuntsCount++;

        base.OnDeath(killedByPredator: wolfKill || bearKill);
    }

    void AvoidPredators()
    {

        if (isDead) return;
        avoidanceCheckCooldown -= Time.deltaTime;
        if(avoidanceCheckCooldown > 0) return;

        avoidanceCheckCooldown = avoidanceCheckInterval;

        Collider[] nearby = Physics.OverlapSphere(transform.position,avoidanceRange);
        Vector3 avoidDirection = Vector3.zero;
        int threatCount = 0;

        foreach(Collider hit in nearby)
        {
            if(!hit.CompareTag("Wolf") && !hit.CompareTag("Bear")) continue;
            AnimalBehaviour predator = hit.GetComponentInParent<AnimalBehaviour>();
            if(predator == null || predator.isDead ) continue;

            if(enemy != null && hit.transform.IsChildOf(enemy.transform)) continue; // If the animal is hunted this function shouldnt be called
            float distance = Vector3.Distance(transform.position,hit.transform.position);
            if(distance < avoidanceRange)
            {
                Vector3 awayFromThreat = (transform.position - hit.transform.position).normalized;
                float urgency = 1f - (distance / avoidanceRange); // The closer the threats get the farther the animal goes
                avoidDirection += awayFromThreat * urgency;
                threatCount ++;
            }
        }
        if (threatCount > 0 && CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Fleeing)
            {
                Vector3 avoidTarget = transform.position + avoidDirection.normalized * 15f;
                
                NavMeshHit hit;
                if(NavMesh.SamplePosition(avoidTarget,out hit, 15f, NavMesh.AllAreas))
                {
                    if (agent.isOnNavMesh)
                    {
                        agent.SetDestination(hit.position);
                        if (CurrentState == State.Idle)
                        {
                            ChangeState(State.Wander);
                        }
                    }

                }
                if (memory != null && threatCount >= 3)
            {
                memory.RememberDanger(transform.position);
            }
            }    
    }
    public float GetAge() { return animal.age; }
    public float GetGrownUpAge() { return animal.grownUpAge; }
}