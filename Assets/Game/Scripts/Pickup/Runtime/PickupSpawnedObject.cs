using Game.Interaction;
using Game.Targeting;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupSpawnedObject
    {
        public PickupSpawnedObject(
            IWorldHandle handle,
            IWorldPickup pickup,
            IDisplayInfo displayInfo,
            IWorldSpatial spatial,
            IInteractable interaction,
            ITargetable target)
        {
            Handle = handle;
            Pickup = pickup;
            DisplayInfo = displayInfo;
            Spatial = spatial;
            Interaction = interaction;
            Target = target;
        }

        public WorldId WorldId => Handle.WorldId;

        public IWorldHandle Handle { get; }

        public IWorldPickup Pickup { get; }

        public IDisplayInfo DisplayInfo { get; }

        public IWorldSpatial Spatial { get; }

        public IInteractable Interaction { get; }

        public ITargetable Target { get; }
    }
}