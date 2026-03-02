using UnityEngine;

public class BearHearing : AnimalHearing
{
    protected override void DetectMovement()
    {
        HeardAnimal = null;
        float close = Mathf.Infinity;
        foreach (Animal a in animalsInRange) 
        {
            if (a == null) continue;
            if (!a.isMoving) continue;
            if (a.species == Species.bear) continue;

            float d = Vector3.Distance(transform.position,a.transform.position);
            if (d < close) {
                close = d;
                HeardAnimal = a;
            };
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
}
