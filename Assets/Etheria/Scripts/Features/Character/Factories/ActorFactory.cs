using Etheria.Core.DI;
using Etheria.Game.Actor;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly ScopeHierarchy _scopeHierarchy;

        public ActorFactory(
            LifetimeScope parentScope,
            ScopeHierarchy scopeHierarchy)
        {
            _parentScope = parentScope;
            _scopeHierarchy = scopeHierarchy;
        }

        public LifetimeScope Create(
            LifetimeScope prefab,
            Vector3 position,
            Quaternion rotation = default)
        {
            var scope = _parentScope.CreateChildFromPrefab(prefab);

            if (rotation == default)
                rotation = Quaternion.identity;

            scope.transform.SetParent(
                _scopeHierarchy.ContentRoot,
                worldPositionStays: false);

            scope.transform.SetPositionAndRotation(position, rotation);

            return scope;
        }
    }
}
