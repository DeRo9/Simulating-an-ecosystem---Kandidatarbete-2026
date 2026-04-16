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

        minHearing = 35f;
        maxHearing = 40f;

        minStrength = 23f;
        maxStrength = 25f;

        // Food
        minFood = 100f;
        maxFood = 150f;

        // Health
        minHealth = 100f;
        maxHealth = 120f;

        // Stamina
        minStamina = 8f;
        maxStamina = 10f;


        base.Awake();
    }   

}
