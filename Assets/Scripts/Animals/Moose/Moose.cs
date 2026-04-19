using UnityEngine;

public class Moose : Animal
{

    protected override void Awake()
    {
        species = Species.moose;

        canAttack = true;

        grownUpAge = 4f;
        oldAge = 8f;

        size = 1f; 

        minSpeed = 1.65f;
        maxSpeed = 1.8f;

        minSight = 20f;
        maxSight = 25f;

        minHearing = 25f;
        maxHearing = 30f;

        minStrength = 13f;
        maxStrength = 15f;

        minHealth = 80f;
        maxHealth = 100f;

        minStaminaUsage = 5f;
        maxStaminaUsage = 5.5f;

        minHunger = 60f;
        maxHunger = 80f;

        minThirst = 65f;
        maxThirst = 85f;


        base.Awake();
    }
}
