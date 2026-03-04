using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class WolfBehaviour : AnimalBehaviour
{

    AnimalFOV fov;
    WolfHearing hearing;

    GameObject preyTarget;
    public GameObject CurrentTarget => preyTarget;
    float attackRange = 3.5f; // Range within which the wolf can attack prey

    float maxHuntTime = 20f; // Maximum time the wolf will spend hunting before giving up
    [SerializeField]
    float huntTime = 0;

    float huntCooldown = 5f; // Time the wolf must wait after giving up on a hunt before it can hunt again
    [SerializeField]
    float huntCooldownTimer = 0;

    float repathTimer = 0f;
    float repathInterval = 0.3f; // Time interval for recalculating path to prey

    float attackTimer = 0f;
    float attackInterval = 1f; // Time interval for attacking, to prevent multiple attacks in quick succession


    GameObject waterTarget;



    protected override void Start()
    {
        base.Start();
        fov = GetComponent<AnimalFOV>();
        hearing = GetComponent<WolfHearing>();
    }


    protected override void Update()
    {
        if (hearing != null && hearing.HeardSomething)
        {
            Debug.Log("Wolf heard: " + hearing.HeardAnimal.name);
           // ChangeState(State.Idle); testing
        }
        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Hunt)
        {
            if(IsHungry()) { /* Impemented inside isHungry(), so it will automatically change to hunt there*/ }
        }

        base.Update();
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

        }

        if(closestPrey != null)
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

    bool FindWater()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange);

        float closestDistance = Mathf.Infinity;
        GameObject closestWater = null;

        foreach (Collider hit in hits)
        {

            Debug.Log("Detected water collider");

            if (hit.CompareTag("Water"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWater = hit.gameObject;
                }

            }

        }

        if(closestWater != null)
        {
            waterTarget = closestWater;
            return true;
        }
        return false;

    }

    protected override void HuntState()
    {
        if(preyTarget != null)
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

        if(preyTarget == null)
        {
            ChangeState(State.Wander);
            return; // If the wolf has no prey, switch to wandering
        }

        MooseBehaviour moose = preyTarget.GetComponentInParent<MooseBehaviour>();
        if(moose != null && moose.isDead)
        {
            notifyDeath();
        }

        if (preyTarget != null)
        {
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

            if (!fov.IsInFOV(preyTarget.transform))
            {
                LostPrey(); // If the prey is no longer in the wolf's field of view, give up
                return;
            }

        }

        // Hunting timer
        huntTime += Time.deltaTime; 
        if (huntTime >= maxHuntTime)
        {   
            LostPrey(); // If the wolf has been hunting for too long, give up
            return;
        }

    }

    void AttackOnContact()
    {
        if(preyTarget != null)
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
        huntTime = 0; // Reset hunting time
        huntCooldownTimer = huntCooldown; // Start cooldown timer
        agent.speed = animal.speed; // Reset speed to normal
        ChangeState(State.Wander);
    }

    protected override bool IsHungry()
    {
        if(huntCooldownTimer > 0)
        {
            huntCooldownTimer -= Time.deltaTime; // Decrease cooldown timer
            return false; // Can't hunt while on cooldown
        }

        // Wolf is hungry, find food
        if (needs.isHungry && FindPrey())
        {
            ChangeState(State.Hunt);
            return true;
        }
        return false;
    }

    protected override bool IsThirsty()
    {
        // Wolf is thristy, find water source
        if (needs.isThirsty)
        {
            return FindWater();
        }
        return false;
    }

    protected override void EatStateForSpecificAnimal()
    {
        // When the wolf has killed its prey, it will eat it
    }

    protected override void DrinkStateForSpecificAnimal()
    {
        if (waterTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(waterTarget.transform.position);
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



    protected override void UpdateDrink()
    {
        // If the water target is null, switch back to wandering
        if (waterTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }

        // If the wolf has reached the water, stop moving
        if (hasArrived())
        {
            agent.isStopped = true;
            Debug.Log("Wolf drank water.");

            if (!needs.isThirsty)
            {
            waterTarget = null;
            ChangeState(State.Wander);
            }
        }

        else
        {
            agent.isStopped = false;
        }   
    }

    public void notifyDeath()
    {
        preyTarget = null;
        huntTime = 0f;
        agent.speed = animal.speed;
        ChangeState(State.Wander);

    }

}
