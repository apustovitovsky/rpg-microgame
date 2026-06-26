using Etheria.Core.DI;
using Etheria.Game.Actor;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly ScopeRoot _scopeContentRoot;

        public ActorFactory(
            LifetimeScope parentScope,
            ScopeRoot scopeContentRoot)
        {
            _parentScope = parentScope;
            _scopeContentRoot = scopeContentRoot;
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
                _scopeContentRoot.Transform,
                worldPositionStays: false);

            scope.transform.SetPositionAndRotation(position, rotation);

            return scope;
        }
    }
}
