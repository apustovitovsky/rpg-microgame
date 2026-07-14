using System;
using UnityEngine;
using VContainer.Unity;

namespace Game.World
{
    public interface IWorldSpawner
    {
        GameObject Spawn(
            Guid instanceId,
            GameObject prefab,
            SpawnPlacement placement,
            IInstaller installer);

        bool Despawn(Guid instanceId);
    }
}