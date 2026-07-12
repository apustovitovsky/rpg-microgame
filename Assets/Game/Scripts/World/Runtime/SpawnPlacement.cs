using UnityEngine;

namespace Game.World
{
    public readonly struct SpawnPlacement
    {
        public SpawnPlacement(
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}