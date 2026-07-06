using Game.Actor;
using Game.World;
using UnityEngine;

namespace Game.Player
{
    public interface IPlayerActorSpawner
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