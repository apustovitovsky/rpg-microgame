using UnityEngine;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    public interface IActorFactory
    {
        LifetimeScope Create(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}
