using System;
using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class MooseBehaviour : AnimalBehaviour
{

    private Animal animal;

    new AnimalNeeds needs;
    GameObject foodTarget;
    GameObject waterTarget;
    MooseFOV fov;

    GameObject enemy; // For fleeing from wolves and bears

    protected override void Start()
    {
        base.Start();
        animal = GetComponent<Animal>();
        needs = GetComponent<AnimalNeeds>();
        fov = GetComponent<MooseFOV>();
    }


    protected override void Update()
    {

        // If not currently eating, drinking or fleeing, check if the moose needs to eat or drink and switch to the appropriate state
        if (CurrentState != State.Eat && CurrentState != State.Drink && CurrentState != State.Fleeing)
        {
            // If the moose is more thirsty than hungry, switch to drink state, if more hungry than thirsty, switch to eat state
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


            if (!fov.IsInFOV(hit.transform))
            {
                continue; // Skip if not in FOV
            }

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

            // No longer thirsty
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

    protected override void UpdateFlee()
    {
        // If there is no enemy, switch back to wandering
        if (enemy == null)
        {
            agent.speed = animal.speed;
            ChangeState(State.Wander);
            return;
        }

        // Flee in the opposite direction of the enemy
        Vector3 fleeDirection = (transform.position - enemy.transform.position).normalized;
        Vector3 fleeDestination = transform.position + fleeDirection * animal.sightRange;

        NavMeshHit hit;
        // sample a valid position on the navmesh in the flee direction
        if (NavMesh.SamplePosition(fleeDestination, out hit, animal.sightRange, NavMesh.AllAreas))
        {
            if (!agent.hasPath)
                agent.SetDestination(hit.position);
        }
    }

    protected override void FleeState()
    {
        // set food and water to null to stop the moose from trying to eat or drink while fleeing
        foodTarget = null; 
        waterTarget = null;

        agent.isStopped = false;
        agent.speed = animal.runningSpeed; // Increase speed while fleeing
    }

    public void OnBeingHunted(GameObject predator)
    {
        enemy = predator;
        ChangeState(State.Fleeing);
    }

    public void OnNoLongerHunted(GameObject predator)
    {
        if(enemy == predator)
        {
            enemy = null;
            agent.speed = animal.speed; // Reset speed to normal
            ChangeState(State.Wander);
        }
    }

    public void InflictDamage(float damage)
    {
        needs.TakeDamage(damage);
    }

}