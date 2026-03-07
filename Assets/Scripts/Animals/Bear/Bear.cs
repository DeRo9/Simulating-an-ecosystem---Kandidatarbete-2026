using UnityEngine;

public class Bear : Animal
{
    protected override void Awake()
    {
        species = Species.bear;
        speed = 3f;
        grownUpAge = 8f;
        oldAge = 16f;
        size = 1.5f;
        sightRange = 60f;
        hearingRange = 50f;
        attackDamage = 40f;

        base.Awake();
    }

    public override void GetStats(out float speed, out float size, out float sight, out float hearing)
    {
        speed = this.speed;
        size = this.size;
        sight = this.sightRange;
        hearing = this.hearingRange;
    }
}
