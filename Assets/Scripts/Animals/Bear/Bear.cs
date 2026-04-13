using System.Drawing;
using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        species = Species.bear;

        canAttack = true;

        grownUpAge = 8f;
        oldAge = 16f;

        size = 1f;

        minSpeed = 2f;
        maxSpeed = 3f;

        minSight = 50f;
        maxSight = 70f;

        minHearing = 45f;
        maxHearing = 55f;

        minStrength = 10f;
        maxStrength = 18f;



        base.Awake();
    }   

}
