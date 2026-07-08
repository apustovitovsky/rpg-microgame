using System;
using Game.World;

namespace Game.Player
{
    public interface IPlayerService
    {
        IWorldHandle CurrentActor { get; }

        event Action CurrentActorChanged;

        void BindActor(IWorldHandle actor);

        void UnbindActor(IWorldHandle actor);
    }
}