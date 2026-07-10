using System;
using Game.World;

namespace Game.Actor
{
    public interface IActorService
    {
        bool TryGet(
            Guid instanceId,
            out IActorRuntime actor);
    }

    public interface IActorRegistrationService
    {
        IDisposable Register(IActorRuntime actor);
    }

    public sealed class ActorService :
        IActorService,
        IActorRegistrationService
    {
        private readonly InstanceIndex<IActorRuntime> _actors = new();

        public IDisposable Register(IActorRuntime actor)
        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            return _actors.Register(
                actor.InstanceId,
                actor);
        }

        public bool TryGet(
            Guid instanceId,
            out IActorRuntime actor)
        {
            return _actors.TryGet(
                instanceId,
                out actor);
        }
    }
}