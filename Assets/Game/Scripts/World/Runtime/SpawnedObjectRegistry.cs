using System;
using System.Collections.Generic;


namespace Game.World
{
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

        public bool TryGet(
            Guid instanceId,
            out ISpawnedObject spawnedObject)
        {
            spawnedObject = null;

            return instanceId != Guid.Empty &&
                   _entries.TryGetValue(
                       instanceId,
                       out spawnedObject);
        }

        public bool TryGetInstance<TInstance>(
            Guid instanceId,
            out TInstance instance)
            where TInstance : class, IWorldInstance
        {
            instance = null;

            if (!TryGet(instanceId, out var spawnedObject))
                return false;

            instance = spawnedObject.Instance as TInstance;
            return instance != null;
        }

        public bool Despawn(Guid instanceId)
        {
            if (!TryGet(instanceId, out var spawnedObject))
                return false;

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