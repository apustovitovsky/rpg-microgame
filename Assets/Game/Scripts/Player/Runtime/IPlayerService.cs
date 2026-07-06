using System;
using Game.Actor;

namespace Game.Player
{
    public interface IPlayerService
    {
        WorldActor CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(WorldActor actor);
        void UnbindActor(WorldActor actor);
    }
}