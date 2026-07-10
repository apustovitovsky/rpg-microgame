using System;
using System.Collections.Generic;

namespace Game.World
{
    public interface ISpawnedObjectRegistry
    {
        bool Track(ISpawnedObject spawnedObject);

        bool Despawn(Guid instanceId);

        void DespawnAll();
    }

    public sealed class SpawnedObjectRegistry :
        ISpawnedObjectRegistry
    {
        private readonly Dictionary<Guid, ISpawnedObject> _entries =
            new();

        public bool Track(ISpawnedObject spawnedObject)
        {
            if (spawnedObject == null ||
                spawnedObject.InstanceId == Guid.Empty ||
                spawnedObject.IsDisposed)
            {
                spawnedObject?.Dispose();
                return false;
            }

            var instanceId = spawnedObject.InstanceId;

            if (!_entries.TryAdd(instanceId, spawnedObject))
            {
                spawnedObject.Dispose();
                return false;
            }

            spawnedObject.Add(new LifetimeToken(() =>
            {
                if (_entries.TryGetValue(
                        instanceId,
                        out var current) &&
                    ReferenceEquals(current, spawnedObject))
                {
                    _entries.Remove(instanceId);
                }
            }));

            return true;
        }

        public bool Despawn(Guid instanceId)
        {
            if (instanceId == Guid.Empty ||
                !_entries.TryGetValue(
                    instanceId,
                    out var spawnedObject))
            {
                return false;
            }

            spawnedObject.Dispose();
            return true;
        }

        public void DespawnAll()
        {
            var instanceIds = new List<Guid>(_entries.Keys);

            foreach (var instanceId in instanceIds)
                Despawn(instanceId);
        }
    }
}