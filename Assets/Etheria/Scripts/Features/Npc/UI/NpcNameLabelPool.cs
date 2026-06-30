using Etheria.Game.UI;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    public sealed class NpcNameLabelPool
    {
        private readonly IObjectPool<NpcNameLabelView> _pool;

        public NpcNameLabelPool(
            IObjectResolver resolver,
            NpcNameLabelView prefab,
            INpcNameLabelPoolRoots roots)
        {
            _pool = new ObjectPool<NpcNameLabelView>(
                createFunc: () =>
                {
                    NpcNameLabelView instance = resolver.Instantiate(
                        prefab,
                        roots.InactiveRoot);

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
                    Object.Destroy(view.gameObject),
                collectionCheck: true,
                defaultCapacity: 8,
                maxSize: 64);
        }

        public NpcNameLabelView Get(
            Transform anchor,
            string displayName,
            Camera camera)
        {
            NpcNameLabelView view = _pool.Get();
            view.Bind(anchor, displayName, camera);
            return view;
        }

        public void Release(NpcNameLabelView view)
        {
            _pool.Release(view);
        }
    }
}