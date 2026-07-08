using Game.World;

namespace Game.Pickup
{
    public interface IPickupSpawner
    {
        WorldId Spawn(PickupSpawnRequest request);
    }

    public sealed class PickupSpawner : IPickupSpawner
    {
        private readonly PickupWorldObjectFactory _factory;
        private readonly IWorldLifetimeManager _world;

        public PickupSpawner(
            PickupWorldObjectFactory factory,
            IWorldLifetimeManager world)
        {
            _factory = factory;
            _world = world;
        }

        public WorldId Spawn(PickupSpawnRequest request)
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