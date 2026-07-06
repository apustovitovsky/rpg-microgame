using Game.Interaction;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    public sealed class WorldPickup : IWorldObject
    {
        public WorldPickup(
            WorldId worldId,
            string displayName,
            Transform root,
            IPickup pickup,
            IInteractable interaction)
        {
            WorldId = worldId;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? worldId.ToString()
                : displayName.Trim();

            Root = root;
            Pickup = pickup;
            Interaction = interaction;
        }

        public WorldId WorldId { get; }
        public string DisplayName { get; }
        public Transform Root { get; }

        public IPickup Pickup { get; }
        public IInteractable Interaction { get; }

        public bool TryGet<TEndpoint>(out TEndpoint endpoint)
            where TEndpoint : class
        {
            endpoint = null;

            if (this is TEndpoint self)
                endpoint = self;
            else if (Pickup is TEndpoint pickup)
                endpoint = pickup;
            else if (Interaction is TEndpoint interaction)
                endpoint = interaction;

            return endpoint != null;
        }
    }
}