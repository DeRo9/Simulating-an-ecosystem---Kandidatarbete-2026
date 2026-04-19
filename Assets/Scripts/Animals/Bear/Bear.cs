using System.Drawing;
using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        species = Species.bear;

        canAttack = true;

        grownUpAge = 8f;
        oldAge = 16f;

        size = 1f;

        speed = 1.9f;

        minSight = 20f;
        maxSight = 30f;

        minHearing = 35f;
        maxHearing = 40f;

        health = 110f;

        strength = 24f;

        needs.staminaDecreaseRate = 8f;

        base.Awake();
    }   

}
