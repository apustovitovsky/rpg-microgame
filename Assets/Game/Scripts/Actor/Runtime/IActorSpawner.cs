using Game.World;
using UnityEngine;

namespace Game.Actor
{
    public interface IActorSpawner
    {
        IWorldObject Spawn(
            WorldId worldId,
            string displayName,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);
    }
}