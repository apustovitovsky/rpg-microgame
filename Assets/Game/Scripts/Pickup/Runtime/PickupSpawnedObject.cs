using Game.Interaction;
using Game.Targeting;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupSpawnedObject
    {
        public PickupSpawnedObject(
            IWorldObject worldObject,
            IWorldPickup pickup,
            IDisplayInfo displayInfo,
            IWorldSpatial spatial,
            IInteractable interaction,
            ITargetable target)
        {
            WorldObject = worldObject;
            Pickup = pickup;
            DisplayInfo = displayInfo;
            Spatial = spatial;
            Interaction = interaction;
            Target = target;
        }

        public WorldId WorldId => WorldObject.WorldId;

        public IWorldObject WorldObject { get; }

        public IWorldPickup Pickup { get; }

        public IDisplayInfo DisplayInfo { get; }

        public IWorldSpatial Spatial { get; }

        public IInteractable Interaction { get; }

        public ITargetable Target { get; }
    }
}