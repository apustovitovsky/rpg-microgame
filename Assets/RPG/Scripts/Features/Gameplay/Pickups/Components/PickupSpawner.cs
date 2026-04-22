using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PickupSpawner : MonoBehaviour, IInitializable, IDisposable
    {
        [Tooltip("Pickup definition")]
        [field: SerializeField] public PickupDefinitionSO Definition { get; private set; }

        [SerializeField] private Pickup _pickup;
        [SerializeField] private bool _respawns = true;
        [SerializeField] private float _respawnCooldown = 10f;

        private bool _isRespawnPending;

        public void Initialize()
        {
            if (_pickup != null)
            {
                _pickup.Collected += OnPickupCollected;
                _pickup.Respawn();
            }
        }

        private void OnPickupCollected(Pickup pickup)
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
                if (_pickup != null) _pickup.Respawn();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isRespawnPending = false;
            }
        }
        public void Dispose()
        {
            if (_pickup != null)
            {
                _pickup.Collected -= OnPickupCollected;
            }
        }
    }
}
