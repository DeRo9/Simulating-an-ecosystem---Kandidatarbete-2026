using UnityEngine;

public class Moose : Animal
{

    private AnimalNeeds needs;

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
        minSight = 40f;
        maxSight = 60f;

        // Hearing
        minHearing = 15f;
        maxHearing = 25f;

        // Strength
        minStrength = 13f;
        maxStrength = 15f;

        // Health
        minHealth = 80f;
        maxHealth = 100f;
        needs.maxHealth = health;

        base.Awake();
    }
}
