using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private WorldPickup _pickup;
        [SerializeField] private bool _respawns = true;
        [SerializeField] private float _respawnCooldown = 10f;

        private bool _isRespawnPending;

        private void Awake()
        {
            if (_pickup != null)
            {
                _pickup.Collected += OnPickupCollected;
            }
        }

        private void OnDestroy()
        {
            if (_pickup != null)
            {
                _pickup.Collected -= OnPickupCollected;
            }
        }

        private void OnPickupCollected(WorldPickup pickup)
        {
            if (!_respawns || _isRespawnPending)
                return;

            RespawnRoutineAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid RespawnRoutineAsync(CancellationToken cancellationToken)
        {
            _isRespawnPending = true;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_respawnCooldown), cancellationToken: cancellationToken);
                _pickup?.Respawn();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRespawnPending = false;
            }
        }
    }
}
