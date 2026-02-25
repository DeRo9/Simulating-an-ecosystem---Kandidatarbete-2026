using UnityEngine;

public class Moose : Animal
{
    protected override void Awake()
    {
        grownUpAge = 4f;
        oldAge = 8f;

        base.Awake();
    }
}
