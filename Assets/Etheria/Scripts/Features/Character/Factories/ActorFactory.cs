using Etheria.Game.Actor;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly LifetimeScope _parentScope;

        public ActorFactory(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public LifetimeScope Create(
            LifetimeScope prefab,
            Vector3 position,
            Quaternion rotation = default)
        {
            var scope = _parentScope.CreateChildFromPrefab(prefab);

            if (rotation == default)
                rotation = Quaternion.identity;

            scope.transform.SetPositionAndRotation(position, rotation);

            return scope;
        }
    }
}