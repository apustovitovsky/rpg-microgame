using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldManager
    {
        bool Track(
            IWorldHandle handle,
            IRegistrationToken lifetime);

        bool TryGetHandle(
            WorldId worldId,
            out IWorldHandle handle);

        bool Despawn(WorldId worldId);

        void DespawnAll();
    }

    public sealed class WorldManager : IWorldManager
    {
        private readonly Dictionary<WorldId, WorldEntry> _entries = new();

        public bool Track(
            IWorldHandle handle,
            IRegistrationToken lifetime)
        {
            if (handle == null ||
                handle.WorldId.IsEmpty)
            {
                lifetime?.Dispose();
                return false;
            }

            if (_entries.ContainsKey(handle.WorldId))
            {
                lifetime?.Dispose();
                return false;
            }

            _entries.Add(
                handle.WorldId,
                new WorldEntry(
                    handle,
                    lifetime));

            return true;
        }

        public bool TryGetHandle(
            WorldId worldId,
            out IWorldHandle handle)
        {
            handle = null;

            if (worldId.IsEmpty)
                return false;

            if (!_entries.TryGetValue(worldId, out var entry))
                return false;

            handle = entry.Handle;
            return true;
        }

        public bool Despawn(WorldId worldId)
        {
            if (worldId.IsEmpty)
                return false;

            if (!_entries.Remove(worldId, out var entry))
                return false;

            entry.Lifetime?.Dispose();

            if (entry.Handle.GameObject != null)
                UnityEngine.Object.Destroy(entry.Handle.GameObject);

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
                IWorldHandle handle,
                IRegistrationToken lifetime)
            {
                Handle = handle;
                Lifetime = lifetime;
            }

            public IWorldHandle Handle { get; }

            public IRegistrationToken Lifetime { get; }
        }
    }
}