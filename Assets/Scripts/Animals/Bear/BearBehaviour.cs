using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : AnimalBehaviour
{
    GameObject foodTarget;
    BearHearing hearing;
    AnimalFOV fov;
    GameObject preyTarget;
    public GameObject CurrentTarget => preyTarget;
    float attackRange = 3.5f; // Range within which the wolf can attack prey

    float huntCooldown = 5f; // Time the wolf must wait after giving up on a hunt before it can hunt again
    [Header("Hunting")]
    [SerializeField]
    float huntCooldownTimer = 0;

    float repathTimer = 0f;
    float repathInterval = 0.3f; // Time interval for recalculating path to prey

    float attackTimer = 0f;
    float attackInterval = 1f; // Time interval for attacking, to prevent multiple attacks in quick succession

    float foodSearchingCooldown;

    float deathWaitTimer = 0f;
    float deathWaitDuration = 2f; // Timer to wait before eating, otherwise the moose will instantly be eaten (Not leting the animation finish).
    bool waitingForDeathAnimation = false;

    GameObject pendingCarcass;

    [Header("Layers")]
    [SerializeField]
    LayerMask foodLayer;

    [Header("Layers")]
    [SerializeField]
    LayerMask carcassLayer;

    [SerializeField]
    LayerMask PreyLayer;

    protected override void Start()
    {
        base.Start();
        hearing = GetComponent<BearHearing>();
        fov = GetComponent<AnimalFOV>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

        if (CurrentState == State.Pregnant)
            return;

        // Update animation based on movement
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude <= 3.2f); // "isWalking" är en bool i animator
        anim.SetBool("isRunning", agent.velocity.magnitude > 3.5f); // "isRunning" är en bool i animator

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
            Debug.Log("Bear heard: " + hearing.HeardAnimal.name);
        }
        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Hunt && CurrentState != State.Fleeing)
        {

            // If the bear is more thirsty than hungry, switch to drink state, if more hungry than thirsty, switch to eat state
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
                else if (FindFood())
                    ChangeState(State.Eat);
            }
        }

        if (huntCooldownTimer > 0)
            huntCooldownTimer -= Time.deltaTime;

    }

    // Finds the closest food item within the detection radius and sets it as the target
    bool FindFood()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange, foodLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {

            Debug.Log("Bear found plant.");
            if (hit.CompareTag("Plant"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFood = hit.gameObject;
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

    bool FindPrey()
    {

        if (preyTarget != null)
            return true; // If the wolf already has a target, don't change prey

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange);

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

            if (hit.CompareTag("Wolf"))
            {
                WolfBehaviour wolf = hit.GetComponentInParent<WolfBehaviour>();
                if (wolf == null || wolf.isDead) continue; // Skip if moose is already dead.

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
            StatisticsTableManager.instance.BearhuntAttemptsCount++;

            MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
            if (moose != null)
            {
                moose.OnBeingHunted(gameObject); // Notify the moose that it is being hunted
            }

            WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
            if(wolf != null)
            {
                wolf.OnBeingHunted(gameObject); // Notify the wolf that it is being hunted
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

    protected override void UpdateHunt()
    {

        if (preyTarget == null)
        {
            ChangeState(State.Wander);
            return; // If the bear has no prey, switch to wandering
        }

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
            if (!fov.IsInFOV(preyTarget.transform))
            {
                LostPrey(); // If the prey is no longer in the bear's field of view, give up
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
            LostPrey(); // If the bear has no more stamina to run after, give up
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
                anim.SetTrigger("Attack");
                DamageTarget();
                attackTimer = 0f;
                Debug.Log("Bear attacked prey");
            }
        }
    }

    public void DamageTarget()
    {
        if (preyTarget == null) return;

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null)
        {
            moose?.InflictDamage(animal.attackDamage);
        }

        WolfBehaviour wolf = preyTarget.GetComponentInParent<WolfBehaviour>();
        if (wolf != null)
        {
            wolf?.InflictDamage(animal.attackDamage);
        }
    }

    void LostPrey()
    {
        StatisticsTableManager.instance.BearhuntFailuresCount++;

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

    protected override void UpdateWander()
    {
        if (hasArrived()) // If the Bear has reached its destination, switch to idle state
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



        // If the Bear has reached the food, stop moving
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
            else if(foodTarget.CompareTag("Plant"))
            {
                needs.Eat(100);
                Destroy(foodTarget);
                foodTarget = null;
                Debug.Log("Bear ate.");
                ChangeState(State.Wander);
                return;
            }
        }

        // If the Bear is no longer hungry, stop eating and switch back to wandering
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
                Debug.Log("Bear found carcass.");
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

        if (carcass != null)
        {
            return carcass.gameObject;
        }

        return obj;

    }


}


