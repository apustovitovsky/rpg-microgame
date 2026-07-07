using Game.World;
using UnityEngine;

namespace Game.Actor
{
    public readonly struct ActorSpawnRequest
    {
        public ActorSpawnRequest(
            WorldId worldId,
            string displayName,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            WorldId = worldId;
            DisplayName = displayName;
            Prefab = prefab;
            Position = position;
            Rotation = rotation;
            Parent = parent;
        }

        public WorldId WorldId { get; }

        public string DisplayName { get; }

        public GameObject Prefab { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Transform Parent { get; }
    }
}