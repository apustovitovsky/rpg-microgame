using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public interface IWorldManager
    {
        bool Register(IWorldObject worldObject);

        bool Despawn(WorldId worldId);
        void DespawnAll();
    }
    
    public sealed class WorldManager : IWorldManager
    {
        private readonly IWorldObjectRegistryWriter _registry;
        private readonly Dictionary<WorldId, IWorldObject> _objects = new();

        public WorldManager(IWorldObjectRegistryWriter registry)
        {
            _registry = registry;
        }

        public bool Register(IWorldObject worldObject)
        {
            if (worldObject == null ||
                worldObject.WorldId.IsEmpty)
            {
                return false;
            }

            if (_objects.ContainsKey(worldObject.WorldId))
                return false;

            _objects.Add(
                worldObject.WorldId,
                worldObject);

            _registry.Register(worldObject);

            return true;
        }

        public bool Despawn(WorldId worldId)
        {
            if (worldId.IsEmpty)
                return false;

            if (!_objects.TryGetValue(worldId, out var worldObject))
                return false;

            _objects.Remove(worldId);
            _registry.Unregister(worldObject);

            if (worldObject.GameObject != null)
                Object.Destroy(worldObject.GameObject);

            return true;
        }

        public void DespawnAll()
        {
            var worldIds = new List<WorldId>(_objects.Keys);

            foreach (var worldId in worldIds)
                Despawn(worldId);

            _objects.Clear();
        }
    }
}