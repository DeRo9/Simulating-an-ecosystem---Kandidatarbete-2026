using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        species = Species.bear;
        speed = 3f;
        grownUpAge = 8f;
        oldAge = 16f;
        size = 1.5f;

        base.Awake();
    }
}
