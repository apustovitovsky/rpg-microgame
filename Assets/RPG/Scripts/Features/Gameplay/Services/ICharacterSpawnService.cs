using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface ICharacterSpawnService
    {
        LifetimeScope SpawnPlayer(Vector3 position);
        LifetimeScope SpawnTurret(Vector3 position);
        LifetimeScope SpawnDrone(Vector3 position);
    }
}
