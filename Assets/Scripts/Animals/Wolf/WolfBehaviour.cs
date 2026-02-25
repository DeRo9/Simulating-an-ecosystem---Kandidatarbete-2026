using UnityEngine;

public class WolfBehaviour : AnimalBehaviour
{

    new AnimalNeeds needs;
    GameObject foodTarget;
    GameObject waterTarget;
    float foodDetectionRadius = 40f;
    float waterDetectionRadius = 200f;
    

    protected override void Start()
    {
        base.Start();
        needs = GetComponent<AnimalNeeds>();
    }

    bool FindPrey()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, foodDetectionRadius);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {

            Debug.Log("Wolf found plant.");
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

        if(closestFood != null)
        {
            foodTarget = closestFood;
            return true;
        }

        return false;
    }

    bool FindWater()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, waterDetectionRadius);

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


    protected override bool IsHungry()
    {
        // Wolf is hungry, find food
        if (needs.isHungry)
        {
            return FindPrey();
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
        if (foodTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(foodTarget.transform.position);
        }
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

    protected override void UpdateEat()
    {
        // If the food target is null, switch back to wandering
        if (foodTarget == null)
        {
            ChangeState(State.Wander);
            return;
        }



        // If the wolf has reached the food, stop moving
        // Probably needs to be changed so that the wolf can continue to follow the moose as it runs away.
        if (hasArrived())
        {
            agent.isStopped = true; 
            Debug.Log("Wolf ate.");
        }
        else
        {
            agent.isStopped = false;
        }

        // If the wolf is no longer hungry, stop eating and switch back to wandering
        if (!needs.isHungry)
        {
            foodTarget = null;
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
