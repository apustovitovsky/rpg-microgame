using UnityEngine;

namespace Game.World
{
    public interface IWorldHandle
    {
        WorldId WorldId { get; }

        GameObject GameObject { get; }
    }

    public sealed class WorldHandle : IWorldHandle
    {
        public WorldHandle(
            WorldId worldId,
            GameObject gameObject)
        {
            WorldId = worldId;
            GameObject = gameObject;
        }

        public WorldId WorldId { get; }

        public GameObject GameObject { get; }
    }
}