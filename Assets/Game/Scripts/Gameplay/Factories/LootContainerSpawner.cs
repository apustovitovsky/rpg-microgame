using System;
using Game.World;

namespace Game.Loot
{
    public interface ILootContainerSpawner
    {
        Guid Spawn(LootContainerSpawnRequest request);
    }

    public sealed class LootContainerSpawner :
        ILootContainerSpawner
    {
        private readonly LootContainerFactory _factory;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public LootContainerSpawner(
            LootContainerFactory factory,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _factory = factory;
            _spawnedObjects = spawnedObjects;
        }

        public Guid Spawn(LootContainerSpawnRequest request)
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