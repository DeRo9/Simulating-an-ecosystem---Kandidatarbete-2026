public class MooseCalfBehaviour : CubBehaviour
{
    protected override void BecomeAdult()
    {
        base.BecomeAdult();
        MooseBehaviour moose = GetComponent<MooseBehaviour>();
        if (moose != null) moose.enabled = true;
    }
}