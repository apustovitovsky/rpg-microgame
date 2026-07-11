using System;
using UnityEngine;

namespace Game.Loot
{
    public readonly struct LootContainerSpawnRequest
    {
        public LootContainerSpawnRequest(
            LootContainerInstance instance,
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

        public LootContainerInstance Instance { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}