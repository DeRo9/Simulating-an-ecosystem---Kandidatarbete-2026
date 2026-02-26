using UnityEngine;

public class Moose : Animal
{
    protected override void Awake()
    {
        species = Species.moose;
        speed = 2f;
        grownUpAge = 4f;
        oldAge = 8f;
        size = 1f;
        sightRange = 50f;
        hearingRange = 20f;

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
