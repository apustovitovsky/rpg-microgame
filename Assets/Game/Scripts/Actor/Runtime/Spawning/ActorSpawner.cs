using System;
using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorSpawner : IActorSpawner
    {
        private readonly LifetimeScope _parentScope;
        private readonly ScopeRoot _scopeRoot;

        public ActorSpawner(
            LifetimeScope parentScope,
            ScopeRoot scopeRoot)
        {
            _parentScope = parentScope;
            _scopeRoot = scopeRoot;
        }

        public IActorView Spawn(
            string actorId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            actorId = actorId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(actorId))
                throw new ArgumentException("Actor id is required.", nameof(actorId));

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (!prefab.TryGetComponent<LifetimeScope>(out var prefabScope))
            {
                throw new InvalidOperationException(
                    $"Actor prefab '{prefab.name}' must have {nameof(LifetimeScope)} on root.");
            }

            var context = new ActorSpawnContext(actorId);

            var scope = _parentScope.CreateChildFromPrefab(
                prefabScope,
                builder => builder.RegisterInstance(context));

            scope.transform.SetParent(
                parent != null ? parent : _scopeRoot.Transform,
                worldPositionStays: false);

            scope.transform.SetPositionAndRotation(position, rotation);

            return scope.GetComponentInChildren<IActorView>(true)
                   ?? throw new InvalidOperationException(
                       $"Actor prefab '{prefab.name}' has no {nameof(IActorView)}.");
        }
    }
}