using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Pickup
{
    public sealed class ItemPickupInteraction :
        IInteractable
    {
        private readonly ICollectable _collectable;
        private readonly IItemPickupService _pickupService;
        private readonly ItemPickupInteractionSettings _settings;

        public ItemPickupInteraction(
            ICollectable collectable,
            IItemPickupService pickupService,
            ItemPickupInteractionSettings settings)
        {
            _collectable = collectable
                ?? throw new ArgumentNullException(nameof(collectable));

            _pickupService = pickupService
                ?? throw new ArgumentNullException(
                    nameof(pickupService));

            _settings = settings;
        }

        public Vector3 InteractionPoint =>
            _settings.InteractionAnchor.position;

        public float MaxRange =>
            _settings.MaxRange;

        public bool CanInteract(
            InteractionContext context)
        {
            return context.InteractorInstanceId != Guid.Empty &&
                   context.TargetInstanceId ==
                   _collectable.InstanceId &&
                   _collectable.CanCollect(
                       context.InteractorInstanceId);
        }

        public async UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (!CanInteract(context))
            {
                return InteractionResult.Rejected;
            }

            var result = await _pickupService.CollectAsync(
                context.InteractorInstanceId,
                _collectable,
                token);

            return result switch
            {
                CollectResult.Succeeded =>
                    InteractionResult.Completed,

                CollectResult.AlreadyInProgress =>
                    InteractionResult.Busy,

                _ => InteractionResult.Rejected
            };
        }
    }

    public readonly struct ItemPickupInteractionSettings
    {
        public ItemPickupInteractionSettings(
            Transform interactionAnchor,
            float maxRange)
        {
            InteractionAnchor = interactionAnchor;
            MaxRange = maxRange;
        }

        public Transform InteractionAnchor { get; }

        public float MaxRange { get; }
    }
}