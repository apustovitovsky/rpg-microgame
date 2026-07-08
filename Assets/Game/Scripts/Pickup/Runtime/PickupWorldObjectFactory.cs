using System;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    public sealed class PickupWorldObjectFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IObjectResolver _resolver;
        private readonly PickupWorldRegistrar _registrar;

        public PickupWorldObjectFactory(
            LifetimeScope parentScope,
            IObjectResolver resolver,
            PickupWorldRegistrar registrar)
        {
            _parentScope = parentScope;
            _resolver = resolver;
            _registrar = registrar;
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

                pickup.TryGetComponent<IInteractable>(out var interactable);
                pickup.TryGetComponent<ITargetable>(out var targetable);

                var spawnedPickup = new PickupSpawnedObject(
                    worldObject,
                    pickup,
                    pickup,
                    pickup,
                    interactable,
                    targetable);

                return new WorldSpawnResult(
                    worldObject,
                    _registrar.Register(spawnedPickup));
            }
        }
    }
}