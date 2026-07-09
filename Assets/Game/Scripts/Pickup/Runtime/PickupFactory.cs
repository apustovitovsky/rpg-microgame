using System;
using Game.Interaction;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    public sealed class PickupFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IObjectResolver _resolver;
        private readonly IPickupRegistrationService _pickups;
        private readonly IInteractionRegistrationService _interactions;

        public PickupFactory(
            LifetimeScope parentScope,
            IObjectResolver resolver,
            IPickupRegistrationService pickups,
            IInteractionRegistrationService interactions)
        {
            _parentScope = parentScope;
            _resolver = resolver;
            _pickups = pickups;
            _interactions = interactions;
        }

        public IWorldLifetime Create(PickupSpawnRequest request)
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

                var pickupComponent = instance.GetComponentInChildren<PickupComponent>(true);

                if (pickupComponent == null)
                    throw new InvalidOperationException(
                        $"Pickup prefab '{request.Definition.Prefab.name}' has no {nameof(PickupComponent)}.");

                var info = new WorldInfo(
                    request.WorldId,
                    request.Definition.DisplayName);

                var pickup = new WorldPickup(
                    info,
                    request.Definition);

                pickupComponent.Initialize(pickup);

                if (pickupComponent.TryGetComponent<PickupInteractable>(out var pickupInteract))
                    _resolver.Inject(pickupInteract);

                pickupComponent.TryGetComponent<IInteractable>(out var interactable);

                var lifetime = new WorldLifetime(
                    pickupComponent.gameObject,
                    info);

                lifetime.Add(_pickups.Register(pickup));

                if (interactable != null)
                {
                    lifetime.Add(_interactions.RegisterInteractable(
                        request.WorldId,
                        interactable));
                }

                return lifetime;
            }
        }
    }
}