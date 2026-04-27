using UnityEngine;

namespace Etheria.Gameplay
{
    public sealed class WorldPickup : Pickup
    {
        [SerializeField] private Transform _visualAnchor;

        private GameObject _spawnObject;
        private GameObject _spawnPrefab;

        protected override void OnRespawn()
        {
            RebuildSpawnObject();
        }

        private void RebuildSpawnObject()
        {
            if (_instance == null)
            {
                DestroySpawnObject();
                return;
            }

            var definition = _instance.Definition;
            if (definition == null)
            {
                DestroySpawnObject();
                return;
            }

            var fragment = definition.GetFragment<PickupVisualFragmentSO>();
            if (fragment == null || fragment.Prefab == null)
            {
                DestroySpawnObject();
                return;
            }

            if (_spawnObject == null || _spawnPrefab != fragment.Prefab)
            {
                var anchor = _visualAnchor != null ? _visualAnchor : transform;

                DestroySpawnObject();
                _spawnObject = Instantiate(fragment.Prefab, anchor);
                _spawnPrefab = fragment.Prefab;
            }

            ApplyLocalTransform(fragment);
        }


        private void ApplyLocalTransform(PickupVisualFragmentSO prefabFragment)
        {
            _spawnObject.transform.SetLocalPositionAndRotation(
                prefabFragment.LocalPosition,
                Quaternion.Euler(prefabFragment.LocalRotationEuler));

            _spawnObject.transform.localScale = prefabFragment.LocalScale;
        }

        private void DestroySpawnObject()
        {
            if (_spawnObject != null)
                Destroy(_spawnObject);

            _spawnObject = null;
            _spawnPrefab = null;
        }
    }
}

