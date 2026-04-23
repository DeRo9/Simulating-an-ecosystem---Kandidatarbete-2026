public class WolfCubBehaviour : CubBehaviour
{
    protected override void BecomeAdult()
    {
        base.BecomeAdult();
        WolfBehaviour wolf = GetComponent<WolfBehaviour>();
        if (wolf != null) wolf.enabled = true;
    }
}