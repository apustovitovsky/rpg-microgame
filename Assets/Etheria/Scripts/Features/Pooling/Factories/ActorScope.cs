using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Camera.Pooling
{
    public sealed class Actor : MonoBehaviour
    {
        private IObjectPool<Actor> _pool;

        public void SetPool(IObjectPool<Actor> pool)
        {
            _pool = pool;
        }

        public void Release()
        {
            _pool?.Release(this);
        }
    }

    public sealed class ActorScope : LifetimeScope
    {
        [SerializeField] private Actor projectilePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ObjectPool<Actor>>(resolver =>
            {
                var sceneRoot = resolver.Resolve<LifetimeScope>();
                var poolRoot = sceneRoot.transform;

                ObjectPool<Actor> pool = null;

                pool = new ObjectPool<Actor>(
                    createFunc: () =>
                    {
                        var instance = Instantiate(projectilePrefab, poolRoot);
                        instance.SetPool(pool);
                        instance.gameObject.SetActive(false);
                        return instance;
                    },
                    actionOnGet: projectile =>
                    {
                        projectile.transform.SetParent(poolRoot, false);
                        projectile.gameObject.SetActive(true);
                    },
                    actionOnRelease: projectile =>
                    {
                        projectile.gameObject.SetActive(false);
                        projectile.transform.SetParent(poolRoot, false);
                    },
                    actionOnDestroy: projectile =>
                    {
                        Destroy(projectile.gameObject);
                    },
                    collectionCheck: true,
                    defaultCapacity: 16,
                    maxSize: 128);

                return pool;
            }, Lifetime.Scoped);
        }
    }
}

