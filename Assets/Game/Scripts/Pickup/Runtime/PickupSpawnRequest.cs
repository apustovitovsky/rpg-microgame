using System;
using UnityEngine;

namespace Game.Pickup
{
    public readonly struct PickupSpawnRequest
    {
        public PickupSpawnRequest(
            PickupInstance instance,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public PickupInstance Instance { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}