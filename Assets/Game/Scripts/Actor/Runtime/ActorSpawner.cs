using Game.World;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        WorldId Spawn(ActorSpawnRequest request);
    }

    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly ActorFactory _factory;
        private readonly IWorldManager _world;

        public ActorSpawner(
            ActorFactory factory,
            IWorldManager world)
        {
            _factory = factory;
            _world = world;
        }

        public WorldId Spawn(ActorSpawnRequest request)
        {
            var lifetime = _factory.Create(request);

            if (lifetime == null ||
                lifetime.WorldId.IsEmpty ||
                lifetime.IsDisposed)
            {
                lifetime?.Dispose();
                return default;
            }

            if (!_world.Track(lifetime))
                return default;

            return lifetime.WorldId;
        }
    }
}