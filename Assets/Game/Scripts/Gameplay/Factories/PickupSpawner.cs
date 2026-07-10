using System;
using Game.World;

namespace Game.Pickup
{
    public interface IPickupSpawner
    {
        Guid Spawn(PickupSpawnRequest request);
    }

    public sealed class PickupSpawner : IPickupSpawner
    {
        private readonly PickupFactory _factory;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public PickupSpawner(
            PickupFactory factory,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _factory = factory;
            _spawnedObjects = spawnedObjects;
        }

        public Guid Spawn(PickupSpawnRequest request)
        {
            var spawnedObject = _factory.Create(request);

            if (spawnedObject == null ||
                spawnedObject.InstanceId == Guid.Empty ||
                spawnedObject.IsDisposed)
            {
                spawnedObject?.Dispose();
                return Guid.Empty;
            }

            if (!_spawnedObjects.Track(spawnedObject))
                return Guid.Empty;

            return spawnedObject.InstanceId;
        }
    }
}