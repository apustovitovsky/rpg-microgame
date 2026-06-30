using UnityEngine;

namespace Etheria.Game.World
{
    public sealed class NavigationNode
    {
        public string Id { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public float Radius { get; }
        public NavigationFlag Flags { get; }

        public NavigationNode(
            string id,
            Vector3 position,
            Quaternion rotation,
            float radius,
            NavigationFlag flags)
        {
            Id = id?.Trim() ?? string.Empty;
            Position = position;
            Rotation = rotation;
            Radius = radius;
            Flags = flags;
        }
    }
}