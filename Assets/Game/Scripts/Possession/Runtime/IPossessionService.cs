using System;
using Game.Actor;

namespace Game.Possession
{
    public interface IPossessionService
    {
        IActorView CurrentActor { get; }

        event Action CurrentActorChanged;

        void Possess(IActorView actor);
        void Unpossess(IActorView actor);
    }
}