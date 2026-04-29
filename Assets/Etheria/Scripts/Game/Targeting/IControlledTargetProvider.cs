namespace Etheria.Game.Targeting
{
    public interface IControlledTargetProvider
    {
        ITargetable ControlledTarget { get; }
        void SetTarget(ITargetable target);
        void ClearTarget();
    }
}
