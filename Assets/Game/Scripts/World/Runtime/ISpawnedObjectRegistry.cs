using System;


namespace Game.World
{
    public interface ISpawnedObjectRegistry
    {
        bool Track(ISpawnedObject spawnedObject);

        bool TryGet(
            Guid instanceId,
            out ISpawnedObject spawnedObject);

        bool TryGetInstance<TInstance>(
            Guid instanceId,
            out TInstance instance)
            where TInstance : class, IWorldInstance;

        bool Despawn(Guid instanceId);

        void DespawnAll();
    }
}