using UnityEngine;

namespace Game.World
{
    public interface IWorldObject
    {
        WorldId WorldId { get; }
        Transform Root { get; }

        bool TryGet<TEndpoint>(out TEndpoint endpoint)
            where TEndpoint : class;
    }

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
    }
}