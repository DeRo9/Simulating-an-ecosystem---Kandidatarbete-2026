using UnityEngine;

public class BearBehaviour : AnimalBehaviour
{

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
        ChangeState(State.Wander); // Just for now
    }

    protected override void UpdateDrink()
    {
        ChangeState(State.Wander); // Just for now
    }

}


