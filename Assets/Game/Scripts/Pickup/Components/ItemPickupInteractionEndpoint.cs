using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupInteractionEndpoint :
        MonoBehaviour,
        IInteractable,
        IPrefabInstaller
    {
        [SerializeField] private Transform _interactionAnchor;

        [field: SerializeField]
        public float MaxRange { get; private set; } = 5f;

        private ItemPickupEndpoint _pickup;
        private IItemPickupService _pickupService;

        public Vector3 InteractionPoint =>
            _interactionAnchor != null
                ? _interactionAnchor.position
                : transform.position;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<IInteractable>();

            builder.RegisterBinding<IInteractable>();
        }

        [Inject]
        public void Construct(
            ItemPickupEndpoint pickup,
            IItemPickupService pickupService)
        {
            _pickup = pickup
                != null ? pickup : throw new ArgumentNullException(nameof(pickup));

            _pickupService = pickupService
                ?? throw new ArgumentNullException(nameof(pickupService));
        }

        public bool CanInteract(InteractionContext context)
        {
            return _pickupService != null &&
                   _pickup != null &&
                   context.TargetInstanceId ==
                   _pickup.InstanceId &&
                   _pickup.CanCollect(
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
                _pickup,
                token);

            if (result != CollectResult.Succeeded)
            {
                Debug.LogWarning(
                    $"Item pickup failed: {result}.",
                    this);
            }
        }
    }
}