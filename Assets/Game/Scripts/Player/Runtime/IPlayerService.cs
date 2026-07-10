using System;

namespace Game.Player
{
    public interface IPlayerService
    {
        Guid CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(Guid actorInstanceId);

        void UnbindActor(Guid actorInstanceId);
    }
}