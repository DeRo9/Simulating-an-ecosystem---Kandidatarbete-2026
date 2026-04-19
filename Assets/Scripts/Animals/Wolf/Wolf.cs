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
        gameObject.tag = "Wolf";

        canAttack = true;

        grownUpAge = 2f;
        oldAge = 4f;

        baseSpeed = 1.8f;

        baseSight = 30f;

        baseHearing = 45f;

        baseHealth = 30f;
        baseStrength = 10f;

        staminaDecreaseRate = 4f;

        base.Awake();
    }   
}
