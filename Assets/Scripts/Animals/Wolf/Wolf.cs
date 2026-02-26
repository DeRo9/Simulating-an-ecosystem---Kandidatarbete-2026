using UnityEngine;

public class Wolf : Animal
{
    protected override void Awake()
    {
        species = Species.wolf;
        speed = 4f;
        grownUpAge = 2f;
        oldAge = 4f;
        size = 2f;

        base.Awake();
    }
}
