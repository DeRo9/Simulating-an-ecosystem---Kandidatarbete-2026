using UnityEngine;

public class Wolf : Animal
{
    protected override void Awake()
    {
        species = Species.wolf;
        speed = 4f;
        grownUpAge = 2f;
        oldAge = 4f;
        size = 2f;
        sightRange = 80f;
        hearingRange = 50f;
        attackDamage = 20f;

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
