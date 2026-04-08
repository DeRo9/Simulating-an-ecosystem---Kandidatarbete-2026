using UnityEngine;

public class Moose : Animal
{

    protected override void Awake()
    {
        species = Species.moose;

        canAttack = false;

        grownUpAge = 4f;
        oldAge = 8f;

        size = 1f;

        // Speed
        minSpeed = 1.5f;
        maxSpeed = 2f;

        // Sight
        minSight = 40f;
        maxSight = 60f;

        // Hearing
        minHearing = 15f;
        maxHearing = 25f;

        // Strength
        minStrength = 8f;
        maxStrength = 14f;

        base.Awake();
    }

    




}
