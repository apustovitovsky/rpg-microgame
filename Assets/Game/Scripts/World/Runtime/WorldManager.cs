using System.Collections.Generic;


namespace Game.World
{
    public interface IWorldManager
    {
        bool Track(
            IWorldObject worldObject,
            IRegistrationToken lifetime);

        bool TryGetObject(
            WorldId worldId,
            out IWorldObject worldObject);

        bool Despawn(WorldId worldId);

        void DespawnAll();
    }

    public sealed class WorldManager : IWorldManager
    {
        private readonly Dictionary<WorldId, WorldEntry> _entries = new();

        public bool Track(
            IWorldObject worldObject,
            IRegistrationToken lifetime)
        {
            if (worldObject == null ||
                worldObject.WorldId.IsEmpty)
            {
                lifetime?.Dispose();
                return false;
            }

            if (_entries.ContainsKey(worldObject.WorldId))
            {
                lifetime?.Dispose();
                return false;
            }

            _entries.Add(
                worldObject.WorldId,
                new WorldEntry(
                    worldObject,
                    lifetime));

            return true;
        }

        public bool TryGetObject(
            WorldId worldId,
            out IWorldObject worldObject)
        {
            worldObject = null;

            if (worldId.IsEmpty)
                return false;

            if (!_entries.TryGetValue(worldId, out var entry))
                return false;

            worldObject = entry.WorldObject;
            return true;
        }
        
        public bool Despawn(WorldId worldId)
        {
            if (worldId.IsEmpty)
                return false;

            if (!_entries.Remove(worldId, out var entry))
                return false;

            entry.Lifetime?.Dispose();

            if (entry.WorldObject.GameObject != null)
                UnityEngine.Object.Destroy(entry.WorldObject.GameObject);

            return true;
        }

        public void DespawnAll()
        {
            var worldIds = new List<WorldId>(_entries.Keys);

            foreach (var worldId in worldIds)
                Despawn(worldId);
        }

        private readonly struct WorldEntry
        {
            public WorldEntry(
                IWorldObject worldObject,
                IRegistrationToken lifetime)
            {
                WorldObject = worldObject;
                Lifetime = lifetime;
            }

            public IWorldObject WorldObject { get; }

            public IRegistrationToken Lifetime { get; }
        }
    }
}