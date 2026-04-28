using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    public interface IActorFactory
    {
        LifetimeScope Create(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}

