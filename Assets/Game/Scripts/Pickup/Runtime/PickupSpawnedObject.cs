using Game.Interaction;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupSpawnedObject
    {
        public PickupSpawnedObject(
            WorldId worldId,
            IWorldPickup pickup,
            IDisplayable displayInfo,
            IInteractable interaction)
        {
            WorldId = worldId;
            Pickup = pickup;
            DisplayInfo = displayInfo;
            Interaction = interaction;
        }

        public WorldId WorldId { get; }

        public IWorldPickup Pickup { get; }

        public IDisplayable DisplayInfo { get; }

        public IInteractable Interaction { get; }
    }
}