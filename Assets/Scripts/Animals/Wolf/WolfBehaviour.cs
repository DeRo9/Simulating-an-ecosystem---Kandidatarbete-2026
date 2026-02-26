using UnityEngine;
using UnityEngine.InputSystem.Android;

public class WolfBehaviour : AnimalBehaviour
{
    //Animaltype
    private new Animal animal;

    new AnimalNeeds needs;
    AnimalFOV fov;

    GameObject preyTarget;
    float attackRange = 2f; // Range within which the wolf can attack prey

    float maxHuntTime = 20f; // Maximum time the wolf will spend hunting before giving up
    [SerializeField]
    float huntTime = 0;

    float huntCooldown = 5f; // Time the wolf must wait after giving up on a hunt before it can hunt again
    [SerializeField]
    float huntCooldownTimer = 0;

    GameObject waterTarget;



    protected override void Start()
    {
        base.Start();
        animal = GetComponent<Animal>();
        needs = GetComponent<AnimalNeeds>();
        fov = GetComponent<AnimalFOV>();
    }


    protected override void Update()
    {
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
            
            Debug.Log("Wolf found prey.");
            if (hit.CompareTag("Moose"))
            {
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
            agent.SetDestination(preyTarget.transform.position);
        }
         else
        {
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

        huntTime += Time.deltaTime; // Increase hunting time

        if (huntTime >= maxHuntTime)
        {
            preyTarget = null; // Give up on the prey after hunting for too long
            huntTime = 0; // Reset hunting time
            huntCooldownTimer = huntCooldown; // Start cooldown timer
            ChangeState(State.Wander);
            return;
        }

        // Keep moving towards the prey
        float distance = Vector3.Distance(transform.position, preyTarget.transform.position); 
        agent.isStopped = false;
        agent.SetDestination(preyTarget.transform.position);

        if(distance <= attackRange)
        {
            // Implement attacking here
        }
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

}
