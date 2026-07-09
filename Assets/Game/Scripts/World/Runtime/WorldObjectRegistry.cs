using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldObjectRegistry
    {
        bool TryGetInfo(
            WorldId worldId,
            out WorldInfo info);

        bool Track(IWorldObject lifetime);

        bool Despawn(WorldId worldId);

        void DespawnAll();
    }

    public sealed class WorldObjectRegistry : IWorldObjectRegistry
    {
        private readonly Dictionary<WorldId, IWorldObject> _entries = new();

        public bool Track(IWorldObject lifetime)
        {
            if (lifetime == null ||
                lifetime.WorldId.IsEmpty ||
                lifetime.IsDisposed)
            {
                lifetime?.Dispose();
                return false;
            }

            if (_entries.ContainsKey(lifetime.WorldId))
            {
                lifetime.Dispose();
                return false;
            }

            _entries.Add(
                lifetime.WorldId,
                lifetime);

            lifetime.Add(new LifetimeToken(() =>
            {
                _entries.Remove(lifetime.WorldId);
            }));

            return true;
        }

        public bool Despawn(WorldId worldId)
        {
            if (worldId.IsEmpty)
                return false;

            if (!_entries.TryGetValue(worldId, out var lifetime))
                return false;

            lifetime.Dispose();
            return true;
        }

        public void DespawnAll()
        {
            var worldIds = new List<WorldId>(_entries.Keys);

            foreach (var worldId in worldIds)
                Despawn(worldId);
        }

        public bool TryGetInfo(
            WorldId worldId,
            out WorldInfo info)
        {
            info = default;

            if (worldId.IsEmpty)
                return false;

            if (!_entries.TryGetValue(worldId, out var lifetime))
                return false;

            info = lifetime.Info;
            return true;
        }
    }
}