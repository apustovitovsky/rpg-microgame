using Game.World;

namespace Game.Pickup
{
    public interface IPickupSpawner
    {
        IWorldObject Spawn(PickupSpawnRequest request);
    }

    public sealed class PickupSpawner : IPickupSpawner
    {
        private readonly PickupWorldObjectFactory _factory;
        private readonly IWorldManager _world;

        public PickupSpawner(
            PickupWorldObjectFactory factory,
            IWorldManager world)
        {
            _factory = factory;
            _world = world;
        }

        public IWorldObject Spawn(PickupSpawnRequest request)
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