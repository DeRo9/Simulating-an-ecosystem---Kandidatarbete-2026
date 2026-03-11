using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class WolfBehaviour : AnimalBehaviour
{

    AnimalFOV fov;
    WolfHearing hearing;

    GameObject preyTarget;

    
    float huntCooldown = 5f; // Time the wolf must wait after giving up on a hunt before it can hunt again
    [Header("Hunting")]
    [SerializeField]
    float huntCooldownTimer = 0;

    public GameObject CurrentTarget => preyTarget;
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

    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<WolfHearing>();
    }


    protected override void Update()
    {
        base.Update();

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
            Debug.Log("Wolf heard: " + hearing.HeardAnimal.name);
            // ChangeState(State.Idle); testing
        }
        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Hunt)
        {
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

    protected override void UpdateHunt()
    {

        if (preyTarget == null)
        {
            ChangeState(State.Wander);
            return; // If the wolf has no prey, switch to wandering
        }

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if (moose != null && moose.isDead)
        {
            notifyDeath();
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
        //StopAttack(); // Stop attacking if the prey is lost

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
        }

        if (hasArrived())
        {
            agent.isStopped = true;
            needs.Eat(100);
            Destroy(foodTarget);
            foodTarget = null;
            ChangeState(State.Wander);
        }

        if (!needs.isHungry)
        {
            foodTarget = null;
            ChangeState(State.Wander);
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
        pendingCarcass = preyTarget;
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

}
