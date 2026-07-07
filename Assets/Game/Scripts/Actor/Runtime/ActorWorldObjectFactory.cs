using System.Collections.Generic;
using Game.World;
using VContainer;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory
    {
        private readonly IObjectResolver _resolver;

        public ActorWorldObjectFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public IWorldObject Create(
            WorldId worldId)
        {
            var view = _resolver.Resolve<IActorView>();

            var capabilities = _resolver.Resolve<IEnumerable<IWorldCapability>>();

            return new WorldObject(
                worldId,
                view.Root.gameObject,
                new WorldCapabilityProvider(capabilities));
        }
    }
}