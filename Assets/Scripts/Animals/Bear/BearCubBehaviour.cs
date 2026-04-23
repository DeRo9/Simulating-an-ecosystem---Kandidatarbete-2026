public class BearCubBehaviour : CubBehaviour
{
    protected override void BecomeAdult()
    {
        base.BecomeAdult();
        BearBehaviour bear = GetComponent<BearBehaviour>();
        if (bear != null) bear.enabled = true;
    }
}