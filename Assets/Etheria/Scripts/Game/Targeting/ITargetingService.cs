using System;

namespace Etheria.Game.Targeting
{
    public interface ITargetingService
    {
        ITargetable CurrentTarget { get; }
        event Action<ITargetable> TargetChanged;

        bool TryAcquireFromView();
        bool TrySetTarget(ITargetable target);
        bool IsValid(ITargetable target);
        bool ValidateCurrentTarget();
        void ClearTarget();
    }
}
