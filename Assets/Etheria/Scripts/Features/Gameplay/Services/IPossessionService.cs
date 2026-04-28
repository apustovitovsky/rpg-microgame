using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    public interface IPossessionService
    {
        void Possess(LifetimeScope actor);
        void Unpossess();
    }
}

