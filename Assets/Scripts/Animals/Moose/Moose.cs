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
        minSpeed = 1.65f;
        maxSpeed = 1.8f;

        // Sight
        minSight = 20f;
        maxSight = 25f;

        // Hearing
        minHearing = 25f;
        maxHearing = 30f;

        // Strength
        minStrength = 13f;
        maxStrength = 15f;

        // Health
        minHealth = 80f;
        maxHealth = 100f;

        base.Awake();
    }
}
