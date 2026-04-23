using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class WorldPickup : Pickup
    {
        [SerializeField] private PickupDefinitionSO _definition;
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField] private bool _destroyOnCollect = true;
        [SerializeField, Min(0f)] private float _autoRespawnCooldown;

        private GameObject _spawnObject;
        private CancellationTokenSource _respawnCts;
        private const string PreviewObjectName = "[Pickup Visual Preview]";


        private void Start()
        {
            if (_definition != null && _instance == null)
                Initialize(new PickupInstance(_definition));
        }

        public override void Initialize(IPickupInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _definition = instance.Definition;
            base.Initialize(instance);
        }

        protected override void OnRespawn()
        {
            RebuildSpawnObject();
        }

        private void RebuildSpawnObject()
        {
            DestroySpawnObject();

            if (_definition == null)
                return;

            var prefabFragment = _definition.GetFragment<PickupVisualFragmentSO>();

            if (prefabFragment == null || prefabFragment.Prefab == null)
                return;

            var anchor = _spawnAnchor != null ? _spawnAnchor : transform;
            _spawnObject = Instantiate(prefabFragment.Prefab, anchor);

            ApplyLocalTransform(prefabFragment);
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
            if (_spawnObject == null)
                return;

            Destroy(_spawnObject);
            _spawnObject = null;
        }

        protected override void OnCollect()
        {
            DestroySpawnObject();

            if (_destroyOnCollect)
            {
                Destroy(gameObject);
                return;
            }

            if (_autoRespawnCooldown > 0f)
                ScheduleRespawn();
        }

        private void ScheduleRespawn()
        {
            CancelScheduledRespawn();

            _respawnCts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());

            RespawnAfterDelay(_respawnCts.Token).Forget();
        }

        private void CancelScheduledRespawn()
        {
            if (_respawnCts == null)
                return;

            _respawnCts.Cancel();
            _respawnCts.Dispose();
            _respawnCts = null;
        }

        private async UniTaskVoid RespawnAfterDelay(CancellationToken token)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(_autoRespawnCooldown),
                cancellationToken: token);

            _respawnCts = null;
            Respawn();
        }

#if UNITY_EDITOR
        // private void OnValidate()
        // {
        //     if (Application.isPlaying)
        //         return;

        //     if (UnityEditor.EditorUtility.IsPersistent(gameObject))
        //         return;

        //     RebuildSpawnObjectImmediate();
        // }

        private void RebuildSpawnObjectImmediate()
        {
            DestroySpawnObjectImmediate();

            if (_definition == null)
                return;

            var prefabFragment = _definition.GetFragment<PickupVisualFragmentSO>();
            if (prefabFragment == null || prefabFragment.Prefab == null)
                return;

            var anchor = _spawnAnchor != null ? _spawnAnchor : transform;

            _spawnObject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(
                prefabFragment.Prefab,
                anchor);

            _spawnObject.name = PreviewObjectName;

            ApplyLocalTransform(prefabFragment);
        }

        private void DestroySpawnObjectImmediate()
        {
            if (_spawnObject == null)
                return;

            DestroyImmediate(_spawnObject);
            _spawnObject = null;
        }
#endif
    }
}
