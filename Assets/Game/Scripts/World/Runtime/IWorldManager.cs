using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldManager
    {
        bool TryGetInfo(
            WorldId worldId,
            out WorldInfo info);

        bool Track(IWorldLifetime lifetime);

        bool Despawn(WorldId worldId);

        void DespawnAll();
    }

    public sealed class WorldManager : IWorldManager
    {
        private readonly Dictionary<WorldId, IWorldLifetime> _lifetimes = new();

        public bool Track(IWorldLifetime lifetime)
        {
            if (lifetime == null ||
                lifetime.WorldId.IsEmpty ||
                lifetime.IsDisposed)
            {
                lifetime?.Dispose();
                return false;
            }

            if (_lifetimes.ContainsKey(lifetime.WorldId))
            {
                lifetime.Dispose();
                return false;
            }

            _lifetimes.Add(
                lifetime.WorldId,
                lifetime);

            lifetime.Add(new LifetimeToken(() =>
            {
                _lifetimes.Remove(lifetime.WorldId);
            }));

            return true;
        }

        public bool Despawn(WorldId worldId)
        {
            if (worldId.IsEmpty)
                return false;

            if (!_lifetimes.TryGetValue(worldId, out var lifetime))
                return false;

            lifetime.Dispose();
            return true;
        }

        public void DespawnAll()
        {
            var worldIds = new List<WorldId>(_lifetimes.Keys);

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

            if (!_lifetimes.TryGetValue(worldId, out var lifetime))
                return false;

            info = lifetime.Info;
            return true;
        }
    }
}