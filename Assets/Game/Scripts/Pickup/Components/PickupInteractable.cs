using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private PickupComponent _pickup;
        [SerializeField] private Transform _interactionAnchor;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        private IPickupService _pickupService;

        public Vector3 InteractionPoint =>
            _interactionAnchor != null
                ? _interactionAnchor.position
                : transform.position;

        public void Initialize(IPickupService pickupService)
        {
            _pickupService = pickupService;
        }

        public bool CanInteract(InteractionContext context)
        {
            return _pickupService != null &&
                   _pickup != null &&
                   !context.InteractorWorldId.IsEmpty &&
                   !_pickup.WorldId.IsEmpty;
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
                return;

            var result = await _pickupService.CollectAsync(
                context.InteractorWorldId,
                _pickup,
                token);

            if (result != PickupResult.Succeeded)
            {
                Debug.LogWarning(
                    $"Pickup failed: {result}.",
                    this);
            }
        }
    }
}