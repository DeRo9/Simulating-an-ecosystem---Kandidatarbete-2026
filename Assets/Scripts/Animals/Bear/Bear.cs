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

        size = 1.5f;

        // Speed
        minSpeed = 2f;
        maxSpeed = 3.5f;

        // Sight
        minSight = 50f;
        maxSight = 70f;

        // Hearing
        minHearing = 45f;
        maxHearing = 55f;

        // Strength
        minStrength = 10f;
        maxStrength = 18f;

        //attackDamage = 40f;


        base.Awake();
    }   

    /*
    public override void GetStats(out float speed, out float size, out float sight, out float hearing)
    {
        speed = this.speed;
        size = this.size;
        sight = this.sightRange;
        hearing = this.hearingRange;
    }
    */

    public override void GetStats(out float speed, out float sight, out float hearing)
    {
        speed = this.speed;
        sight = this.sightRange;
        hearing = this.hearingRange;
    }



}
