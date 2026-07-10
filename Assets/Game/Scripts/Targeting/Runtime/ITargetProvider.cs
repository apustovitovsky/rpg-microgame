using System;

namespace Game.Targeting
{
    public interface ITargetProvider
    {
        ITargetable CurrentTarget { get; }

        event Action<ITargetable> CurrentTargetChanged;
    }
}