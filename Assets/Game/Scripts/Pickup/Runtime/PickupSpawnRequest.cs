using System;
using UnityEngine;

namespace Game.Pickup
{
    public readonly struct PickupSpawnRequest
    {
        public PickupSpawnRequest(
            Guid instanceId,
            PickupDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Pickup instance id is required.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));

            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public Guid InstanceId { get; }

        public PickupDefinition Definition { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}