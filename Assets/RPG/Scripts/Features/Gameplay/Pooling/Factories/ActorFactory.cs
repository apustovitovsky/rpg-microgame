namespace RPG.Gameplay.Pooling
{
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;

    public sealed class ActorFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly IActorPoolRoots _roots;
        private readonly GameObject _prefab;

        public ActorFactory(
            IObjectResolver resolver,
            IActorPoolRoots roots,
            GameObject prefab)
        {
            _resolver = resolver;
            _roots = roots;
            _prefab = prefab;
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var instance = Object.Instantiate(_prefab, position, rotation, _roots.ActiveRoot);
            _resolver.Inject(instance);
            return instance;
        }
    }
}