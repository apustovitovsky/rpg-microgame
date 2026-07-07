using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;
using VContainer;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupInteract :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private PickupComponent _pickup;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        private IPickupService _pickupService;

        [Inject]
        public void Construct(IPickupService pickupService)
        {
            _pickupService = pickupService;
        }

        public bool CanInteract(InteractionContext context)
        {
            return _pickupService != null &&
                   _pickup != null &&
                   context.Interactor != null &&
                   !_pickup.WorldId.IsEmpty;
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
                return;

            var result = await _pickupService.CollectAsync(
                context.Interactor.WorldId,
                _pickup.WorldId,
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