using Game.World;
using UnityEngine;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        WorldActor Spawn(
            WorldId worldId,
            string displayName,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}