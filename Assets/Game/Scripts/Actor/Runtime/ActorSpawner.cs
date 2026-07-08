using Game.World;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        IWorldObject Spawn(ActorSpawnRequest request);
    }

    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly ActorWorldObjectFactory _factory;
        private readonly IWorldManager _world;

        public ActorSpawner(
            ActorWorldObjectFactory factory,
            IWorldManager world)
        {
            _factory = factory;
            _world = world;
        }

        public IWorldObject Spawn(ActorSpawnRequest request)
        {
            var result = _factory.Create(request);

            if (!result.IsValid)
            {
                result.Lifetime?.Dispose();
                return null;
            }

            if (!_world.Track(
                    result.WorldObject,
                    result.Lifetime))
            {
                return null;
            }

            return result.WorldObject;
        }
    }
}