using System;
using Game.CommandSystem;
using Game.Core;
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
        private readonly IDisplayNameRegistrationService _displayNames;
        private readonly IInstanceRegistry<ICommandReceiver> _commandReceivers;

        public PickupFactory(
            IItemPickupService pickupService,
            IInventoryService inventories,
            IDisplayNameRegistrationService displayNames,
            IInstanceRegistry<ICommandReceiver> commandReceivers)
        {
            _pickupService = pickupService;
            _inventories = inventories;
            _displayNames = displayNames;
            _commandReceivers = commandReceivers;
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

                var commandReceiver = new WorldCommandReceiver(
                    pickupInstance,
                    new IWorldCommandHandler[]
                    {
                        new InteractCommandHandler(interactable),
                    });

                spawnedObject.Add(
                    _commandReceivers.Register(
                        pickupInstance.InstanceId,
                        commandReceiver));

                spawnedObject.Add(
                    _displayNames.Register(
                        pickupInstance.InstanceId,
                        new DisplayNameProvider(
                            () => definition.DisplayName)));

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