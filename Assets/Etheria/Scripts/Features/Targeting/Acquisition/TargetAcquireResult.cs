using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public readonly struct TargetAcquireResult
    {
        public TargetAcquireResult(TargetAcquireStatus status, ITargetable target)
        {
            Status = status;
            Target = target;
        }

        public TargetAcquireStatus Status { get; }
        public ITargetable Target { get; }

        public static TargetAcquireResult None =>
            new(TargetAcquireStatus.None, null);
    }

    public enum TargetAcquireStatus
    {
        None = 0,
        NewTarget = 1,
        CurrentTarget = 2
    }
}
