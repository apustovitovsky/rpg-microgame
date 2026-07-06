using System;
using Game.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly LifetimeScope _parentScope;

        public ActorSpawner(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public WorldActor Spawn(
            WorldId worldId,
            string displayName,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            displayName = displayName?.Trim() ?? string.Empty;

            if (worldId.IsEmpty)
                throw new ArgumentException("Actor world id is required.", nameof(worldId));

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(
                    prefab,
                    position,
                    rotation,
                    parent);

                instance.name = string.IsNullOrWhiteSpace(displayName)
                    ? worldId.ToString()
                    : $"{displayName} ({worldId})";

                var scope = instance.GetComponentInChildren<ActorScope>(true);

                if (scope == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no {nameof(ActorScope)}.");
                }

                if (scope.Container == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no built VContainer scope.");
                }

                var identity = scope.Container.Resolve<IActorIdentity>()
                    ?? throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no {nameof(IActorIdentity)}.");

                identity.Initialize(
                    worldId,
                    displayName);

                var factory = scope.Container.Resolve<WorldActorFactory>()
                    ?? throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no {nameof(WorldActorFactory)}.");

                return factory.Create(
                    worldId,
                    displayName);
            }
        }
    }
}