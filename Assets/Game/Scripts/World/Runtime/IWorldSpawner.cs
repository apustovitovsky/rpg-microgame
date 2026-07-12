using System;

namespace Game.World
{
    public interface IWorldSpawner
    {
        ISpawnedObject Spawn<TInstance>(
            SpawnRequest<TInstance> request)
            where TInstance : class, IWorldInstance;

        bool Despawn(Guid instanceId);
    }
}