using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Features.Pooling;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Collectables
{
    public sealed class PickupPad : MonoBehaviour
    {
        [SerializeField] private PickupDefinitionSO _pickupDefinition;
        [SerializeField] private bool _autoRespawn = true;
        [SerializeField, Min(MinRespawnCooldown)] private int _respawnCooldown = 5;

        private const int MinRespawnCooldown = 1;
        private PickupPool _pickupPool;
        private Pickup _activePickup;
        private CancellationTokenSource _respawnCts;

        [Inject]
        public void Construct(PickupPool pickupPool)
        {
            _pickupPool = pickupPool
                ?? throw new ArgumentNullException(nameof(pickupPool));
        }

        private void Start()
        {
            SpawnPickup();
        }

        private void OnDestroy()
        {
            UnsubscribeFromActivePickup();
            CancelScheduledRespawn();
        }

        private void SpawnPickup()
        {
            if (_pickupPool == null || _pickupDefinition == null || _activePickup != null)
                return;

            _activePickup = _pickupPool.Get(_pickupDefinition, transform.position, transform.rotation);
            _activePickup.Collected += HandlePickupCollected;
        }

        private void HandlePickupCollected(Pickup pickup)
        {
            if (_activePickup != pickup)
                return;

            _activePickup.Collected -= HandlePickupCollected;
            _activePickup = null;

            if (_autoRespawn)
                ScheduleRespawn();
        }

        private void UnsubscribeFromActivePickup()
        {
            if (_activePickup == null)
                return;

            _activePickup.Collected -= HandlePickupCollected;
            _activePickup = null;
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
            var delaySeconds = Mathf.Max(_respawnCooldown, MinRespawnCooldown);

            await UniTask.Delay(
                TimeSpan.FromSeconds(delaySeconds),
                cancellationToken: token);

            _respawnCts = null;
            SpawnPickup();
        }
    }
}

