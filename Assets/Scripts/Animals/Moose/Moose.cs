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

        speed = 2f;

        minSight = 20f;
        maxSight = 25f;

        minHearing = 25f;
        maxHearing = 30f;

        health = 90f;

        strength = 14f;

        needs.staminaDecreaseRate = 5f;

        base.Awake();
    }
}
