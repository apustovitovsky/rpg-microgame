using Game.World;

namespace Game.Pickup
{
    public interface IPickupSpawner
    {
        WorldId Spawn(PickupSpawnRequest request);
    }

    public sealed class PickupSpawner : IPickupSpawner
    {
        private readonly PickupFactory _factory;
        private readonly IWorldObjectRegistry _world;

        public PickupSpawner(
            PickupFactory factory,
            IWorldObjectRegistry world)
        {
            _factory = factory;
            _world = world;
        }

        public WorldId Spawn(PickupSpawnRequest request)
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