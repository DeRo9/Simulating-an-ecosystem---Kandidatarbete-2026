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
        speed = 4f;
        grownUpAge = 2f;
        oldAge = 4f;
        size = 2f;
        sightRange = 80f;
        hearingRange = 50f;
        attackDamage = 20f;

        gameObject.tag = "Wolf";

        base.Awake();
    }
}
