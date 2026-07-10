using System;
using Game.Interaction;
using Game.Inventory;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IActorRegistrationService _actors;
        private readonly IInteractionRegistrationService _interactions;
        private readonly IInventoryRegistrationService _inventories;

        public ActorFactory(
            LifetimeScope parentScope,
            IActorRegistrationService actors,
            IInteractionRegistrationService interactions,
            IInventoryRegistrationService inventories)
        {
            _parentScope = parentScope;
            _actors = actors;
            _interactions = interactions;
            _inventories = inventories;
        }

        public IWorldObject Create(ActorSpawnRequest request)
        {
            if (request.WorldId.IsEmpty)
                throw new ArgumentException(
                    "Actor world id is required.",
                    nameof(request));

            if (request.Definition == null)
                throw new ArgumentNullException(nameof(request.Definition));

            if (request.Definition.Prefab == null)
                throw new ArgumentNullException(nameof(request.Definition.Prefab));

            var info = new WorldInfo(
                request.WorldId,
                request.Definition.DisplayName);

            using (LifetimeScope.EnqueueParent(_parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                builder.Register<IWorldActor, WorldActor>(Lifetime.Scoped)
                    .WithParameter(info);
            }))
            {
                var instance = UnityEngine.Object.Instantiate(
                    request.Definition.Prefab,
                    request.Position,
                    request.Rotation,
                    request.Parent);

                instance.name =
                    $"{request.Definition.DisplayName} ({request.WorldId})";

                var scope =
                    instance.GetComponentInChildren<ActorModule>(true);

                if (scope == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Definition.Prefab.name}' " +
                        $"has no {nameof(ActorModule)}.");
                }

                if (scope.Container == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Definition.Prefab.name}' " +
                        "has no built VContainer scope.");
                }

                if (scope.Container.TryResolve<Targetable>(out var targetable))
                    targetable.Initialize(info);

                var transform =
                    scope.Container.Resolve<IActorTransform>();

                var actor =
                    scope.Container.Resolve<IWorldActor>();

                scope.Container.TryResolve<IInteractable>(
                    out var interactable);

                var lifetime = new WorldObject(
                    transform.Root.gameObject,
                    info);

                lifetime.Add(_actors.Register(actor));

                if (scope.Container.TryResolve<IInventory>(out var inventory))
                {
                    var owner = new InventoryOwner(
                        request.WorldId,
                        inventory);

                    lifetime.Add(_inventories.Register(owner));
                }

                if (interactable != null)
                {
                    lifetime.Add(
                        _interactions.RegisterInteractable(
                            request.WorldId,
                            interactable));
                }

                return lifetime;
            }
        }
    }
}