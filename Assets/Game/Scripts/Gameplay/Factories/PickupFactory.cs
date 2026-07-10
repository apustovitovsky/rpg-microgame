using System;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupFactory
    {
        private readonly IItemPickupService _pickupService;
        private readonly IInventoryService _inventories;
        private readonly IInteractionRegistrationService _interactions;

        public PickupFactory(
            IItemPickupService pickupService,
            IInventoryService inventories,
            IInteractionRegistrationService interactions)
        {
            _pickupService = pickupService;
            _inventories = inventories;
            _interactions = interactions;
        }

        public IWorldObject Create(PickupSpawnRequest request)
        {
            if (request.WorldId.IsEmpty)
                throw new ArgumentException(
                    "Pickup world id is required.",
                    nameof(request));

            if (request.Definition == null)
                throw new ArgumentNullException(nameof(request.Definition));

            if (request.Definition.Prefab == null)
                throw new InvalidOperationException(
                    "Pickup prefab is required.");

            if (request.Definition.Item == null)
                throw new InvalidOperationException(
                    "Pickup item definition is required.");

            if (request.Definition.Amount <= 0)
                throw new InvalidOperationException(
                    "Pickup amount must be greater than zero.");

            var instance = UnityEngine.Object.Instantiate(
                request.Definition.Prefab,
                request.Position,
                request.Rotation,
                request.Parent);

            instance.name =
                $"{request.Definition.DisplayName} ({request.WorldId})";

            var collectable =
                instance.GetComponentInChildren<ItemPickupCollectable>(true);

            if (collectable == null)
            {
                throw new InvalidOperationException(
                    $"Pickup prefab '{request.Definition.Prefab.name}' has no " +
                    $"{nameof(ItemPickupCollectable)}.");
            }

            var interactable =
                instance.GetComponentInChildren<ItemPickupInteractable>(true);

            if (interactable == null)
            {
                throw new InvalidOperationException(
                    $"Pickup prefab '{request.Definition.Prefab.name}' has no " +
                    $"{nameof(ItemPickupInteractable)}.");
            }

            var targetable =
                instance.GetComponentInChildren<Targetable>(true);

            if (targetable == null)
            {
                throw new InvalidOperationException(
                    $"Pickup prefab '{request.Definition.Prefab.name}' has no " +
                    $"{nameof(Targetable)}.");
            }

            var info = new WorldInfo(
                request.WorldId,
                request.Definition.DisplayName);

            collectable.Initialize(
                request.WorldId,
                request.Definition,
                _inventories);

            interactable.Initialize(_pickupService);
            targetable.Initialize(info);

            var lifetime = new WorldObject(
                instance,
                info);

            lifetime.Add(
                _interactions.RegisterInteractable(
                    request.WorldId,
                    interactable));

            return lifetime;
        }
    }
}