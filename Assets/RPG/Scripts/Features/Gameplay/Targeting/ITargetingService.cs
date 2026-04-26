using System;

namespace RPG.Gameplay
{
    public interface ITargetingService
    {
        ITargetable CurrentTarget { get; }
        event Action<ITargetable> TargetChanged;

        bool TryAcquireFromView();
        bool TrySetTarget(ITargetable target);
        bool IsValid(ITargetable target);
        void ClearTarget();
    }
}