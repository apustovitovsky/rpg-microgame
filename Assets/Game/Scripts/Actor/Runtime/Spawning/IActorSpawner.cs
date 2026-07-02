using UnityEngine;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        IActorView Spawn(
            string actorId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}