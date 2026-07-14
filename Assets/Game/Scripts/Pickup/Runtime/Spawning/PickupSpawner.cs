using System;
using Game.Core;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupSpawner :
        IPickupSpawner
    {
        private readonly IWorldSpawner _worldSpawner;

        public PickupSpawner(IWorldSpawner worldSpawner)
        {
            _worldSpawner = worldSpawner
                ?? throw new ArgumentNullException(nameof(worldSpawner));
        }

        public PickupInstance Spawn(PickupSpawnRequest request)
        {
            var definition = request.Definition != null
                ? request.Definition
                : throw new ArgumentNullException(
                    nameof(request),
                    "Pickup spawn request requires a definition.");

            var instanceId = request.InstanceId ?? Guid.NewGuid();

            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Pickup instance id cannot be empty.",
                    nameof(request));
            }

            var instance = definition.CreateInstance(instanceId);

            var root = _worldSpawner.Spawn(
                instance.InstanceId,
                definition.Prefab,
                request.Placement,
                new WorldInstanceInstaller<PickupInstance>(instance));

            if (!string.IsNullOrWhiteSpace(definition.Id))
                root.name = definition.Id;

            return instance;
        }
    }
}