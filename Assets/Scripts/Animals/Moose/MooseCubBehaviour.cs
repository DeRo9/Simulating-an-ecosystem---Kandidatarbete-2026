using UnityEngine;
public class MooseCubBehaviour: CubBehaviour
{
    protected override void BecomeAdult()
    {
        base.BecomeAdult();
        MooseBehaviour moose = GetComponent<MooseBehaviour>();
        if (moose != null ) moose.enabled = true;
    }
}