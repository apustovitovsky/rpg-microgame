using System;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using Game.UI;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IInteractionRegistrationService _interactions;
        private readonly IInventoryRegistrationService _inventories;
        private readonly IDisplayNameRegistrationService _displayNames;
        private readonly IInstanceRegistry<IPossessable> _possessables;
        private readonly IInstanceRegistry<ITargetProvider> _targetProviders;

        public ActorFactory(
            LifetimeScope parentScope,
            IInteractionRegistrationService interactions,
            IInventoryRegistrationService inventories,
            IDisplayNameRegistrationService displayNames,
            IInstanceRegistry<IPossessable> possessables,
            IInstanceRegistry<ITargetProvider> targetProviders)
        {
            _parentScope = parentScope;
            _interactions = interactions;
            _inventories = inventories;
            _displayNames = displayNames;
            _possessables = possessables;
            _targetProviders = targetProviders;
        }

        public ISpawnedObject Create(ActorSpawnRequest request)
        {
            var actorInstance = request.Instance;
            var definition = actorInstance.Definition;

            if (definition.Prefab == null)
            {
                throw new ArgumentException(
                    "Actor definition prefab is required.",
                    nameof(request));
            }

            using (LifetimeScope.EnqueueParent(_parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                builder.RegisterInstance(actorInstance);
            }))
            {
                var gameObject = UnityEngine.Object.Instantiate(
                    definition.Prefab,
                    request.Position,
                    request.Rotation,
                    request.Parent);

                gameObject.name = definition.DefinitionId;

                ISpawnedObject spawnedObject = new SpawnedObject(
                    actorInstance,
                    gameObject);

                try
                {
                    var scope = gameObject
                        .GetComponentInChildren<ActorModule>(true);

                    if (scope == null)
                    {
                        throw new InvalidOperationException(
                            $"Actor prefab '{definition.Prefab.name}' " +
                            $"has no {nameof(ActorModule)}.");
                    }

                    if (scope.Container == null)
                    {
                        throw new InvalidOperationException(
                            $"Actor prefab '{definition.Prefab.name}' " +
                            "has no built VContainer scope.");
                    }

                    if (scope.Container.TryResolve<Targetable>(
                            out var targetable))
                    {
                        targetable.Initialize(
                            actorInstance.InstanceId);
                    }

                    if (scope.Container.TryResolve<IPossessable>(
                            out var possessable))
                    {
                        spawnedObject.Add(
                            _possessables.Register(
                                actorInstance.InstanceId,
                                possessable));
                    }

                    if (scope.Container.TryResolve<ITargetProvider>(
                            out var targetProvider))
                    {
                        spawnedObject.Add(
                            _targetProviders.Register(
                                actorInstance.InstanceId,
                                targetProvider));
                    }

                    spawnedObject.Add(
                        _displayNames.Register(
                            actorInstance.InstanceId,
                            new DisplayNameProvider(
                                () => definition.DisplayName)));

                    if (scope.Container.TryResolve<IInventory>(
                            out var inventory))
                    {
                        var owner = new InventoryOwner(
                            actorInstance.InstanceId,
                            inventory);

                        spawnedObject.Add(
                            _inventories.Register(owner));
                    }

                    if (scope.Container.TryResolve<IInteractable>(
                            out var interactable))
                    {
                        spawnedObject.Add(
                            _interactions.RegisterInteractable(
                                actorInstance.InstanceId,
                                interactable));
                    }

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
}