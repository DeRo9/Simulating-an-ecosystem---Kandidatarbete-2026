using UnityEngine;

public class BearBehaviour : AnimalBehaviour
{
    GameObject foodTarget;
    BearHearing hearing;

    protected override void Start()
    {
        base.Start();
        hearing = GetComponent<BearHearing>();
    }

    protected override void Update()
    {
        if (hearing != null && hearing.HeardSomething)
        {
            Debug.Log("Bear heard: " + hearing.HeardAnimal.name);
           // ChangeState(State.Idle); testing
        }
        if (CurrentState != State.Eat && CurrentState != State.Drink)
        {

            if (needs.howThirstyInPercent < needs.howHungryInPercent && IsThirsty())
            {
                ChangeState(State.Drink);
            }
            else if (needs.howThirstyInPercent > needs.howHungryInPercent && IsHungry())
            {
                ChangeState(State.Eat);
            }
        }

        base.Update();
    }

    // Finds the closest food item within the detection radius and sets it as the target
    bool FindFood()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, animal.sightRange);

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

        if(closestFood != null)
        {
            foodTarget = closestFood;
            return true;
        }

        return false;
    }


    protected override bool IsHungry()
    {
        // Bear is hungry, find food
        if (needs.isHungry)
        {
            return FindFood();
        }
        return false;
    }

    protected override bool IsThirsty()
    {
        // Bear is thristy, find water source
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
            Debug.Log("Bear ate.");
        }
        else
        {
            agent.isStopped = false;
        }

        // If the Bear is no longer hungry, stop eating and switch back to wandering
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

        // If the moose has reached the water, stop moving
        if (hasArrived())
        {
            agent.isStopped = true;
            Debug.Log("Moose drank water.");

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
        
        // If the moose is no longer thirsty, stop drinking and switch back to wandering
 
        
    }

}


