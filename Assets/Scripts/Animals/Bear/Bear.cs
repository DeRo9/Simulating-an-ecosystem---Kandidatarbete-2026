using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        grownUpAge = 8f;
        oldAge = 16f;

        base.Awake();
    }
}
