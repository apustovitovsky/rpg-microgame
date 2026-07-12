using System;
using Game.World;

namespace Game.UI
{
    public interface IDisplayNameService
    {
        bool TryGet(
            Guid instanceId,
            out string displayName);
    }

    public sealed class DisplayNameService :
        IDisplayNameService
    {
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public DisplayNameService(
            ISpawnedObjectRegistry spawnedObjects)
        {
            _spawnedObjects = spawnedObjects
                ?? throw new ArgumentNullException(
                    nameof(spawnedObjects));
        }

        public bool TryGet(
            Guid instanceId,
            out string displayName)
        {
            displayName = null;

            if (!_spawnedObjects.TryGet(
                    instanceId,
                    out var spawnedObject))
            {
                return false;
            }

            displayName = spawnedObject.Instance.DisplayName;

            return !string.IsNullOrWhiteSpace(displayName);
        }
    }
}