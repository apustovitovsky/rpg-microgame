using UnityEngine;

namespace RPG.Gameplay
{
    public interface IActorFactory
    {
        GameObject Create(GameObject prefab, Vector3 position, Quaternion rotation = default);
    }
}
