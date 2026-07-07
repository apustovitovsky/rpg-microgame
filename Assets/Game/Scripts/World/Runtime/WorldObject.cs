using UnityEngine;

namespace Game.World
{
    public sealed class WorldObject : IWorldObject
    {
        private readonly IWorldCapabilityProvider _capabilities;

        public WorldObject(
            WorldId worldId,
            GameObject gameObject,
            IWorldCapabilityProvider capabilities)
        {
            WorldId = worldId;
            GameObject = gameObject;
            _capabilities = capabilities;
        }

        public WorldId WorldId { get; }

        public GameObject GameObject { get; }

        public Transform Root => GameObject.transform;

        public bool TryGet<TCapability>(out TCapability capability)
            where TCapability : class
        {
            if (_capabilities == null)
            {
                capability = null;
                return false;
            }

            return _capabilities.TryGet(out capability);
        }
    }
}