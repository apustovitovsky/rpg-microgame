using System;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    public sealed class PickupFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IInteractionRegistrationService _interactions;
        private readonly IPickupService _pickupService;

        public PickupFactory(
            LifetimeScope parentScope,
            IPickupService pickupService,
            IInteractionRegistrationService interactions)
        {
            _parentScope = parentScope;
            _pickupService = pickupService;
            _interactions = interactions;
        }

        public IWorldObject Create(PickupSpawnRequest request)
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

                pickupComponent.Initialize(
                    request.WorldId,
                    request.Definition);

                var targetable = instance.GetComponentInChildren<Targetable>(true);

                if (targetable != null)
                    targetable.Initialize(info);

                if (pickupComponent.TryGetComponent<PickupInteractable>(out var pickupInteract))
                    pickupInteract.Initialize(_pickupService);

                pickupComponent.TryGetComponent<IInteractable>(out var interactable);

                var lifetime = new WorldObject(
                    pickupComponent.gameObject,
                    info);

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