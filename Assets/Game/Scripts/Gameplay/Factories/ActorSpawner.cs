using System;
using Game.World;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        Guid Spawn(ActorSpawnRequest request);
    }

    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly ActorFactory _factory;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public ActorSpawner(
            ActorFactory factory,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _factory = factory;
            _spawnedObjects = spawnedObjects;
        }

        public Guid Spawn(ActorSpawnRequest request)
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