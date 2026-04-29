using System;

namespace Etheria.Game.Targeting
{
    public interface ITargetingService
    {
        ITargetable CurrentTarget { get; }

        bool TryAcquireFromView();
        bool TryCycleTarget(int direction);
        bool TrySetTarget(ITargetable target);
        bool IsValid(ITargetable target);
        void ClearTarget();
    }
}
