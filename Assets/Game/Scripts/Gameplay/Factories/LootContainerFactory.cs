using System;
using Game.CommandSystem;
using Game.Core;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using Game.UI;
using Game.World;

namespace Game.Loot
{
    public sealed class LootContainerFactory
    {
        private readonly ILootSessionService _sessions;
        private readonly IInventoryRegistrationService _inventories;
        private readonly IInstanceRegistry<ICommandReceiver> _commandReceivers;
        private readonly IDisplayNameRegistrationService _displayNames;

        public LootContainerFactory(
            ILootSessionService sessions,
            IInventoryRegistrationService inventories,
            IInstanceRegistry<ICommandReceiver> commandReceivers,
            IDisplayNameRegistrationService displayNames)
        {
            _sessions = sessions;
            _inventories = inventories;
            _commandReceivers = commandReceivers;
            _displayNames = displayNames;
        }

        public ISpawnedObject Create(
            LootContainerSpawnRequest request)
        {
            var containerInstance = request.Instance;
            var definition = containerInstance.Definition;

            if (definition.Prefab == null)
            {
                throw new InvalidOperationException(
                    "Loot container prefab is required.");
            }

            var gameObject = UnityEngine.Object.Instantiate(
                definition.Prefab,
                request.Position,
                request.Rotation,
                request.Parent);

            gameObject.name =
                $"{definition.DisplayName} " +
                $"({containerInstance.InstanceId:N})";

            ISpawnedObject spawnedObject = new SpawnedObject(
                containerInstance,
                gameObject);

            try
            {
                var interactable = gameObject
                    .GetComponentInChildren<LootInteractable>(true);

                if (interactable == null)
                {
                    throw new InvalidOperationException(
                        $"Loot container prefab " +
                        $"'{definition.Prefab.name}' has no " +
                        $"{nameof(LootInteractable)}.");
                }

                var targetable = gameObject
                    .GetComponentInChildren<Targetable>(true);

                if (targetable == null)
                {
                    throw new InvalidOperationException(
                        $"Loot container prefab " +
                        $"'{definition.Prefab.name}' has no " +
                        $"{nameof(Targetable)}.");
                }

                interactable.Initialize(
                    containerInstance.InstanceId,
                    _sessions);

                targetable.Initialize(
                    containerInstance.InstanceId);

                var commandReceiver = new WorldCommandReceiver(
                    containerInstance,
                    new IWorldCommandHandler[]
                    {
                        new InteractCommandHandler(interactable),
                    });

                spawnedObject.Add(
                    _inventories.Register(
                        containerInstance));

                spawnedObject.Add(
                    _commandReceivers.Register(
                        containerInstance.InstanceId,
                        commandReceiver));

                spawnedObject.Add(
                    _displayNames.Register(
                        containerInstance.InstanceId,
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