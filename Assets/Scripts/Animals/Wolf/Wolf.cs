using UnityEngine;

public class Wolf : Animal
{
    protected override void Awake()
    {
        grownUpAge = 2f;
        oldAge = 4f;

        base.Awake();
    }
}
