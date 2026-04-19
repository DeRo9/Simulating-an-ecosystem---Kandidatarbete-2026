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

        size = 1f;

        speed = 1.8f;

        minSight = 30f;
        maxSight = 35f;

        minHearing = 40f;
        maxHearing = 45f;

        minHealth = 20f;
        maxHealth = 40f;

        strength = 10f;

        needs.staminaDecreaseRate = 4f;

        gameObject.tag = "Wolf";

        base.Awake();
    }   
}
