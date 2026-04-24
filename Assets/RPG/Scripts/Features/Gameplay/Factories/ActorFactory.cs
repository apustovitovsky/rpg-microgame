using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly LifetimeScope _parentScope;

        public ActorFactory(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public GameObject Create(GameObject prefab, Vector3 position, Quaternion rotation = default)
        {
            GameObject instance;

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                instance = Object.Instantiate(prefab, position, rotation);
                instance.name = prefab.name;
            }

            return instance;
        }
    }
}
