using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.World
{
    public sealed class WorldSpawner :
        IWorldSpawner
    {
        private readonly LifetimeScope _parentScope;
        private readonly ISpawnedObjectRegistry _spawnedObjects;

        public WorldSpawner(
            LifetimeScope parentScope,
            ISpawnedObjectRegistry spawnedObjects)
        {
            _parentScope = parentScope
                != null ? parentScope : throw new ArgumentNullException(nameof(parentScope));

            _spawnedObjects = spawnedObjects
                ?? throw new ArgumentNullException(nameof(spawnedObjects));
        }

        public ISpawnedObject Spawn<TInstance>(
            SpawnRequest<TInstance> request)
            where TInstance : class, IWorldInstance
        {
            var definition = request.Definition
                != null ? request.Definition : throw new ArgumentNullException(
                    nameof(request),
                    "Spawn request requires a definition.");

            if (definition.Prefab == null)
            {
                throw new ArgumentException(
                    "World definition prefab is required.",
                    nameof(request));
            }

            if (request.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id cannot be empty.",
                    nameof(request));
            }

            var instanceId = request.InstanceId ?? Guid.NewGuid();

            if (_spawnedObjects.TryGet(instanceId, out _))
            {
                throw new InvalidOperationException(
                    $"World instance '{instanceId}' is already spawned.");
            }

            var instance = definition.CreateInstance(instanceId)
                ?? throw new InvalidOperationException(
                    $"World definition '{definition.name}' " +
                    "created a null instance.");

            using (LifetimeScope.EnqueueParent(_parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                builder.RegisterInstance(instance)
                    .AsSelf()
                    .As<IWorldInstance>();
            }))
            {
                var gameObject = UnityEngine.Object.Instantiate(
                    definition.Prefab,
                    request.Placement.Position,
                    request.Placement.Rotation,
                    request.Placement.Parent);

                if (!string.IsNullOrWhiteSpace(definition.Id))
                    gameObject.name = definition.Id;

                var spawnedObject = new SpawnedObject(
                    instance,
                    gameObject);

                if (_spawnedObjects.Track(spawnedObject))
                    return spawnedObject;

                throw new InvalidOperationException(
                    $"World instance '{instanceId}' " +
                    "could not be tracked.");
            }
        }

        public bool Despawn(Guid instanceId)
        {
            return _spawnedObjects.Despawn(instanceId);
        }
    }
}