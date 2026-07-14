using System;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorSpawner :
        IActorSpawner
    {
        private readonly IWorldSpawner _worldSpawner;

        public ActorSpawner(IWorldSpawner worldSpawner)
        {
            _worldSpawner = worldSpawner
                ?? throw new ArgumentNullException(nameof(worldSpawner));
        }

        public ActorInstance Spawn(ActorSpawnRequest request)
        {
            var definition = request.Definition != null
                ? request.Definition
                : throw new ArgumentNullException(
                    nameof(request),
                    "Actor spawn request requires a definition.");

            var instanceId = request.InstanceId ?? Guid.NewGuid();

            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Actor instance id cannot be empty.",
                    nameof(request));
            }

            var instance = definition.CreateInstance(instanceId);

            var root = _worldSpawner.Spawn(
                instance.InstanceId,
                definition.Prefab,
                request.Placement,
                new WorldInstanceInstaller<ActorInstance>(instance));

            if (!string.IsNullOrWhiteSpace(definition.Id))
                root.name = definition.Id;

            return instance;
        }
    }
}