using System;
using Game.Targeting;

namespace Game.Actor
{
    public interface IActorTargeting
    {
        ITargetable CurrentTarget { get; }

        event Action<ITargetable> CurrentTargetChanged;
    }
}