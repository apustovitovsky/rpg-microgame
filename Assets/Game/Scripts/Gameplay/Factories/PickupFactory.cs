using System;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using Game.UI;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupFactory
    {
        private readonly IItemPickupService _pickupService;
        private readonly IInventoryService _inventories;
        private readonly IInteractionRegistrationService _interactions;
        private readonly IDisplayNameRegistrationService _displayNames;

        public PickupFactory(
            IItemPickupService pickupService,
            IInventoryService inventories,
            IInteractionRegistrationService interactions,
            IDisplayNameRegistrationService displayNames)
        {
            _pickupService = pickupService;
            _inventories = inventories;
            _interactions = interactions;
            _displayNames = displayNames;
        }

        public ISpawnedObject Create(PickupSpawnRequest request)
        {
            var pickupInstance = request.Instance;
            var definition = pickupInstance.Definition;

            if (definition.Prefab == null)
            {
                throw new InvalidOperationException(
                    "Pickup prefab is required.");
            }

            if (definition.Item == null)
            {
                throw new InvalidOperationException(
                    "Pickup item definition is required.");
            }

            if (definition.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Pickup amount must be greater than zero.");
            }

            var gameObject = UnityEngine.Object.Instantiate(
                definition.Prefab,
                request.Position,
                request.Rotation,
                request.Parent);

            gameObject.name =
                $"{definition.DisplayName} " +
                $"({pickupInstance.InstanceId:N})";

            ISpawnedObject spawnedObject = new SpawnedObject(
                pickupInstance,
                gameObject);

            try
            {
                var collectable = gameObject
                    .GetComponentInChildren<ItemPickupCollectable>(true);

                if (collectable == null)
                {
                    throw new InvalidOperationException(
                        $"Pickup prefab '{definition.Prefab.name}' has no " +
                        $"{nameof(ItemPickupCollectable)}.");
                }

                var interactable = gameObject
                    .GetComponentInChildren<ItemPickupInteractable>(true);

                if (interactable == null)
                {
                    throw new InvalidOperationException(
                        $"Pickup prefab '{definition.Prefab.name}' has no " +
                        $"{nameof(ItemPickupInteractable)}.");
                }

                var targetable = gameObject
                    .GetComponentInChildren<Targetable>(true);

                if (targetable == null)
                {
                    throw new InvalidOperationException(
                        $"Pickup prefab '{definition.Prefab.name}' has no " +
                        $"{nameof(Targetable)}.");
                }

                collectable.Initialize(
                    pickupInstance.InstanceId,
                    definition,
                    _inventories);

                interactable.Initialize(_pickupService);

                targetable.Initialize(pickupInstance.InstanceId);

                spawnedObject.Add(
                    _displayNames.Register(
                        pickupInstance.InstanceId,
                        new DisplayNameProvider(
                            () => definition.DisplayName)));

                spawnedObject.Add(
                    _interactions.RegisterInteractable(
                        pickupInstance.InstanceId,
                        interactable));

                return spawnedObject;
            }
            catch
            {
                spawnedObject.Dispose();
                throw;
            }
        }
    }
}