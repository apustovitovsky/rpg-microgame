using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IActorFactory
    {
        LifetimeScope Create(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}
