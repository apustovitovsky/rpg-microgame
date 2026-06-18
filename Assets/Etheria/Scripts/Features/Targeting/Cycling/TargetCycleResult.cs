using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public readonly struct TargetCycleResult
    {
        public TargetCycleResult(TargetCycleStatus status, ITargetable target)
        {
            Status = status;
            Target = target;
        }

        public TargetCycleStatus Status { get; }
        public ITargetable Target { get; }

        public static TargetCycleResult None =>
            new(TargetCycleStatus.None, null);
    }

    public enum TargetCycleStatus
    {
        None = 0,
        Selected = 1
    }
}
