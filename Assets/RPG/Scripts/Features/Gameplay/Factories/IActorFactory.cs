using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IActorFactory
    {
        Actor Create(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}
