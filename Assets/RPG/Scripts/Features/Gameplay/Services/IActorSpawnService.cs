using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IActorSpawnService
    {
        LifetimeScope Spawn(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
        LifetimeScope SpawnAndPossess(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}
