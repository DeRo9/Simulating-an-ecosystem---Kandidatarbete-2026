using System.Drawing;
using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        species = Species.bear;
        gameObject.tag = "Bear";

        canAttack = true;

        grownUpAge = 8f;
        oldAge = 16f;

        baseSpeed = 2f;

        baseSight = 25f;

        baseHearing = 40f;

        baseHealth = 110f;
        baseStrength = 24f;

        staminaDecreaseRate = 8f;

        base.Awake();    
    }   

}
