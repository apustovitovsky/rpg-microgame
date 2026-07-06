using Game.Actor;
using UnityEngine;

namespace Game.Player
{
    public interface IPlayerActorSpawner
    {
        ActorInstance Spawn(
            string instanceId,
            string definitionId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}