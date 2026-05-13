using UnityEngine;

public class Moose : Animal
{

    protected override void Awake()
    {
        species = Species.moose;
        gameObject.tag = "Moose";

        canAttack = true;

        grownUpAge = 4f;
        oldAge = 8f;

        baseSpeed = 2.5f;
        baseSight = 25f;
        baseHearing = 20f;

        baseHealth = 90f;
        baseStrength = 14f;

        staminaDecreaseRate = 5f;

        base.Awake();
    }
}
