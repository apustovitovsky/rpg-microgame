using System;

namespace RPG.Gameplay
{
    public interface ITargetingService
    {
        IActorTargetable CurrentTarget { get; }
        event Action<IActorTargetable> TargetChanged;

        bool TryAcquireFromView();
        bool TrySetTarget(IActorTargetable target);
        bool IsValid(IActorTargetable target);
        void ClearTarget();
    }
}