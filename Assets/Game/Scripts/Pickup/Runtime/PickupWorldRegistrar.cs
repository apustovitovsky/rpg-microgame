using System;
using Game.Interaction;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupWorldRegistrar
    {
        private readonly IWorldRegistry<IWorldPickup> _pickups;
        private readonly IWorldRegistry<IDisplayable> _displayInfos;
        private readonly IWorldRegistry<IInteractable> _interactions;

        public PickupWorldRegistrar(
            IWorldRegistry<IWorldPickup> pickups,
            IWorldRegistry<IDisplayable> displayInfos,
            IWorldRegistry<IInteractable> interactions)
        {
            _pickups = pickups;
            _displayInfos = displayInfos;
            _interactions = interactions;
        }

        public IRegistrationToken Register(PickupSpawnedObject pickup)
        {
            if (pickup == null)
                throw new ArgumentNullException(nameof(pickup));

            var lifetime = new CompositeRegistration();

            lifetime.Add(_pickups.Register(pickup.WorldId, pickup.Pickup));
            lifetime.Add(_displayInfos.Register(pickup.WorldId, pickup.DisplayInfo));

            if (pickup.Interaction != null)
                lifetime.Add(_interactions.Register(pickup.WorldId, pickup.Interaction));

            return lifetime;
        }
    }
}