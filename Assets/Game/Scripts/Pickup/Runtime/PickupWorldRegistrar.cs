using System;
using Game.Interaction;
using Game.Targeting;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupWorldRegistrar
    {
        private readonly IWorldRegistry<IWorldPickup> _pickups;
        private readonly IWorldRegistry<IDisplayInfo> _displayInfos;
        private readonly IWorldRegistry<IWorldSpatial> _spatials;
        private readonly IWorldRegistry<IInteractable> _interactions;
        private readonly IWorldRegistry<ITargetable> _targets;

        public PickupWorldRegistrar(
            IWorldRegistry<IWorldPickup> pickups,
            IWorldRegistry<IDisplayInfo> displayInfos,
            IWorldRegistry<IWorldSpatial> spatials,
            IWorldRegistry<IInteractable> interactions,
            IWorldRegistry<ITargetable> targets)
        {
            _pickups = pickups;
            _displayInfos = displayInfos;
            _spatials = spatials;
            _interactions = interactions;
            _targets = targets;
        }

        public IRegistrationToken Register(PickupSpawnedObject pickup)
        {
            if (pickup == null)
                throw new ArgumentNullException(nameof(pickup));

            var lifetime = new CompositeRegistration();

            lifetime.Add(_pickups.Register(pickup.WorldId, pickup.Pickup));
            lifetime.Add(_displayInfos.Register(pickup.WorldId, pickup.DisplayInfo));
            lifetime.Add(_spatials.Register(pickup.WorldId, pickup.Spatial));

            if (pickup.Interaction != null)
                lifetime.Add(_interactions.Register(pickup.WorldId, pickup.Interaction));

            if (pickup.Target != null)
                lifetime.Add(_targets.Register(pickup.WorldId, pickup.Target));

            return lifetime;
        }
    }
}