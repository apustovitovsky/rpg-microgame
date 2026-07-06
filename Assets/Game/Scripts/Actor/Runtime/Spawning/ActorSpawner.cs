using System;
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

        public ActorInstance Spawn(
            string instanceId,
            string definitionId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            instanceId = instanceId?.Trim() ?? string.Empty;
            definitionId = definitionId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Actor instance id is required.", nameof(instanceId));

            if (string.IsNullOrWhiteSpace(definitionId))
                throw new ArgumentException("Actor definition id is required.", nameof(definitionId));

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(
                    prefab,
                    position,
                    rotation,
                    parent);

                instance.name = $"{definitionId} ({instanceId})";

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

                identity.Initialize(instanceId, definitionId);

                var factory = scope.Container.Resolve<ActorInstanceFactory>()
                    ?? throw new InvalidOperationException(
                        $"Actor prefab '{prefab.name}' has no {nameof(ActorInstanceFactory)}.");

                return factory.Create(instanceId, definitionId);
            }
        }
    }
}