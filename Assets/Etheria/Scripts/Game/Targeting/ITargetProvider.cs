using System;

namespace Etheria.Game.Targeting
{
    public interface ITargetProvider
    {
        ITargetCandidate CurrentTarget { get; }
        bool IsLocked { get; }

        event Action<ITargetCandidate> TargetChanged;
        event Action<bool> LockChanged;
    }
}