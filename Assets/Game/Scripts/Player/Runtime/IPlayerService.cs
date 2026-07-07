using System;
using Game.World;

namespace Game.Player
{
    public interface IPlayerService
    {
        IWorldObject CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(IWorldObject actor);

        void UnbindActor(IWorldObject actor);
    }
}