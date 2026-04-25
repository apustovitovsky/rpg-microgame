using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class WorldPickup : Pickup
    {
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField, Min(0f)] private float _autoRespawnCooldown;

        private GameObject _spawnObject;
        private CancellationTokenSource _respawnCts;

        public override void PrepareForRelease()
        {
            CancelScheduledRespawn();
            DestroySpawnObject();
            base.PrepareForRelease();
        }

        protected override void OnRespawn()
        {
            RebuildSpawnObject();
        }

        private void RebuildSpawnObject()
        {
            DestroySpawnObject();

            if (_instance == null)
                return;

            var definition = _instance.Definition;

            if (definition == null)
                return;

            var prefabFragment = definition.GetFragment<PickupVisualFragmentSO>();

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

            if (_autoRespawnCooldown > 0f)
            {
                ScheduleRespawn();
                return;
            }

            base.OnCollect();
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
    }
}

