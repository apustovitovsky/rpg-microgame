using UnityEngine;

namespace Game.World
{
    public interface IWorldObject
    {
        WorldId WorldId { get; }

        GameObject GameObject { get; }

        Transform Root { get; }
    }

    public sealed class WorldObject : IWorldObject
    {
        public WorldObject(
            WorldId worldId,
            GameObject gameObject)
        {
            WorldId = worldId;
            GameObject = gameObject;
        }

        public WorldId WorldId { get; }

        public GameObject GameObject { get; }

        public Transform Root => GameObject.transform;
    }
}