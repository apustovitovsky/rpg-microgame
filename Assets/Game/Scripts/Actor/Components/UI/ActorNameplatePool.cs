using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorNameplatePool
    {
        private readonly IObjectPool<ActorNameplateView> _pool;

        public ActorNameplatePool(
            IObjectResolver resolver,
            ActorNameplateView prefab,
            IActorNameplatePoolRoots roots)
        {
            _pool = new ObjectPool<ActorNameplateView>(
                createFunc: () =>
                {
                    ActorNameplateView instance = resolver.Instantiate(prefab, roots.InactiveRoot);
                    instance.gameObject.SetActive(false);
                    return instance;
                },
                actionOnGet: view =>
                {
                    view.transform.SetParent(roots.ActiveRoot, false);
                    view.transform.SetAsLastSibling();
                    view.gameObject.SetActive(true);
                },
                actionOnRelease: view =>
                {
                    view.Clear();
                    view.gameObject.SetActive(false);
                    view.transform.SetParent(roots.InactiveRoot, false);
                },
                actionOnDestroy: view =>
                {
                    if (view != null)
                        Object.Destroy(view.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: 8,
                maxSize: 64);
        }

        public ActorNameplateView Get(Transform anchor, string text, Camera camera)
        {
            ActorNameplateView view = _pool.Get();
            view.Bind(anchor, text, camera);
            return view;
        }

        public void Release(ActorNameplateView view)
        {
            if (view != null)
                _pool.Release(view);
        }
    }
}