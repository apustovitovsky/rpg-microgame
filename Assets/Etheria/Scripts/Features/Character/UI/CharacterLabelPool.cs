using Etheria.Game.UI;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    public sealed class CharacterLabelPool
    {
        private readonly IObjectPool<CharacterLabelView> _pool;

        public CharacterLabelPool(
            IObjectResolver resolver,
            CharacterLabelView prefab,
            ICharacterLabelPoolRoots roots)
        {
            _pool = new ObjectPool<CharacterLabelView>(
                createFunc: () =>
                {
                    CharacterLabelView instance = resolver.Instantiate(
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

        public CharacterLabelView Get(
            Transform anchor,
            string displayName,
            Camera camera)
        {
            CharacterLabelView view = _pool.Get();
            view.Bind(anchor, displayName, camera);
            return view;
        }

        public void Release(CharacterLabelView view)
        {
            _pool.Release(view);
        }
    }
}