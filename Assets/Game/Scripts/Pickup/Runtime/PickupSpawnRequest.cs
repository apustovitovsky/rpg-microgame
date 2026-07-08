using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    public readonly struct PickupSpawnRequest
    {
        public PickupSpawnRequest(
            WorldId worldId,
            PickupDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            WorldId = worldId;
            Definition = definition;
            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public WorldId WorldId { get; }

        public PickupDefinition Definition { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}