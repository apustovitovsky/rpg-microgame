using System.Collections.Generic;

namespace Game.World
{
    public interface IWorldObjectRegistry
    {
        bool TryGet(WorldId id, out IWorldObject obj);

        bool TryGetEndpoint<TEndpoint>(
            WorldId id,
            out TEndpoint endpoint)
            where TEndpoint : class;
    }

    public interface IWorldObjectRegistryWriter
    {
        void Register(IWorldObject obj);

        void Unregister(IWorldObject obj);

        bool Unregister(WorldId id);
    }
    
    public sealed class WorldObjectRegistry :
        IWorldObjectRegistry,
        IWorldObjectRegistryWriter
    {
        private readonly Dictionary<WorldId, IWorldObject> _objects = new();

        public bool TryGet(
            WorldId id,
            out IWorldObject obj)
        {
            obj = null;

            if (id.IsEmpty)
                return false;

            return _objects.TryGetValue(id, out obj);
        }

        public bool TryGetEndpoint<TEndpoint>(
            WorldId id,
            out TEndpoint endpoint)
            where TEndpoint : class
        {
            endpoint = null;

            if (!TryGet(id, out var obj))
                return false;

            return obj.TryGet(out endpoint);
        }

        public void Register(IWorldObject obj)
        {
            if (obj == null ||
                obj.WorldId.IsEmpty)
            {
                return;
            }

            _objects[obj.WorldId] = obj;
        }

        public void Unregister(IWorldObject obj)
        {
            if (obj == null ||
                obj.WorldId.IsEmpty)
            {
                return;
            }

            if (_objects.TryGetValue(obj.WorldId, out var existing) &&
                ReferenceEquals(existing, obj))
            {
                _objects.Remove(obj.WorldId);
            }
        }

        public bool Unregister(WorldId id)
        {
            if (id.IsEmpty)
                return false;

            return _objects.Remove(id);
        }
    }
}