namespace Game.Targeting
{
    public sealed class TargetableFilter :
        ITargetFilter
    {
        public bool IsMatch(ITargetable target)
        {
            return target != null &&
                target.IsTargetable &&
                target.TargetPoint != null;
        }
    }
}