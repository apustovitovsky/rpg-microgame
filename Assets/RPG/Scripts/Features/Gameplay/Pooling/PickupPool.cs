using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class PickupPool
    {
        private readonly IObjectPool<Pickup> _pool;

        public PickupPool(
            IObjectResolver resolver,
            Pickup prefab,
            IPickupPoolRoots roots)
        {
            _pool = new ObjectPool<Pickup>(
                createFunc: () =>
                {
                    var instance = resolver.Instantiate(prefab, roots.InactiveRoot);
                    instance.SetRelease(Release);
                    instance.gameObject.SetActive(false);
                    return instance;
                },
                actionOnGet: pickup =>
                {
                    pickup.transform.SetParent(roots.ActiveRoot, false);
                    pickup.gameObject.SetActive(true);
                },
                actionOnRelease: pickup =>
                {
                    pickup.gameObject.SetActive(false);
                    pickup.transform.SetParent(roots.InactiveRoot, false);
                },
                actionOnDestroy: pickup => Object.Destroy(pickup.gameObject),
                collectionCheck: true,
                defaultCapacity: 32,
                maxSize: 256);
        }

        public Pickup Get(PickupDefinitionSO definition, Vector3 position, Quaternion rotation)
        {
            var pickup = _pool.Get();

            pickup.transform.SetPositionAndRotation(position, rotation);
            pickup.SetInstance(new PickupInstance(definition));

            return pickup;
        }

        public void Release(Pickup pickup)
        {
            pickup.PrepareForRelease();
            _pool.Release(pickup);
        }
    }
}
