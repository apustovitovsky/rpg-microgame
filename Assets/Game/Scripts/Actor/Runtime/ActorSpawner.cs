using Game.World;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        WorldId Spawn(ActorSpawnRequest request);
    }

    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly ActorWorldObjectFactory _factory;
        private readonly IWorldLifetimeManager _world;

        public ActorSpawner(
            ActorWorldObjectFactory factory,
            IWorldLifetimeManager world)
        {
            _factory = factory;
            _world = world;
        }

        public WorldId Spawn(ActorSpawnRequest request)
        {
            var result = _factory.Create(request);

            if (!result.IsValid)
            {
                result.Lifetime?.Dispose();
                return default;
            }

            if (!_world.Track(result.Lifetime))
                return default;

            return result.WorldId;
        }
    }
}