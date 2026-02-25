using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class MooseBehaviour : AnimalBehaviour
{

    AnimalNeeds needs;
    GameObject foodTarget;
    GameObject waterTarget;
    float foodDetectionRadius = 20f;
    float waterDetectionRadius = 150f;
    

    protected override void Start()
    {
        base.Start();
        needs = GetComponent<AnimalNeeds>();
    }



    // Finds the closest food item within the detection radius and sets it as the target
    bool FindFood()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, foodDetectionRadius);

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider hit in hits)
        {

            Debug.Log("Moose found plant.");
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
        // Moose is hungry, find food
        if (needs.isHungry)
        {
            return FindFood();
        }
        return false;
    }

    protected override bool IsThirsty()
    {
        // Moose is thristy, find water source
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
        if (hasArrived()) // If the moose has reached its destination, switch to idle state
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



        // If the moose has reached the food, stop moving
        if (hasArrived())
        {
            agent.isStopped = true;
            Debug.Log("Moose ate.");
        }
        else
        {
            agent.isStopped = false;
        }

        // If the moose is no longer hungry, stop eating and switch back to wandering
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