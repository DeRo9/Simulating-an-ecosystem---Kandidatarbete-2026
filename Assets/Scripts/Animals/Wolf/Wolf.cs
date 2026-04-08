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

        // Speed
        minSpeed = 1.5f;
        maxSpeed = 2f;

        // Sight
        minSight = 70f;
        maxSight = 90f;

        // Hearing
        minHearing = 45f;
        maxHearing = 55f;

        // Strength
        minStrength = 8f;
        maxStrength = 14f;

        //attackDamage = 20f;

        gameObject.tag = "Wolf";

        base.Awake();
    }   



}
