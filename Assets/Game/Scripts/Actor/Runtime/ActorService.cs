using System;
using Game.World;

namespace Game.Actor
{
    public interface IActorService
    {
        bool TryGet(
            WorldId worldId,
            out IWorldActor actor);
    }

    public interface IActorRegistrationService
    {
        IDisposable Register(IWorldActor actor);
    }

    public sealed class ActorService :
        IActorService,
        IActorRegistrationService
    {
        private readonly WorldIndex<IWorldActor> _actors = new();

        public IDisposable Register(IWorldActor actor)
        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            return _actors.Register(
                actor.WorldId,
                actor);
        }

        public bool TryGet(
            WorldId worldId,
            out IWorldActor actor)
        {
            return _actors.TryGet(
                worldId,
                out actor);
        }
    }
}