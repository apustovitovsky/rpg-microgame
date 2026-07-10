using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private ItemPickupCollectable _collectable;
        [SerializeField] private Transform _interactionAnchor;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        private IItemPickupService _pickupService;

        public Vector3 InteractionPoint =>
            _interactionAnchor != null
                ? _interactionAnchor.position
                : transform.position;

        public void Initialize(IItemPickupService pickupService)
        {
            _pickupService = pickupService;
        }

        public bool CanInteract(InteractionContext context)
        {
            return _pickupService != null &&
                   _collectable != null &&
                   context.TargetInstanceId ==
                   _collectable.InstanceId &&
                   _collectable.CanCollect(
                       context.InteractorInstanceId);
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
                return;

            var result = await _pickupService.CollectAsync(
                context.InteractorInstanceId,
                _collectable,
                token);

            if (result != CollectResult.Succeeded)
                Debug.LogWarning(
                    $"Item pickup failed: {result}.",
                    this);
        }
    }
}