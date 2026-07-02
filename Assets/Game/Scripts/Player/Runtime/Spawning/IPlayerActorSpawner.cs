using Game.Actor;
using UnityEngine;

namespace Game.Player
{
    public interface IPlayerActorSpawner
    {
        IActorView Spawn(
            string actorId,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}