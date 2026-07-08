using System;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    public sealed class PickupWorldObjectFactory :
        IWorldObjectFactory<PickupSpawnRequest>
    {
        private readonly LifetimeScope _parentScope;
        private readonly IObjectResolver _resolver;
        private readonly IWorldRegistry<IWorldObject> _worldObjects;
        private readonly IWorldRegistry<IWorldPickup> _pickups;
        private readonly IWorldRegistry<IInteractable> _interactions;
        private readonly IWorldRegistry<ITargetable> _targets;

        public PickupWorldObjectFactory(
            LifetimeScope parentScope,
            IObjectResolver resolver,
            IWorldRegistry<IWorldObject> worldObjects,
            IWorldRegistry<IWorldPickup> pickups,
            IWorldRegistry<IInteractable> interactions,
            IWorldRegistry<ITargetable> targets)
        {
            _parentScope = parentScope;
            _resolver = resolver;
            _worldObjects = worldObjects;
            _pickups = pickups;
            _interactions = interactions;
            _targets = targets;
        }

        public WorldSpawnResult Create(PickupSpawnRequest request)
        {
            if (request.WorldId.IsEmpty)
                throw new ArgumentException("Pickup world id is required.", nameof(request));

            if (request.Definition == null)
                throw new ArgumentNullException(nameof(request.Definition));

            if (request.Definition.Prefab == null)
                throw new ArgumentNullException(nameof(request.Definition.Prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(
                    request.Definition.Prefab,
                    request.Position,
                    request.Rotation,
                    request.Parent);

                instance.name = $"{request.Definition.DisplayName} ({request.WorldId})";

                var pickup = instance.GetComponentInChildren<WorldPickup>(true);

                if (pickup == null)
                    throw new InvalidOperationException(
                        $"Pickup prefab '{request.Definition.Prefab.name}' has no {nameof(WorldPickup)}.");

                pickup.Initialize(
                    request.WorldId,
                    request.Definition);

                if (pickup.TryGetComponent<PickupInteract>(out var pickupInteract))
                    _resolver.Inject(pickupInteract);

                var worldObject = new WorldObject(
                    request.WorldId,
                    pickup.gameObject);

                var lifetime = new CompositeRegistration();

                lifetime.Add(_worldObjects.Register(request.WorldId, worldObject));
                lifetime.Add(_pickups.Register(request.WorldId, pickup));

                if (pickup.TryGetComponent<IInteractable>(out var interactable))
                    lifetime.Add(_interactions.Register(request.WorldId, interactable));

                if (pickup.TryGetComponent<ITargetable>(out var targetable))
                    lifetime.Add(_targets.Register(request.WorldId, targetable));

                return new WorldSpawnResult(
                    worldObject,
                    lifetime);
            }
        }
    }
}