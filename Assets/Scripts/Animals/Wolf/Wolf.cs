using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wolf : Animal
{
    public WolfPackManager pack;
    public bool isLeader;

    protected override void Awake()
    {
        species = Species.wolf;

        canAttack = true;

        grownUpAge = 2f;
        oldAge = 4f;

        size = 2f;

        minSpeed = 1.5f;
        maxSpeed = 1.8f;

        minSight = 30f;
        maxSight = 35f;

        // Hearing
        minHearing = 40f;
        maxHearing = 45f;

        minStrength = 9f;
        maxStrength = 11f;

        minHealth = 20f;
        maxHealth = 40f;

        needs.staminaDecreaseRate = 4.5f;

        gameObject.tag = "Wolf";

        base.Awake();
    }   
}
