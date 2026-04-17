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

        minSpeed = 1.8f;
        maxSpeed = 1.95f;

        minSight = 20f;
        maxSight = 30f;

        minHearing = 45f;
        maxHearing = 55f;

        minStrength = 23f;
        maxStrength = 25f;

        minHealth = 100f;
        maxHealth = 120f;

        needs.staminaDecreaseRate = 8f;

        base.Awake();
    }   

}
