namespace Game.Targeting
{
    public interface ITargetFilter
    {
        bool IsMatch(ITargetable target);
    }
}