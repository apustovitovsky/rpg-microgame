using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IActorSpawnService
    {
        Actor Spawn(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
        Actor SpawnAndPossess(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}
