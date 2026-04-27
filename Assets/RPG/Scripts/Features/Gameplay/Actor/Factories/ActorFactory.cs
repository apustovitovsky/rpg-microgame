using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IActorPoolRoots _roots;

        public ActorFactory(
            LifetimeScope parentScope,
            IActorPoolRoots roots)
        {
            _parentScope = parentScope;
            _roots = roots;
        }

        public LifetimeScope Create(
            LifetimeScope prefab,
            Vector3 position,
            Quaternion rotation = default)
        {
            var scope = _parentScope.CreateChildFromPrefab(prefab);

            var transform = scope.transform;
            transform.SetParent(_roots.ActiveRoot, false);
            transform.SetPositionAndRotation(position, rotation);

            return scope;
        }
    }
}
