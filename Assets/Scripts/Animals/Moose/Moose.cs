using UnityEngine;

public class Moose : Animal
{
    protected override void Awake()
    {
        species = Species.moose;
        speed = 2f;
        grownUpAge = 4f;
        oldAge = 8f;
        size = 1f;

        base.Awake();
    }
}
