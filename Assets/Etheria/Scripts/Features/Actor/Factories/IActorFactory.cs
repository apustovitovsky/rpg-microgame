using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public interface IActorFactory
    {
        LifetimeScope Create(LifetimeScope prefab, Vector3 position, Quaternion rotation = default);
    }
}

