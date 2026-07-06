using UnityEngine;

namespace Game.Actor
{
    public interface IActorSpawner
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