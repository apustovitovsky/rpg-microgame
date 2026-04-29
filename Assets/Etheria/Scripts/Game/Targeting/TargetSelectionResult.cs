using Etheria.Game.Targeting;

namespace Etheria.Game.Targeting
{
    public readonly struct TargetSelectionResult
    {
        public TargetSelectionResult(TargetSelectionStatus status, ITargetable target)
        {
            Status = status;
            Target = target;
        }

        public TargetSelectionStatus Status { get; }
        public ITargetable Target { get; }

        public static TargetSelectionResult None =>
            new(TargetSelectionStatus.None, null);
    }

    public enum TargetSelectionStatus
    {
        None = 0,
        Selected = 1,
        AlreadySelected = 2,
        Cleared = 3
    }
}
