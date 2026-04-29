using System;

namespace Etheria.Game.Targeting
{
    public interface ITargetingService
    {
        ITargetable CurrentTarget { get; }
        event Action<ITargetable> TargetChanged;

        bool TryAcquireFromView();
        bool TryCycleTarget(int direction);
        bool TrySetTarget(ITargetable target);
        bool IsValid(ITargetable target);
        bool ValidateCurrentTarget();
        void ClearTarget();
    }
}
