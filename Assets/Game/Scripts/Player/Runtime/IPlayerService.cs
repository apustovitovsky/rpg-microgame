using System;
using Game.World;

namespace Game.Player
{
    public interface IPlayerService
    {
        WorldId CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(WorldId actorWorldId);

        void UnbindActor(WorldId actorWorldId);
    }
}