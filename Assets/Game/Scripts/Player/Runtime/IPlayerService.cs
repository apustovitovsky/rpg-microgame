using System;
using Game.Actor;

namespace Game.Player
{
    public interface IPlayerService
    {
        ActorInstance CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(ActorInstance actor);
        void UnbindActor(ActorInstance actor);
    }
}