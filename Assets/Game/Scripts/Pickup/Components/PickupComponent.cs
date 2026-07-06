using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Targeting;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupComponent :
        MonoBehaviour,
        ITargetable,
        IPickup
    {
        [SerializeField] private string _displayName = "Pickup";
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        private WorldId _worldId;

        public WorldId WorldId => _worldId;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? WorldId.ToString()
            : _displayName.Trim();

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable =>
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            _isTargetable &&
            !WorldId.IsEmpty;

        public void Initialize(WorldId worldId)
        {
            _worldId = worldId;
        }

        public bool CanCollect(PickupContext context)
        {
            return context.Pickup != null &&
                   ReferenceEquals(context.Pickup, this) &&
                   !WorldId.IsEmpty;
        }

        public UniTask CollectAsync(
            PickupContext context,
            CancellationToken token)
        {
            Debug.Log(
                $"Picked up '{DisplayName}'.",
                this);

            gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }
    }
}